module Actions
    open Cards
    open Credentials
    open Download

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

    let getBasicActionsAsync trelloCred id : Async<BasicAction []> = downloadObjectAsync <| sprintf "https://api.trello.com/1/cards/%s/actions?filter=commentCard&key=%s&token=%s" id trelloCred.Key trelloCred.Token

    //NOTE: Currently assuming that card will have less than 1000 actions. If a card has more than that, we need to deal with the trello paging to get them all
    let getCardCommentActionsAsync trelloCred (rawCard : RawTrelloCard) = 
        async {
            let! basicCommentActions = getBasicActionsAsync trelloCred rawCard.Card.Id
            return {Card = rawCard; Actions = basicCommentActions;}
        }

    let getActionsPerCardAsync trelloCred (rawCards : RawTrelloCard []) =
        async {
            return! rawCards |> Array.map(getCardCommentActionsAsync trelloCred) |> Async.Parallel
        }