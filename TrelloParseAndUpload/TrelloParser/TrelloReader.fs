module TrelloReader

open System
open System.Globalization
open System.Text.RegularExpressions
open FSharp.Data

type TrelloMember = {
    Id : string
    Name : string
    Email : string
    UserName : string
    ImageUrl : string
}

type TrelloCard = {
    SpeakerName : string
    SpeakerEmail : string 
    TalkData : string 
    AdminId : string
    ExtraInfo : string 
    RawInput : string
}

[<Serializable>]
type CardTitle = 
    | Unmatched of string
    | EventDate of string
    | Template of string
    | TalkCard of TrelloCard

type BoardMetaData = {
    Name : string
    ShortUrl : string
    DateLastActivity : DateTime
    DateLastView : DateTime
    Members : TrelloMember []
    Cards : TrelloCard []
}

[<Literal>]
let TrelloExport = @"bristech-speakers-export.json"

type Board = JsonProvider<TrelloExport>

module private Helpers = 

    let createImageUrl (avatarHash : Guid) =
        //sprintf "https://trello-avatars.s3.amazonaws.com/%s/original.png" <| avatarHash.ToString("N")
        sprintf "https://trello-avatars.s3.amazonaws.com/%s/50.png" <| avatarHash.ToString("N")

    let getImageUrl avatarHash = 
        match avatarHash with 
        | Some hash -> createImageUrl hash
        | None -> "https://placebear.com/50/50"

    let idIsNotForIgnoredAdmin (ignoredAdmins : TrelloMember []) id= 
        ignoredAdmins 
        |> Array.exists (fun x -> x.Id = id) 
        |> not


    let parseWasValid (groups : GroupCollection) = 
        groups.Count = 5 && not <| String.IsNullOrWhiteSpace (groups.[1].Value)

    let tryPopulateSpeakerData (groups : GroupCollection) (card : Board.Card) ignoredAdmins (defaultAdmin : TrelloMember)= 
        if parseWasValid groups then
            TalkCard { 
                SpeakerName = groups.[1].Value
                SpeakerEmail = groups.[2].Value
                TalkData = groups.[3].Value
                ExtraInfo = groups.[4].Value
                RawInput = groups.[0].Value
                AdminId = card.IdMembers 
                    |> Array.tryFind (fun id -> idIsNotForIgnoredAdmin ignoredAdmins id )
                    |> function 
                        | Some id -> id
                        | None -> defaultAdmin.Id
                }
        else 
            Unmatched card.Name

    let nameContainsDate (cardName: string) = 
        let monthNames = 
            DateTimeFormatInfo.CurrentInfo.MonthNames 
            |> Array.map (fun x -> x.ToUpperInvariant())
            |> Array.filter (String.IsNullOrWhiteSpace >> not)
        let cardNameUpped = cardName.ToUpperInvariant()
        monthNames 
        |> Array.exists (fun month -> cardNameUpped.Contains(month))

    let (|AllRegexGroups|_|) pattern input = 
        let m = Regex.Match(input, pattern)
        if (m.Success) then Some m.Groups else None

    let parseCard (card : Board.Card) ignoredAdmins defaultAdmin= 
        match card.Name with
        | template when card.Name.ToUpperInvariant().Contains("TEMPLATE") -> Template template
        | dateCard when nameContainsDate card.Name -> EventDate dateCard
        | AllRegexGroups "(.*)\[(.*)\]\((.*)\)(.*)$" groups -> tryPopulateSpeakerData groups card ignoredAdmins defaultAdmin
        | _ -> Unmatched card.Name

    let parseEmail (fullName : string) = 
        let split = fullName.Split()
        match split.Length with 
        | 1  -> 
            split.[0] + "@scottlogic.co.uk"
        | x when x > 1 -> 
            let firstNameFirstLetter = split.[0].Chars(0)
            let lastName = (Array.last split).ToLowerInvariant()
            sprintf "%c%s@scottlogic.co.uk" firstNameFirstLetter lastName
        | _ -> "missingEmail@scottlogic.co.uk" 

    let ignoredAdminUserNames = ["samdavies";"jamesphillpotts";"tamarachehayebmakarem1";"tamaramakarem";"nicholashemley"]

let parseBoard (board : Board.Root) =     
    let ignoredAdmins, admins = 
        board.Members |> Array.map (fun record -> 
                              { TrelloMember.Id = record.Id
                                Name = record.FullName
                                Email = Helpers.parseEmail record.FullName
                                UserName = record.Username
                                ImageUrl = Helpers.getImageUrl record.AvatarHash })
                        |> Array.partition(fun admin ->
                            List.contains admin.UserName Helpers.ignoredAdminUserNames)

    let defaultAdmin = admins |> Array.find (fun x -> x.UserName = "chrissmith58")

    let allCardData = 
        board.Cards
        |> Array.filter(fun x -> not x.Closed)
        |> Array.map(fun card -> Helpers.parseCard card ignoredAdmins defaultAdmin)

    let talkCards = allCardData |> Array.choose (function | TalkCard data -> Some data | _ -> None)

    let boardMeta = {
        Name = board.Name
        ShortUrl = board.ShortUrl
        DateLastActivity = board.DateLastActivity
        DateLastView = board.DateLastView
        Members = admins
        Cards = talkCards
    }
    boardMeta




