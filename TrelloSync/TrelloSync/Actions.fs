module Actions 
    open System
    open System.Net
    open Newtonsoft.Json
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