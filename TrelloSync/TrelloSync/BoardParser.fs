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
    
    let comms = 
        match rawCardAndActions.Actions with
        | [||] -> [||]
        | actions -> 
            match parsedCard.SpeakerEmail with
            | "" -> 
                printfn "Card :%A has comms data, but no speakerEmail. Comms data will be saved, but not imported" parsedCard
                actions |> Array.choose (tryCreateCommsItem parsedCard)
            | _ -> actions |> Array.choose (tryCreateCommsItem parsedCard)
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
