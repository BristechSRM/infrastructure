module Members
    open Credentials

    type BasicMember = 
        { Id : string
          Username : string
          FullName : string
          AvatarHash : string }
    
    type TrelloMember = 
        { Id : string
          Name : string
          Email : string
          UserName : string
          ImageUrl : string }

    type GroupedMembers = 
        { Members : TrelloMember []
          IgnoredMembers : TrelloMember [] }

    type MemberId = { Id: string}

    (*
        This is essentially guessing the admin's Scott Logic email address. 
        After the split, if one string is found, we assume we've gotten their username, or a single name. 
        We then use it directly. 
        If 2 or more strings are found, we take the first and last strings (Basic attempt to get first and last name). 
        Then we use first letter of the first name, and the last name as the email. 
    *)
    let parseToEmail (fullName : string) = 
        let split = fullName.ToLowerInvariant().Split()
        match split.Length with 
        | 1  -> split.[0] + "@scottlogic.co.uk"
        | x when x > 1 -> 
            let firstNameFirstLetter = split.[0].Chars(0)
            let lastName = (Array.last split)
            sprintf "%c%s@scottlogic.co.uk" firstNameFirstLetter lastName
        | _ -> failwith "Full name of member is somehow missing? Make sure everyone on the trello board enters a full name. "

    let getImageUrl avatarHash = 
        match avatarHash with 
        | Some hash -> sprintf "https://trello-avatars.s3.amazonaws.com/%s/50.png" hash
        | None -> "https://placebear.com/50/50"

    let getMemberIdsAsync trelloCred : Async<MemberId []> = Download.from <| sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/members?fields=id&key=%s&token=%s" trelloCred.Key trelloCred.Token        

    let getBasicMemberAsync trelloCred id : Async<BasicMember> = Download.from <| sprintf "https://api.trello.com/1/members/%s?fields=username,fullName,avatarHash&key=%s&token=%s" id trelloCred.Key trelloCred.Token         

    let getMemberDetailsAsync trelloCred id =
        async {            
            let! basicMember = getBasicMemberAsync trelloCred id
            return {
                Id = basicMember.Id
                TrelloMember.Name = basicMember.FullName
                UserName = basicMember.Username
                Email = parseToEmail basicMember.FullName
                ImageUrl = getImageUrl <| Option.ofObj basicMember.AvatarHash //The option.ofObj is used because the hash is sometimes missing (Silently null). It converts the string to an option string, so null is passed as None
            }
        }

    let getMembersAsync trelloCred memberIds = 
        memberIds 
        |> Array.map (fun mem -> getMemberDetailsAsync trelloCred mem.Id) 
        |> Async.Parallel  

    let ignoredAdminUserNames = ["samdavies";"jamesphillpotts";"tamarachehayebmakarem1";"tamaramakarem";"nicholashemley"] 
    let getAllMembersAsync trelloCred = 
        async {
            let! memberIds = getMemberIdsAsync trelloCred
            let! members = getMembersAsync trelloCred memberIds            
            let ignoredMembers, keptMembers = 
                members 
                |> Array.partition(fun mem -> List.contains mem.UserName ignoredAdminUserNames)
            return {
                Members = keptMembers
                IgnoredMembers = ignoredMembers
            }
        }
