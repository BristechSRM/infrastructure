module Board
    open Members
    open Cards
    type TrelloBoard =
        { Members : TrelloMember []
          Cards : BasicCard [] }

    let fetchBoardAsync trelloCred = 
        async {
            //StartChild lets you start multiple asyncs without blocking for the result
            let! cardsAsync = Async.StartChild <| getAllRawTalkCards trelloCred
            let! membersMetaAsync = Async.StartChild <| getAllMembersAsync trelloCred

            //Now we block for the result
            let! cards = cardsAsync
            let! membersMeta = membersMetaAsync

            return {Members = membersMeta.Members; Cards = cards |> Array.map(fun x -> x.Card)}

        }