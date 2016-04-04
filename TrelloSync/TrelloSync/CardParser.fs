module CardParser
    open System
    open System.Text.RegularExpressions
    open Members
    open Cards

    let idIsNotForIgnoredAdmin (ignoredAdmins : TrelloMember []) id= 
        ignoredAdmins 
        |> Array.exists (fun x -> x.Id = id) 
        |> not

    let parseWasValid (groups : GroupCollection) = 
        groups.Count = 5 && not <| String.IsNullOrWhiteSpace (groups.[1].Value)

    let tryParseCard members {Card=card;Groups=groups} = 
        if parseWasValid groups then
            let adminId = 
                card.IdMembers 
                    |> Array.tryFind (fun id -> idIsNotForIgnoredAdmin members.IgnoredMembers id )
                    |> function 
                        | Some id -> id
                        | None -> members.DefaultMember.Id
            let admin = members.Members |> Array.pick (fun memb -> if memb.Id = adminId then Some memb else None ) 
            TalkCard { 
                SpeakerName = groups.[1].Value
                SpeakerEmail = groups.[2].Value
                TalkData = groups.[3].Value
                ExtraInfo = groups.[4].Value
                RawInput = groups.[0].Value
                CardId = card.Id
                AdminId = adminId
                AdminEmail = admin.Email
                }
        else 
            Unmatched card