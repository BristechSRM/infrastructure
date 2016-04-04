﻿module CommsActionParser
    open System
    open System.Text.RegularExpressions 
    open Cards
    open Actions

    type EmailDirection = 
        | Send
        | Recieve

    type EmailMeta = 
        { Date : DateTime
          Direction : EmailDirection
          Text : string
          MemberCreator : MemberCreator }

    type Correspondance = 
        { To : string
          From : string
          Date : DateTime
          Message : string }

    let tryParseEmailDirection (dirString : string) = 
        match dirString.ToUpperInvariant() with
        | "SEND" -> Some Send
        | "RECEIVE" | "RECIEVE" -> Some Recieve
        | _ -> None

    let tryParseCommsComment (action : BasicAction) (groups : GroupCollection) = 
        let success, date = DateTime.TryParse(groups.[1].Value)
        if success then 
            match tryParseEmailDirection groups.[2].Value with
            | Some dir ->                    
                Some {  Date = date
                        Direction = dir
                        Text = groups.[3].Value
                        MemberCreator = action.MemberCreator }
            | None -> None
        else None

    let tryParseCommentAction (action : BasicAction) = 
        match action.Data.Text with
        | AllRegexGroupsMultiLine "\[([0-9\/\.]+) - ([a-zA-Z]+)\](.*)$" groups -> 
            tryParseCommsComment action groups
        | _ -> None

    let createCorrespondance (card : TrelloCard) (em : EmailMeta) = 
        match em.Direction with
        | Send -> 
            { To = card.SpeakerEmail 
              From = card.AdminEmail
              Date = em.Date
              Message = em.Text }
        | Recieve -> 
            { To = card.AdminEmail
              From = card.SpeakerEmail
              Date = em.Date
              Message = em.Text }