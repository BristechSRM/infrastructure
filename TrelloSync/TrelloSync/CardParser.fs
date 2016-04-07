module CardParser
    open Members
    open Cards

    let idIsNotForIgnoredAdmin (ignoredAdmins : TrelloMember []) id= 
        ignoredAdmins 
        |> Array.exists (fun x -> x.Id = id) 
        |> not

    let parseCard members { Card=card; RegexGroups=groups} = 
        let adminId,adminEmail = 
            let allowedAdminIds =  card.IdMembers |> Array.filter(idIsNotForIgnoredAdmin members.IgnoredMembers) 
            match allowedAdminIds with
            | [||]-> None, None
            | [|adminId|] -> 
                let foundMember = members.Members |> Array.tryPick (fun memb -> if memb.Id = adminId then Some memb else None)
                match foundMember with
                | Some admin -> Some adminId, Some admin.Email
                | None -> None, None
            | _ -> failwith <| sprintf "Card %A had multiple members attached, please remove additonal members so that there is one per card" card
        { 
            SpeakerName = groups.[1].Value
            SpeakerEmail = groups.[2].Value
            TalkData = groups.[3].Value
            ExtraInfo = groups.[4].Value
            RawInput = groups.[0].Value
            CardId = card.Id
            AdminId = adminId
            AdminEmail = adminEmail
        }