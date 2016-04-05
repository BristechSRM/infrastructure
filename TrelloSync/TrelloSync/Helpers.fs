[<AutoOpen>]
module Helpers
    open System.Configuration
    open System.Text.RegularExpressions 
    type TrelloCredentials = 
        { Key : string
          Token : string } 

    let credConfigTemplate = """<?xml version="1.0" encoding="utf-8" ?>
                            <appSettings>
                                <add key ="TrelloKey" value="{UserTrelloKey}" />
                                <add key ="TrelloToken" value="{UserTrelloToken}" />
                            </appSettings>"""

    let missingCredentailsMessage = "TrelloKey or TrelloToken is unset. Create a TrelloCreds.config file next to the App.config with the following xml (Filling in the key and token value fields):" + credConfigTemplate

    let getTrelloCredentials() = 
        let creds = {Key = ConfigurationManager.AppSettings.Item("TrelloKey") ; Token = ConfigurationManager.AppSettings.Item("TrelloToken")}
        if creds.Key = "UNSET" || creds.Token = "UNSET" then
            failwith missingCredentailsMessage
        else 
            creds

    let (|AllRegexGroups|_|) pattern input = 
        let m = Regex.Match(input, pattern)
        if (m.Success) then Some m.Groups else None

    let (|AllRegexGroupsMultiLine|_|) pattern input = 
        let m = Regex.Match(input, pattern,RegexOptions.Singleline)
        if (m.Success) then Some m.Groups else None