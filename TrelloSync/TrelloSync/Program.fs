open System.IO
open Newtonsoft.Json

[<EntryPoint>]
let main argv = 
    let trelloCred = getTrelloCredentials()
    let trelloData = BoardParser.parseBoardAsync trelloCred |> Async.RunSynchronously
    let result = JsonConvert.SerializeObject(trelloData, Formatting.Indented)
    File.WriteAllText(@"outlines-import.json",result)
    printfn "%A" argv
    0 
