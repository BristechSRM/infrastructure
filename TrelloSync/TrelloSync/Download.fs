[<RequireQualifiedAccess>]
module Download

open System
open System.Net
open Newtonsoft.Json

let from url : Async<'a> = 
    async { 
        let uri = Uri(url)
        use webClient = new WebClient()
        let! rawData = webClient.AsyncDownloadString(uri)
        let data = JsonConvert.DeserializeObject<'a>(rawData)
        return data
    }
