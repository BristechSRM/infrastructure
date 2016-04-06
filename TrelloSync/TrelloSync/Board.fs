module Board
    open Members
    open Cards
    type TrelloBoard =
        { Members : TrelloMember []
          Cards : BasicCard [] }

    let fetchBoardAsync trelloCred = 
        async {
            let! cards = getAllRawTalkCards trelloCred
            let! membersMeta = getAllMembersAsync trelloCred

            return {Members = membersMeta.Members; Cards = cards |> Array.map(fun x -> x.Card)}

        }