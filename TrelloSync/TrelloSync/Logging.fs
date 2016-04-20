module Logging

open Serilog

let setupLogging () =
    Log.Logger <- LoggerConfiguration().ReadFrom.AppSettings().CreateLogger()
    Log.Logger.Information("Serilog logging initialised") 
