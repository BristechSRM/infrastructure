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
    ExtraInfo : string 
    RawInput : string
    CardId : string
    AdminId : string
    AdminEmail : string
}

type BoardMetaData = {
    Name : string
    ShortUrl : string
    DateLastActivity : DateTime
    DateLastView : DateTime
    Members : TrelloMember []
    Cards : TrelloCard []
}

[<Literal>]
let TrelloExport = @"../bristech-speakers-export.json"

type Board = JsonProvider<TrelloExport>

type RawTrelloCard = {
    Card : Board.Card
    Groups : GroupCollection 
}  

[<Serializable>]
type CardType = 
    | Unmatched of Board.Card
    | EventDate of Board.Card
    | Template of Board.Card
    | UnparsedTalkCard of RawTrelloCard
    | TalkCard of TrelloCard

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

    let tryParseCard (admins : TrelloMember []) ignoredAdmins (defaultAdmin : TrelloMember) (card : Board.Card) (groups : GroupCollection) = 
        if parseWasValid groups then
            let adminId = 
                card.IdMembers 
                    |> Array.tryFind (fun id -> idIsNotForIgnoredAdmin ignoredAdmins id )
                    |> function 
                        | Some id -> id
                        | None -> defaultAdmin.Id
            let admin = admins |> Array.pick (fun memb -> if memb.Id = adminId then Some memb else None ) 
            TalkCard { 
                SpeakerName = groups.[1].Value
                SpeakerEmail = groups.[2].Value
                TalkData = groups.[3].Value
                ExtraInfo = groups.[4].Value
                RawInput = groups.[0].Value
                CardId = card.Id
                AdminId = adminId
                AdminEmail = admin.Email
                }
        else 
            Unmatched card

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

let (|AllRegexGroups|_|) pattern input = 
    let m = Regex.Match(input, pattern)
    if (m.Success) then Some m.Groups else None

let (|AllRegexGroupsMultiLine|_|) pattern input = 
    let m = Regex.Match(input, pattern,RegexOptions.Singleline)
    if (m.Success) then Some m.Groups else None

let categorizeCard (card : Board.Card)= 
    let nameContainsDate (cardName: string) = 
        let monthNames = 
            DateTimeFormatInfo.CurrentInfo.MonthNames 
            |> Array.map (fun x -> x.ToUpperInvariant())
            |> Array.filter (String.IsNullOrWhiteSpace >> not)
        let cardNameUpped = cardName.ToUpperInvariant()
        monthNames 
        |> Array.exists (fun month -> cardNameUpped.Contains(month))

    match card.Name with
    | template when card.Name.ToUpperInvariant().Contains("TEMPLATE") -> Template card
    | dateCard when nameContainsDate card.Name -> EventDate card
    | AllRegexGroups "(.*)\[(.*)\]\((.*)\)(.*)$" groups -> UnparsedTalkCard {Card = card; Groups= groups}
    | _ -> Unmatched card

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
        |> Array.map(categorizeCard)

    let talkCards = 
        allCardData 
        |> Array.choose (function 
            | UnparsedTalkCard rawCard -> 
                let parsedCard = Helpers.tryParseCard admins ignoredAdmins defaultAdmin rawCard.Card rawCard.Groups 
                match parsedCard with
                | TalkCard card -> Some card 
                | _ -> None
            | _ -> None)

    let boardMeta = {
        Name = board.Name
        ShortUrl = board.ShortUrl
        DateLastActivity = board.DateLastActivity
        DateLastView = board.DateLastView
        Members = admins
        Cards = talkCards
    }
    boardMeta




