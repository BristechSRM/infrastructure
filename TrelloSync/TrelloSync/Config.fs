module Config

open System.Configuration
open Serilog

let getBoolConfig (key : string) =
    match bool.TryParse <| ConfigurationManager.AppSettings.Item(key) with
    | true , value -> value
    | _ -> 
        let message = sprintf "Could not parse configuration value for key: %s as boolean" key
        Log.Fatal(key)
        failwith message

let useCache = getBoolConfig "UseCache"
let performImport = getBoolConfig "PerformImport"
    
