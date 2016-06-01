module Config

open System
open System.Configuration
open Serilog

let getConfigValue (key : string) = 
    let value = ConfigurationManager.AppSettings.Item(key)
    if String.IsNullOrWhiteSpace value then
        failwith <| sprintf "Configuration value with key %s is missing / is null  / is blank in configuration. Add key and value to proceed." key
    else 
        value

let getUriConfig (key : string) = 
    let value = getConfigValue key
    let errorMessage = sprintf "%s config is invalid. Make sure a valid TimeSpan value has been entered." key
    try
        Uri(value)
    with
    | :? UriFormatException as ex -> 
        let fullMessage = errorMessage + " " + ex.Message
        Log.Error(fullMessage)
        reraise()
    | ex -> 
        let fullMessage = errorMessage + " " + ex.Message
        Log.Error(fullMessage)
        reraise()  

let getBoolConfig (key : string) =
    match bool.TryParse <| getConfigValue key with
    | true , value -> value
    | _ -> 
        let message = sprintf "Could not parse configuration value for key: %s as boolean" key
        Log.Error(message)
        failwith message

let performImport = getBoolConfig "PerformImport"
let sessionsUri = getUriConfig "SessionsUrl"
let commsUri = getUriConfig "CommsUrl"
    
