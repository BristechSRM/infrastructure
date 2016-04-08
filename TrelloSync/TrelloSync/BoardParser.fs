module BoardParser

open Members
open Cards
open Actions
open CardParser
open CommsActionParser

type CardWithCorrespondence = 
    { TrelloCard : TrelloCard
      Comms : CommsItem [] }

type TrelloBoard = 
    { Members : TrelloMember []
      Cards : CardWithCorrespondence [] }

let parseCardAndActions members rawCardAndActions = 
    let parsedCard = parseCard members rawCardAndActions.Card
    let comms = rawCardAndActions.Actions |> Array.choose (tryCreateCommsItem parsedCard)
    { TrelloCard = parsedCard
      Comms = comms }

let parseBoardAsync trelloCred = 
    async { 
        let! cards = getAllRawTalkCards trelloCred
        let! groupedMembers = getAllMembersAsync trelloCred
        let! cardsAndCommentActions = getActionsPerCardAsync trelloCred cards
        return { Members = groupedMembers.Members
                 Cards = cardsAndCommentActions |> Array.map (parseCardAndActions groupedMembers) }
    }
