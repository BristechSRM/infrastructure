module CommsReader

open System
open System.Text.RegularExpressions
open FSharp.Data
open TrelloReader

[<Literal>]
let TrelloExport = @"../bristech-speakers-export.json"

type Board = JsonProvider<TrelloExport>

type TrelloMemberDetails = 
    { Id : string
      FullName : string
      UserName : string }

type EmailDirection = 
    | Send
    | Recieve

type EmailMeta = 
    { Date : DateTime
      Direction : EmailDirection
      Text : string
      MemberDetails : TrelloMemberDetails }

type Correspondance = 
    { To : string
      From : string
      Date : DateTime
      Message : string }
    member c.AsCsv = 
        sprintf @"%s,%s,%s,""%s""" c.To (c.Date.ToString("s", System.Globalization.CultureInfo.InvariantCulture)) c.From c.Message

type CorrsAndErrors = {   
    Corrs : Correspondance 
    Errors : string list
}

type EmailCommsMeta = 
    { EmailComms : EmailMeta
      RawComment : string
      RawAction : Board.Action }

type CommentAction = 
    | EmailComms of EmailCommsMeta
    | Other of (string * Board.Action)

type CardsAndActions = 
    { Card : TrelloCard
      Comments : CommentAction [] }

type CardsAndEmails = 
    { Card : TrelloCard
      EmailComments : EmailMeta [] }

let parseBoard() = 
    let trelloExportRuntime = @"bristech-speakers-export.json"
    let board = Board.Load(trelloExportRuntime)
    
    let parseEmailDirection (dirString : string) = 
        match dirString.ToUpperInvariant() with
        | "SEND" -> Some Send
        | "RECEIVE" | "RECIEVE" -> Some Recieve
        | _ -> None
    
    let parseCommsComment (action : Board.Action) (comment : string) (groups : GroupCollection) = 
        let success, date = DateTime.TryParse(groups.[1].Value)
        if success then 
            match parseEmailDirection groups.[2].Value with
            | Some dir -> 
                let creator = 
                    { Id = action.MemberCreator.Id
                      FullName = action.MemberCreator.FullName
                      UserName = action.MemberCreator.Username }
                
                let email = 
                    { Date = date
                      Direction = dir
                      Text = groups.[3].Value
                      MemberDetails = creator }
                
                EmailComms { EmailComms = email
                             RawComment = comment
                             RawAction = action }
            | None -> Other(comment, action)
        else Other(comment, action)
    
    let parseCommentAction (comment, action) = 
        match comment with
        | AllRegexGroupsMultiLine "\[([0-9\/\.]+) - ([a-zA-Z]+)\](.*)$" groups -> 
            parseCommsComment action comment groups
        | _ -> Other(comment, action)
    
    let commentActionsForCard cardId (action : Board.Action) = 
        match action.Data.Card with
        | Some cardRef -> 
            match cardRef.Id = cardId with
            | true -> 
                match action.Data.Text.String with
                | Some comment -> Some(comment, action)
                | None -> 
                    match action.Data.Text.Number with
                    | Some num -> Some(num.ToString(), action)
                    | None -> None
            | false -> None
        | None -> None
    
    let parsedBoard = TrelloReader.parseBoard (board)
    let parsedCards = parsedBoard.Cards
    let commentActions = board.Actions |> Array.filter (fun x -> x.Type = "commentCard")
    
    let cardsAndActions = 
        parsedCards |> Array.map (fun card -> 
                           { Card = card
                             Comments = 
                                 commentActions
                                 |> Array.choose (commentActionsForCard card.CardId)
                                 |> Array.map parseCommentAction })
    
    let cardsAndEmails = 
        cardsAndActions |> Array.map (fun ca -> 
                               { Card = ca.Card
                                 EmailComments = 
                                     ca.Comments |> Array.choose (function 
                                                        | EmailComms metaData -> Some metaData.EmailComms
                                                        | _ -> None) })
    
    let createCorrespondance (card : TrelloCard) (em : EmailMeta) = 
        match em.Direction with
        | Send -> 
            { To = card.SpeakerEmail 
              From = card.AdminEmail
              Date = em.Date
              Message = em.Text }
        | Recieve -> 
            { To = card.AdminEmail
              From = card.SpeakerEmail
              Date = em.Date
              Message = em.Text }
    
    let corrs = cardsAndEmails |> Array.collect (fun ce -> ce.EmailComments |> Array.map (createCorrespondance ce.Card))
    let detectMissingTo (corr : CorrsAndErrors)= 
        if corr.Corrs.To = "" then
            {corr with Errors = "Missing To" :: corr.Errors } 
        else 
            corr
    let detectMissingFrom (corr : CorrsAndErrors)= 
        if corr.Corrs.From = "" then
            {corr with Errors = "Missing From" :: corr.Errors }  
        else 
            corr
    let detectMissingDate (corr : CorrsAndErrors)= 
        if corr.Corrs.Date = DateTime.MinValue then
            {corr with Errors = "Missing Date" :: corr.Errors }  
        else 
            corr
    let detectMissingMessage (corr : CorrsAndErrors)= 
        if corr.Corrs.Message = "" then
            {corr with Errors = "Missing Message" :: corr.Errors }  
        else 
            corr
    let detectCorrsErrors = detectMissingTo >> detectMissingFrom >> detectMissingDate >> detectMissingMessage

    let corrsWithErrors = corrs |> Array.choose (fun corr -> 
            let detectedErrors = {Corrs = corr; Errors = [] } |> detectCorrsErrors
            match detectedErrors.Errors with
            | [] -> None
            | _ -> Some detectedErrors )
    let corrsCsv = corrs |> Array.map (fun c -> c.AsCsv)
    corrsCsv
