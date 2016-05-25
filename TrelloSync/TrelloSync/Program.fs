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
            let result = Import.importAll trelloData
            Log.Information("Migration via services complete")
            result
        else
            Log.Information("Import was not enabled. Skipping Input") 
            0

    with
        | ex -> 
            Log.Fatal("Program Exit caused by Exception: {ex}",ex)
            1

