open System
open System.Configuration
open System.Net
open Newtonsoft.Json
open System.Globalization
open System.Text.RegularExpressions

type TrelloCredentials = 
    { Key : string
      Token : string } 

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

    let getMemberDetails trelloCred id = 
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
            //sprintf "https://trello-avatars.s3.amazonaws.com/%s/original.png" <| avatarHash.ToString("N")
            sprintf "https://trello-avatars.s3.amazonaws.com/%s/50.png" <| avatarHash

        let getImageUrl avatarHash = 
            match avatarHash with 
            | Some hash -> createImageUrl hash
            | None -> "https://placebear.com/50/50"

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

    type MemberId = { Id: string}
    let getMembers trelloCred= 
        async {
            let uri = Uri(sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/members?fields=id&key=%s&token=%s" trelloCred.Key trelloCred.Token)
            use webClient = new WebClient()
            let! rawMemberIds = webClient.AsyncDownloadString(uri)
            let memberIds = JsonConvert.DeserializeObject<MemberId []>(rawMemberIds)
            let allMembersAsync = 
                memberIds 
                |> Array.map (fun mem -> getMemberDetails trelloCred mem.Id) 
                |> Async.Parallel                
                
            return! allMembersAsync
        }
    let ignoredAdminUserNames = ["samdavies";"jamesphillpotts";"tamarachehayebmakarem1";"tamaramakarem";"nicholashemley"] 
    let getAllMembers trelloCred = 
        async {
            let! members = getMembers trelloCred
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

    let (|AllRegexGroups|_|) pattern input = 
        let m = Regex.Match(input, pattern)
        if (m.Success) then Some m.Groups else None

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
        | template when card.Name.ToUpperInvariant().Contains("TEMPLATE") -> Template card
        | dateCard when nameContainsDate card.Name -> EventDate card
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

module BoardParser = 
    open Members
    open Cards

    type TrelloBoard = 
        { Members : TrelloMember []
          Cards : TrelloCard [] }

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

    let parseBoard (allMembersAsync : Async<Members.MembersMeta>) (allCardsAsync : Async<Cards.CardType []>) =
        async {
            let! cardsAsync = Async.StartChild allCardsAsync
            let! membersAsync = Async.StartChild allMembersAsync
            let! cards = cardsAsync
            let! members = membersAsync
            let talkCards = cards |> Array.choose(function 
                | UnparsedTalkCard rawCard -> 
                    let parsedCard = tryParseCard members rawCard
                    match parsedCard with
                    | TalkCard card -> Some card 
                    | _ -> None
                | _ -> None)
            return talkCards
        }

[<EntryPoint>]
let main argv = 
    let key = ConfigurationManager.AppSettings.Item("TrelloKey") 
    let token = ConfigurationManager.AppSettings.Item("TrelloToken")
    let trelloCred = {Key = key; Token = token}
    let allMembersAsync = Members.getAllMembers trelloCred
    let allCardsAsync = Cards.getAllCards trelloCred
    let test = BoardParser.parseBoard allMembersAsync allCardsAsync |> Async.RunSynchronously
    printfn "%A" argv
    0 
