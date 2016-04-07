module Credentials

open System.Configuration

type TrelloCredentials = 
    { Key : string
      Token : string }

let credConfigTemplate = """<?xml version="1.0" encoding="utf-8" ?>
                            <appSettings>
                                <add key ="TrelloKey" value="{UserTrelloKey}" />
                                <add key ="TrelloToken" value="{UserTrelloToken}" />
                            </appSettings>"""

let missingCredentialsMessage = 
    "TrelloKey or TrelloToken is unset. Create a TrelloCreds.config file next to the App.config with the following xml (Filling in the key and token value fields):" 
    + credConfigTemplate

let getTrelloCredentials() = 
    let creds = 
        { Key = ConfigurationManager.AppSettings.Item("TrelloKey")
          Token = ConfigurationManager.AppSettings.Item("TrelloToken") }

    if creds.Key = "UNSET" || creds.Token = "UNSET" then failwith missingCredentialsMessage
    else creds
