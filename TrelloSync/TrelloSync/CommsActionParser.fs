module CommsActionParser

open System
open System.Text.RegularExpressions
open Cards
open Actions
open Serilog
open System.Globalization

type EmailDirection = 
    | Send
    | Receive

type EmailMeta = 
    { Date : DateTime
      Direction : EmailDirection
      Text : string }

type CommsItem = 
    { To : string
      From : string
      Date : DateTime
      Message : string }

let tryParseEmailDirection (dirString : string) = 
    match dirString.ToUpperInvariant() with
    | "SEND" -> Some Send
    | "RECEIVE" -> Some Receive
    | _ -> None

let (|AllRegexGroupsMultiLine|_|) pattern input = 
    let m = Regex.Match(input, pattern, RegexOptions.Singleline)
    if (m.Success) then Some m.Groups
    else None

let createCommsItem (groups : GroupCollection) speakerEmail adminEmail action = 
    let dateString = groups.[1].Value
    try
        let date = 
            //Workaround for data error + mono bug
            let success, exactDate =  DateTime.TryParseExact(dateString,[|"dd/mm/yy";"dd/mm/yyyy";"dd.mm.yy";"dd.mm.yyyy"|],CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal)
            if success then
                exactDate
            else 
                DateTime.Parse(dateString,CultureInfo.InvariantCulture)
        match tryParseEmailDirection groups.[2].Value with
        | Some dir -> 
            match dir with
            | Send -> 
                Some { To = speakerEmail
                       From = adminEmail
                       Date = date
                       Message = groups.[3].Value }
            | Receive -> 
                Some { To = adminEmail
                       From = speakerEmail
                       Date = date
                       Message = groups.[3].Value }
        | None -> 
            let message = sprintf "Parsing for direction for comms Action: %A failed" action
            Log.Error(message)
            failwith message
    with
        | ex -> 
            let message = sprintf "Parsing for date: %s for comms Action: %A failed with exception: %s" dateString action ex.Message
            Log.Error(message)
            failwith message

let tryCreateCommsItem (card : TrelloCard) (action : BasicAction) = 
    match action.Data.Text with
    | AllRegexGroupsMultiLine "\[([0-9\/\.]+) - ([a-zA-Z]+)\](.*)$" groups -> 
        match card.AdminEmail with
        | Some adminEmail -> createCommsItem groups card.SpeakerEmail adminEmail action
        | None -> 
            let message = sprintf "Card: %A has commms comments attached, but no admin or the admin has no email? Please review the card and try again" card
            Log.Error(message)
            failwith message
    | _ -> None
