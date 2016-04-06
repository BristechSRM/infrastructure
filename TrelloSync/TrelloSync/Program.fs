open System.IO
open Newtonsoft.Json

[<EntryPoint>]
let main _ = 
    let trelloCred = Credentials.getTrelloCredentials()
    let trelloData = Board.fetchBoardAsync trelloCred |> Async.RunSynchronously
    let result = JsonConvert.SerializeObject(trelloData, Formatting.Indented)
    File.WriteAllText(@"trello-import.json",result)
    0 
