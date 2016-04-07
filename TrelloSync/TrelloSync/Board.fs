module Board
    open Members
    open Cards
    open Actions

    open CardParser

    type CardWithActions = 
        { TrelloCard : TrelloCard 
          CommsActions : BasicAction [] }

    type TrelloBoard =
        { Members : TrelloMember []
          Cards : CardWithActions [] }

    let parseCardAndActions members rawCardsAndActions = 
        let parsedCard = parseCard members rawCardsAndActions.Card
        {TrelloCard = parsedCard; CommsActions = rawCardsAndActions.Actions}

    let parseBoardAsync trelloCred = 
        async {
            let! cards = getAllRawTalkCards trelloCred
            let! groupedMembers = getAllMembersAsync trelloCred
            let! cardsAndCommentActions = getActionsPerCardAsync trelloCred cards

            return 
                {
                    Members = groupedMembers.Members
                    Cards = cardsAndCommentActions |> Array.map (parseCardAndActions groupedMembers)
                }
        }