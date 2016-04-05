module CardParser
    open Members
    open Cards

    let idIsNotForIgnoredAdmin (ignoredAdmins : TrelloMember []) id= 
        ignoredAdmins 
        |> Array.exists (fun x -> x.Id = id) 
        |> not

    let parseCard members {Card=card;Groups=groups} = 
        let adminId = 
            card.IdMembers 
                |> Array.tryFind (fun id -> idIsNotForIgnoredAdmin members.IgnoredMembers id )
                |> function 
                    | Some id -> id
                    | None -> members.DefaultMember.Id
        let admin = members.Members |> Array.pick (fun memb -> if memb.Id = adminId then Some memb else None ) 
        { 
            SpeakerName = groups.[1].Value
            SpeakerEmail = groups.[2].Value
            TalkData = groups.[3].Value
            ExtraInfo = groups.[4].Value
            RawInput = groups.[0].Value
            CardId = card.Id
            AdminId = adminId
            AdminEmail = admin.Email
        }