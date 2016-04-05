module Cards
    open System
    open System.Net
    open System.Globalization
    open System.Text.RegularExpressions 
    open Newtonsoft.Json
    open Helpers

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

    let expectedNumberOfGroupsInCardParse = 5
    let speakerNameGroup = 1

    let nameContainsDate (cardName: string) = 
        let monthNames = 
            DateTimeFormatInfo.CurrentInfo.MonthNames 
            |> Array.map (fun x -> x.ToUpperInvariant())
            |> Array.filter (String.IsNullOrWhiteSpace >> not)
        let cardNameUpped = cardName.ToUpperInvariant()

        monthNames 
        |> Array.exists (fun month -> cardNameUpped.Contains(month))

    let allGroupsMatched (groups : GroupCollection) = 
        groups.Count = expectedNumberOfGroupsInCardParse && not <| String.IsNullOrWhiteSpace (groups.[speakerNameGroup].Value)

    let (|AllRegexGroups|_|) pattern input = 
        let m = Regex.Match(input, pattern)
        if (m.Success) then Some m.Groups else None

    let categorizeCard (card : BasicCard)= 
        match card.Name with
        | _ when card.Name.ToUpperInvariant().Contains("TEMPLATE") -> Template card
        | _ when nameContainsDate card.Name -> EventDate card
        | AllRegexGroups "(.*)\[(.*)\]\((.*)\)(.*)$" groups -> 
            if allGroupsMatched groups then 
                UnparsedTalkCard {Card = card; Groups= groups}
            else 
                Unmatched card
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