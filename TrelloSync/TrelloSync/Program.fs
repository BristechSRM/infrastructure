open Logging
open Serilog

[<EntryPoint>]
let main _ = 
    try
        JsonSettings.setDefaults()
        setupLogging()
        let saveFile = @"trello-import.json"
    
        let trelloData = 
            let trelloCred = Credentials.getTrelloCredentials()
            let trelloData = BoardParser.parseBoardAsync trelloCred |> Async.RunSynchronously
            Log.Information("Trello Data download and parse complete.")
            DataCache.save trelloData saveFile
            Log.Information("Saved to file: {file} for reference", saveFile)
            trelloData
    
        if Config.performImport then 
            Log.Information("Performing import")
            Import.importAll trelloData
        else
            Log.Information("Skipping Import") 
            0

    with
        | ex -> 
            Log.Fatal("Exception: {ex}",ex)
            1

