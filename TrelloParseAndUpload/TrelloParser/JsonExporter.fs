module JsonExporter

open DbRepository
open TrelloReader

open System.IO
open Newtonsoft.Json

 let exportBoardAsSrmTypes (board : BoardMetaData) exportFilePath = 

    let findAdmin (members : TrelloMember []) id = 
        let defaultAdmin = members |> Array.find (fun x -> x.UserName = "chrissmith58")
        let admin = members |> Array.tryFind (fun x -> x.Id = id)
        match admin with 
        | None -> defaultAdmin
        | Some a-> a

    let cardToOutline members index (card : TrelloCard)  = 
        let admin = findAdmin members card.AdminId
        {
            TalkId = index
            Title = card.TalkData
            Status = (sbyte)1
            SpeakerName = card.SpeakerName
            SpeakerEmail = card.SpeakerEmail
            SpeakerRating =(sbyte)0
            AdminName = admin.Name
            AdminImageUrl = admin.ImageUrl
        }

    let cardToOutline' = cardToOutline board.Members
    let outlines = 
        board.Cards
        |> Array.mapi (cardToOutline')
    let outlinesAsJson = JsonConvert.SerializeObject(outlines, Formatting.Indented)
    File.WriteAllText(exportFilePath,outlinesAsJson)
    0
