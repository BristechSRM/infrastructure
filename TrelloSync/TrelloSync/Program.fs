open Logging
open Serilog
open System.IO

[<EntryPoint>]
let main _ = 
    try
        JsonSettings.setDefaults()
        setupLogging()
        let cacheFilePath = @"trello-import.json"
    
        let trelloData = 
            if Config.useCache && File.Exists(cacheFilePath) then 
                let trelloData = DataCache.load cacheFilePath
                Log.Information("Trello Data loaded from file: {file}", cacheFilePath)
                trelloData
            else 
                let trelloCred = Credentials.getTrelloCredentials()
                let trelloData = BoardParser.parseBoardAsync trelloCred |> Async.RunSynchronously
                DataCache.save trelloData cacheFilePath
                Log.Information("Trello Data download and parse complete. Saved to file: {file} for cache as well.", cacheFilePath)
                trelloData
    
        let result = 
            if Config.performImport then Import.importAll trelloData
            else 0
        0
    with
        | ex -> 
            Log.Fatal(ex.Message)
            1

