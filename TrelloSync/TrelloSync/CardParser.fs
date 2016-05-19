module CardParser

open System
open Members
open Cards
open Helpers
open Serilog

let idIsNotForIgnoredAdmin (ignoredAdmins : TrelloMember []) id = 
    ignoredAdmins
    |> Array.exists (fun x -> x.Id = id)
    |> not

let parseCard members { Card = card; RegexGroups = groups } = 
    let adminId, adminEmail = 
        let allowedAdminIds = card.IdMembers |> Array.filter (idIsNotForIgnoredAdmin members.IgnoredMembers)
        match allowedAdminIds with
        | [||] -> None, None
        | [| adminId |] -> 
            let foundMember = 
                members.Members |> Array.tryPick (fun memb -> 
                                       if memb.Id = adminId then Some memb
                                       else None)
            match foundMember with
            | Some admin -> Some adminId, Some admin.Email
            | None -> None, None
        | _ -> 
            let message = sprintf "Card: %A had multiple members attached, please remove additonal members so that there is one per card" card
            Log.Fatal(message)
            failwith message

    //Currently ignoring any date that is not on the first Thursday of the month. 
    let eventDate = 
        match card.Due with
        | None -> None
        | Some date-> 
            if date.DayOfWeek = DayOfWeek.Thursday && date.Day <= 7 then
                Some date
            else 
                None
   
    let forename, surname = parseToNames groups.[1].Value
    { SpeakerForename = forename
      SpeakerSurname = surname
      SpeakerEmail = groups.[2].Value
      TalkData = groups.[3].Value
      ExtraInfo = groups.[4].Value
      RawInput = groups.[0].Value
      CardId = card.Id
      Date = eventDate
      AdminId = adminId
      AdminEmail = adminEmail }
