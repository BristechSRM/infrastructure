module BoardParser 
    open Members
    open Cards
    open Actions

    open CardParser
    open CommsActionParser

    type CardWithCorrs = 
        { TrelloCard : TrelloCard 
          Correspondence : Correspondence []}

    type TrelloBoard = 
        { Members : TrelloMember []
          Cards : CardWithCorrs [] }

    let tryParseCardAndActions members cas =
        let parsedCard = tryParseCard members cas.Card
        match parsedCard with
        | TalkCard card -> 
            let correspondance = cas.Actions |> Array.map tryParseCommentAction |> Array.choose (createCorrespondance card |> Option.map)
            Some {TrelloCard = card; Correspondence = correspondance} 
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