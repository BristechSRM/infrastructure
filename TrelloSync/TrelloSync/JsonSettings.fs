module JsonSettings

open Newtonsoft.Json
open Bristech.Srm.HttpConfig

let setDefaults () = 
    JsonConvert.DefaultSettings <- (fun () -> 
        let settings = JsonSerializerSettings(
                        Formatting = Formatting.Indented,
                        ContractResolver = Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                       )
        settings.Converters.Add(OptionConverter())
        settings)