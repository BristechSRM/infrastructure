open System.IO
open Newtonsoft.Json

[<EntryPoint>]
let main argv = 
    let trelloCred = Helpers.getTrelloCredentials()
    let trelloData = Members.getAllMembersAsync trelloCred |> Async.RunSynchronously
    let result = JsonConvert.SerializeObject(trelloData, Formatting.Indented)
    File.WriteAllText(@"trello-import.json",result)
    printfn "%A" argv
    0 
