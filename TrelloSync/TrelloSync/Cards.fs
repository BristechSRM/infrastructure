module Cards
    open System
    open System.Text.RegularExpressions 
    open Credentials
    open JsonToTypeWebClient

    type BasicCard =
        { Id : string
          Name : string
          IdMembers : string [] }

    type RawTrelloCard = 
        { Card : BasicCard
          RegexGroups : GroupCollection }

    type TrelloCard = 
        { SpeakerName : string
          SpeakerEmail : string 
          TalkData : string 
          ExtraInfo : string 
          RawInput : string
          CardId : string
          AdminId : string
          AdminEmail : string }

    let expectedNumberOfGroupsInCardParse = 5
    let speakerNameGroup = 1

    let allGroupsMatched (groups : GroupCollection) = 
        groups.Count = expectedNumberOfGroupsInCardParse && not <| String.IsNullOrWhiteSpace (groups.[speakerNameGroup].Value)

    let (|AllRegexGroups|_|) pattern input = 
        let m = Regex.Match(input, pattern)
        if (m.Success) then Some m.Groups else None

    let cardsWithTalkData (card : BasicCard)= 
        match card.Name with
        | AllRegexGroups "(.*)\[(.*)\]\((.*)\)(.*)$" groups when allGroupsMatched groups-> Some {Card = card; RegexGroups= groups}
        | _ -> None

    let getBasicCardsAsync trelloCred : Async<BasicCard []> = downloadObjectAsync <| sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/cards/open?fields=id,name,idMembers&key=%s&token=%s" trelloCred.Key trelloCred.Token

    let getAllRawTalkCards trelloCred = 
        async {
            let! basicCards = getBasicCardsAsync trelloCred            
            return basicCards |> Array.choose(cardsWithTalkData)        
        }