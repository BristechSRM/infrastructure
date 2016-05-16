﻿module Program

open System.IO
open System.Configuration
open Logging
open Serilog

[<EntryPoint>]
let main _ = 
    JsonSettings.setDefaults()
    setupLogging()
    let cacheFilePath = @"trello-import.json"
    
    let trelloData = 
        if Config.useCache && File.Exists(cacheFilePath) then 
            let trelloData = DataCache.load cacheFilePath
            Log.Information("Trello Data loaded from file: {file}",cacheFilePath)
            trelloData
        else 
            let trelloCred = Credentials.getTrelloCredentials()
            let trelloData = BoardParser.parseBoardAsync trelloCred |> Async.RunSynchronously
            DataCache.save trelloData cacheFilePath
            Log.Information("Trello Data download and parse complete. Saved to file: {file} for cache as well.", cacheFilePath)
            trelloData
    
    let result = 
        if Config.performImport then
            Import.importAll trelloData
        else
            0
    0
