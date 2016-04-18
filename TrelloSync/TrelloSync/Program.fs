module Program

open System
open System.IO
open System.Configuration

[<EntryPoint>]
let main _ = 
    let cacheFilePath = @"trello-import.json"
    let useCache = 
        match ConfigurationManager.AppSettings.Item("UseCache").ToUpperInvariant() with 
        | "TRUE" -> true
        | "FALSE" -> false
        | other -> failwith <| sprintf "Could not parse 'UseCache' configuration value: %s as boolean. Value must be one of true|TRUE|false|FALSE." other
    
    let trelloData = 
        if useCache && File.Exists(cacheFilePath) then 
            let trelloData = DataCache.load cacheFilePath
            printfn "Trello Data loaded from file"
            trelloData
        else 
            let trelloCred = Credentials.getTrelloCredentials()
            let trelloData = BoardParser.parseBoardAsync trelloCred |> Async.RunSynchronously
            DataCache.save trelloData cacheFilePath
            printfn "Trello Data download and parse complete. Saved to file as cache as well."
            trelloData
    
    let mockImport() = printfn "Migration Complete"
    0
