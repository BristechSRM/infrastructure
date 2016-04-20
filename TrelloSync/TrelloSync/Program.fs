module Program

open System.IO
open System.Configuration
open Logging
open Serilog

[<EntryPoint>]
let main _ = 
    setupLogging()
    let cacheFilePath = @"trello-import.json"
    let useCache = 
        match ConfigurationManager.AppSettings.Item("UseCache").ToUpperInvariant() with 
        | "TRUE" -> true
        | "FALSE" -> false
        | other -> 
            let message = sprintf "Could not parse 'UseCache' configuration value: %s as boolean. Value must be one of true|TRUE|false|FALSE." other
            Log.Fatal(message)
            failwith message
    
    let trelloData = 
        if useCache && File.Exists(cacheFilePath) then 
            let trelloData = DataCache.load cacheFilePath
            Log.Information("Trello Data loaded from file: {file}",cacheFilePath)
            trelloData
        else 
            let trelloCred = Credentials.getTrelloCredentials()
            let trelloData = BoardParser.parseBoardAsync trelloCred |> Async.RunSynchronously
            DataCache.save trelloData cacheFilePath
            Log.Information("Trello Data download and parse complete. Saved to file: {file} for cache as well.", cacheFilePath)
            trelloData
    
    let result = Import.importAll trelloData
    0
