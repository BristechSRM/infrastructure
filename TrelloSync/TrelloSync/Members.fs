module Members

open Helpers
open Credentials

type BasicMember = 
    { Id : string
      Username : string
      FullName : string
      AvatarHash : string }

type TrelloMember = 
    { Id : string
      Forename : string
      Surname : string
      Email : string
      UserName : string
      ImageUrl : string }

type GroupedMembers = 
    { Members : TrelloMember []
      IgnoredMembers : TrelloMember [] }

type MemberId = 
    { Id : string }

let getMemberIdsAsync trelloCred : Async<MemberId []> = 
    Download.from <| sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/members?fields=id&key=%s&token=%s" trelloCred.Key trelloCred.Token
let getBasicMemberAsync trelloCred id : Async<BasicMember> = 
    Download.from <| sprintf "https://api.trello.com/1/members/%s?fields=username,fullName,avatarHash&key=%s&token=%s" id trelloCred.Key trelloCred.Token

let getMemberDetailsAsync trelloCred id = 
    async { 
        let! basicMember = getBasicMemberAsync trelloCred id
        let forename, surname = parseToNames basicMember.FullName
        //The option.ofObj is used because the hash is sometimes missing (Silently null). It converts the string to an option string, so null is passed as None
        return { Id = basicMember.Id
                 Forename = forename
                 Surname = surname
                 UserName = basicMember.Username
                 Email = nameToEmail basicMember.FullName
                 ImageUrl = getImageUrl <| Option.ofObj basicMember.AvatarHash }
    }

let getMembersAsync trelloCred memberIds = 
    memberIds
    |> Array.map (fun mem -> getMemberDetailsAsync trelloCred mem.Id)
    |> Async.Parallel

let ignoredAdminUserNames = [ "samdavies"; "jamesphillpotts"; "tamarachehayebmakarem1"; "tamaramakarem"; "nicholashemley" ]

let getAllMembersAsync trelloCred = 
    async { 
        let! memberIds = getMemberIdsAsync trelloCred
        let! members = getMembersAsync trelloCred memberIds
        let ignoredMembers, keptMembers = members |> Array.partition (fun mem -> List.contains mem.UserName ignoredAdminUserNames)
        return { Members = keptMembers
                 IgnoredMembers = ignoredMembers }
    }
