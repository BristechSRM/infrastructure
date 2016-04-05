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

    let parseCardAndActions members rawCardAndActions =
        let parsedCard = parseCard members rawCardAndActions.Card
        let correspondence = rawCardAndActions.Actions |> Array.map tryParseCommentAction |> Array.choose (createCorrespondence parsedCard |> Option.map)
        {TrelloCard = parsedCard; Correspondence = correspondence} 

    let parseBoardAsync trelloCred =
        async {
            //StartChild lets you start multiple asyncs without blocking for the result
            let! cardsAsync = Async.StartChild <| getAllRawTalkCards trelloCred
            let! membersMetaAsync = Async.StartChild <| getAllMembersAsync trelloCred

            //Now we block for the result
            let! cards = cardsAsync
            let! cardsAndCommentActions = getActionsPerCardAsync trelloCred cards
            let! membersMeta = membersMetaAsync

            let talkCards = cardsAndCommentActions |> Array.map (parseCardAndActions membersMeta)
            return {Members = membersMeta.Members; Cards = talkCards}
        }