open System
open System.Configuration
open System.Net
open Newtonsoft.Json

type TrelloCredentials = 
    { Key : string
      Token : string } 

module Members =     

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

    type MembersMeta = 
        { Members : TrelloMember []
          IgnoredMembers : TrelloMember []
          DefaultMember : TrelloMember }

    let getMemberDetails trelloCred id = 
        let parseToEmail (fullName : string) = 
            let split = fullName.Split()
            match split.Length with 
            | 1  -> 
                split.[0] + "@scottlogic.co.uk"
            | x when x > 1 -> 
                let firstNameFirstLetter = split.[0].Chars(0)
                let lastName = (Array.last split).ToLowerInvariant()
                sprintf "%c%s@scottlogic.co.uk" firstNameFirstLetter lastName
            | _ -> "missingEmail@scottlogic.co.uk" 

        let createImageUrl (avatarHash : string) =
            //sprintf "https://trello-avatars.s3.amazonaws.com/%s/original.png" <| avatarHash.ToString("N")
            sprintf "https://trello-avatars.s3.amazonaws.com/%s/50.png" <| avatarHash

        let getImageUrl avatarHash = 
            match avatarHash with 
            | Some hash -> createImageUrl hash
            | None -> "https://placebear.com/50/50"

        async {
            let uri = Uri(sprintf "https://api.trello.com/1/members/%s?fields=username,fullName,avatarHash&key=%s&token=%s" id trelloCred.Key trelloCred.Token)
            use webClient = new WebClient()
            let! rawMember = webClient.AsyncDownloadString(uri)
            let basicMember = JsonConvert.DeserializeObject<BasicMember>(rawMember)
            return {
                TrelloMember.Id = basicMember.Id
                Name = basicMember.FullName
                UserName = basicMember.Username
                Email = parseToEmail basicMember.FullName
                ImageUrl = getImageUrl <| Option.ofObj basicMember.AvatarHash
            }
        }

    type MemberId = { Id: string}
    let getMembers trelloCred= 
        async {
            let uri = Uri(sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/members?fields=id&key=%s&token=%s" trelloCred.Key trelloCred.Token)
            use webClient = new WebClient()
            let! rawMemberIds = webClient.AsyncDownloadString(uri)
            let memberIds = JsonConvert.DeserializeObject<MemberId []>(rawMemberIds)
            let allMembersAsync = 
                memberIds 
                |> Array.map (fun mem -> getMemberDetails trelloCred mem.Id) 
                |> Async.Parallel                
                
            return! allMembersAsync
        }
    let ignoredAdminUserNames = ["samdavies";"jamesphillpotts";"tamarachehayebmakarem1";"tamaramakarem";"nicholashemley"] 
    let getAllMembers trelloCred = 
        async {
            let! members = getMembers trelloCred
            let ignoredMembers, keptMembers = 
                members 
                |> Array.partition(fun mem -> List.contains mem.UserName ignoredAdminUserNames)
            let defaultMember = members |> Array.find (fun x -> x.UserName = "chrissmith58")
            return {
                Members = keptMembers
                IgnoredMembers = ignoredMembers
                DefaultMember = defaultMember
            }
        }

module Cards = 
    let getAllCards trelloCred = 
        async {
            let uri =  Uri(sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/cards/open?key=%s&token=%s" trelloCred.Key trelloCred.Token)
            use webClient = new WebClient()
            let! rawCards = webClient.AsyncDownloadString(uri)
            return rawCards
        }


[<EntryPoint>]
let main argv = 
    let key = ConfigurationManager.AppSettings.Item("TrelloKey") 
    let token = ConfigurationManager.AppSettings.Item("TrelloToken")
    let trelloCred = {Key = key; Token = token}
    let getAllMembersAsync = Members.getAllMembers trelloCred
    let allCards = Cards.getAllCards trelloCred |> Async.RunSynchronously
//    let uri = Uri(sprintf "https://api.trello.com/1/boards/524ec750ed130abd230011ab/cards/open/?key=%s&token=%s" trelloKey trelloToken)
//    use webClient = new WebClient()
//    let cards = webClient.DownloadString(uri)
    
    printfn "%A" argv
    0 // return an integer exit code
