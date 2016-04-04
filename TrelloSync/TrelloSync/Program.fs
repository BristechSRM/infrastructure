open System
open System.Configuration
open System.IO
open System.Net
open Newtonsoft.Json
open System.Globalization
open System.Text.RegularExpressions

[<AutoOpen>]
module Helpers = 
    type TrelloCredentials = 
        { Key : string
          Token : string } 

    let getTrelloCredentials() = {Key = ConfigurationManager.AppSettings.Item("TrelloKey") ; Token = ConfigurationManager.AppSettings.Item("TrelloToken")}

    let (|AllRegexGroups|_|) pattern input = 
        let m = Regex.Match(input, pattern)
        if (m.Success) then Some m.Groups else None

    let (|AllRegexGroupsMultiLine|_|) pattern input = 
        let m = Regex.Match(input, pattern,RegexOptions.Singleline)
        if (m.Success) then Some m.Groups else None

module Members =  
    type BasicMember = 
        { Id : string
          Username : string
          FullName : string
          AvatarHash : string }
    
    type TrelloMember = 
        { Id : string
          Name : string
          Email : string
          UserName : string
          ImageUrl : string }

    type MembersMeta = 
        { Members : TrelloMember []
          IgnoredMembers : TrelloMember []
          DefaultMember : TrelloMember }

    type MemberId = { Id: string}

    let parseToEmail (fullName : string) = 
        let split = fullName.Split()
        match split.Length with 
        | 1  -> 
            split.[0] + "@scottlogic.co.uk"
        | x when x > 1 -> 
            let firstNameFirstLetter = split.[0].Chars(0)
            let lastName = (Array.last split).ToLowerInvariant()
            sprintf "%c%s@scottlogic.co.uk" firstNameFirstLetter lastName
        | _ -> "missingEmail@scottlogic.co.uk" 

    let createImageUrl (avatarHash : string) =
        sprintf "https://trello-avatars.s3.amazonaws.com/%s/50.png" <| avatarHash

    let getImageUrl avatarHash = 
        match avatarHash with 
        | Some hash -> createImageUrl hash
        | None -> "https://placebear.com/50/50"

    let getMemberDetailsAsync trelloCred id =
        async {
            let uri = Uri(sprintf "https://api.trello.com/1/members/%s?fields=username,fullName,avatarHash&key=%s&token=%s" id trelloCred.Key trelloCred.Token)
            use webClient = new WebClient()
            let! rawMember = webClient.AsyncDownloadString(uri)
            let basicMember = JsonConvert.DeserializeObject<BasicMember>(rawMember)
            return {
                TrelloMember.Id = basicMember.Id
                Name = basicMember.FullName
                UserName = basicMember.Username
                Email = parseToEmail basicMember.FullName
                ImageUrl = getImageUrl <| Option.ofObj basicMember.AvatarHash
            }
        }

    let getMembersAsync trelloCred= 
        async {
            let uri = Uri(sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/members?fields=id&key=%s&token=%s" trelloCred.Key trelloCred.Token)
            use webClient = new WebClient()
            let! rawMemberIds = webClient.AsyncDownloadString(uri)
            let memberIds = JsonConvert.DeserializeObject<MemberId []>(rawMemberIds)
            let allMembersAsync = 
                memberIds 
                |> Array.map (fun mem -> getMemberDetailsAsync trelloCred mem.Id) 
                |> Async.Parallel                
                
            return! allMembersAsync
        }
    let ignoredAdminUserNames = ["samdavies";"jamesphillpotts";"tamarachehayebmakarem1";"tamaramakarem";"nicholashemley"] 
    let getAllMembersAsync trelloCred = 
        async {
            let! members = getMembersAsync trelloCred
            let ignoredMembers, keptMembers = 
                members 
                |> Array.partition(fun mem -> List.contains mem.UserName ignoredAdminUserNames)
            let defaultMember = members |> Array.find (fun x -> x.UserName = "chrissmith58")
            return {
                Members = keptMembers
                IgnoredMembers = ignoredMembers
                DefaultMember = defaultMember
            }
        }

module Cards = 
    type BasicCard =
        { Id : string
          Name : string
          IdMembers : string [] }

    type RawTrelloCard = 
        { Card : BasicCard
          Groups : GroupCollection }

    type TrelloCard = 
        { SpeakerName : string
          SpeakerEmail : string 
          TalkData : string 
          ExtraInfo : string 
          RawInput : string
          CardId : string
          AdminId : string
          AdminEmail : string }

    type CardType = 
        | Unmatched of BasicCard
        | EventDate of BasicCard
        | Template of BasicCard
        | UnparsedTalkCard of RawTrelloCard
        | TalkCard of TrelloCard

    let categorizeCard (card : BasicCard)= 
        let nameContainsDate (cardName: string) = 
            let monthNames = 
                DateTimeFormatInfo.CurrentInfo.MonthNames 
                |> Array.map (fun x -> x.ToUpperInvariant())
                |> Array.filter (String.IsNullOrWhiteSpace >> not)
            let cardNameUpped = cardName.ToUpperInvariant()
            monthNames 
            |> Array.exists (fun month -> cardNameUpped.Contains(month))

        match card.Name with
        | _ when card.Name.ToUpperInvariant().Contains("TEMPLATE") -> Template card
        | _ when nameContainsDate card.Name -> EventDate card
        | AllRegexGroups "(.*)\[(.*)\]\((.*)\)(.*)$" groups -> UnparsedTalkCard {Card = card; Groups= groups}
        | _ -> Unmatched card

    let getAllCards trelloCred = 
        async {
            let uri =  Uri(sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/cards/open?fields=id,name,idMembers&key=%s&token=%s" trelloCred.Key trelloCred.Token)
            use webClient = new WebClient()
            let! rawCards = webClient.AsyncDownloadString(uri)
            let basicCards = JsonConvert.DeserializeObject<BasicCard []>(rawCards)
            return basicCards |> Array.map(categorizeCard)        
        }

    let getAllRawTalkCards trelloCred = 
        async {
            let! allCards = getAllCards trelloCred
            return allCards |> Array.choose(function | UnparsedTalkCard rawTalkCard -> Some rawTalkCard | _ -> None)
        }

module Actions = 
    open Cards

    type ActionData = 
        { Text : string }

    type MemberCreator = 
        { Id : string
          FullName : string
          Username : string}

    type BasicAction = 
        { Id : string
          Data : ActionData
          Type : string
          MemberCreator : MemberCreator
          }

    type RawCardAndActions = 
        { Card : RawTrelloCard 
          Actions : BasicAction [] }

    //NOTE: Currently assuming that card will have less than 1000 actions. If a card has more than that, we need to deal with the trello paging to get them all
    let getCardCommentActionsAsync trelloCred (rawCard : RawTrelloCard) = 
        async {
            let uri = Uri(sprintf "https://api.trello.com/1/cards/%s/actions?filter=commentCard&key=%s&token=%s" rawCard.Card.Id trelloCred.Key trelloCred.Token)
            use webClient = new WebClient()
            let! rawCommentActions = webClient.AsyncDownloadString(uri)
            let basicCommentActions = JsonConvert.DeserializeObject<BasicAction []>(rawCommentActions)
            return {Card = rawCard; Actions = basicCommentActions;}
        }

    let getActionsPerCardAsync trelloCred (rawCards : RawTrelloCard []) =
        async {
            return! rawCards |> Array.map(getCardCommentActionsAsync trelloCred) |> Async.Parallel
        }

module CardParser = 
    open Members
    open Cards

    let idIsNotForIgnoredAdmin (ignoredAdmins : TrelloMember []) id= 
        ignoredAdmins 
        |> Array.exists (fun x -> x.Id = id) 
        |> not

    let parseWasValid (groups : GroupCollection) = 
        groups.Count = 5 && not <| String.IsNullOrWhiteSpace (groups.[1].Value)

    let tryParseCard members {Card=card;Groups=groups} = 
        if parseWasValid groups then
            let adminId = 
                card.IdMembers 
                    |> Array.tryFind (fun id -> idIsNotForIgnoredAdmin members.IgnoredMembers id )
                    |> function 
                        | Some id -> id
                        | None -> members.DefaultMember.Id
            let admin = members.Members |> Array.pick (fun memb -> if memb.Id = adminId then Some memb else None ) 
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

module CommsActionParser = 
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

module BoardParser = 
    open Members
    open Cards
    open Actions

    open CardParser
    open CommsActionParser

    type CardWithCorrs = 
        { TrelloCard : TrelloCard 
          Correspondance : Correspondance []}

    type TrelloBoard = 
        { Members : TrelloMember []
          Cards : CardWithCorrs [] }

    let tryParseCardAndActions members cas =
        let parsedCard = tryParseCard members cas.Card
        match parsedCard with
        | TalkCard card -> 
            let correspondance = cas.Actions |> Array.map tryParseCommentAction |> Array.choose (createCorrespondance card |> Option.map)
            Some {TrelloCard = card; Correspondance = correspondance} 
        | _ -> None

    let parseBoardAsync trelloCred =
        async {
            //StartChild lets you start multiple asyncs without blocking for the result
            let! cardsAsync = Async.StartChild <| getAllRawTalkCards trelloCred
            let! membersAsync = Async.StartChild <| getAllMembersAsync trelloCred

            //Now we block for the result
            let! cards = cardsAsync
            let! cardsAndCommentActions = getActionsPerCardAsync trelloCred cards
            let! members = membersAsync

            let talkCards = cardsAndCommentActions |> Array.choose (tryParseCardAndActions members)
            return {Members = members.Members; Cards = talkCards}
        }

[<EntryPoint>]
let main argv = 
    let trelloCred = getTrelloCredentials()
    let test = BoardParser.parseBoardAsync trelloCred |> Async.RunSynchronously
    let result = JsonConvert.SerializeObject(test, Formatting.Indented)
    File.WriteAllText(@"outlines-import.json",result)
    printfn "%A" argv
    0 
