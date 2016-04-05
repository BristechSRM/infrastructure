module Board
    open Members
    open Cards
    open Actions

    type TrelloBoard =
        { Members : TrelloMember []
          Cards : BasicCard [] 
          Actions : (string * BasicAction []) [] }

    let fetchBoardAsync trelloCred = 
        async {
            let! cards = getAllRawTalkCards trelloCred
            let! membersMeta = getAllMembersAsync trelloCred
            let! cardsAndCommentActions = getActionsPerCardAsync trelloCred cards
            return 
                {
                    Members = membersMeta.Members
                    Cards = cards |> Array.map(fun x -> x.Card)
                    Actions = cardsAndCommentActions |> Array.map(fun x -> (x.Card.Card.Id, x.Actions))
                }

        }