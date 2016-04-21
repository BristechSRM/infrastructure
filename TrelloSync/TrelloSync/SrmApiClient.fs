module SrmApiClient
    open System
    open ImportModels
    open System.Text
    open System.Net.Http
    open Newtonsoft.Json
    open Helpers
    
    let sessionsServiceUri = Uri("http://127.0.0.1:9000")
    let commsServiceUri = Uri("http://127.0.0.1:9001")
    let profilesEndpoint = Uri(sessionsServiceUri, "Profiles")
    let sessionsEndpoint = Uri(sessionsServiceUri, "Sessions")
    let threadsEndpoint = Uri(commsServiceUri, "Threads")

    let httpClient = new HttpClient()

    let postStringAsync (uri : Uri) data = 
        let content = new StringContent(data,Encoding.UTF8,"application/json")
        httpClient.PostAsync(uri,content) |> Async.AwaitTask 

    let postAsync' (uri : Uri) data = 
        async {
            let data = JsonConvert.SerializeObject(data)
            let! response = postStringAsync uri data
            return! response.Content.ReadAsStringAsync() |> Async.AwaitTask
        }
    
    module Profiles = 
        let postAsync (profile : Profile) = 
            async {
                let! result = postAsync' profilesEndpoint profile 
                return parseQuotedGuid result
            }

        let postAndGetIdAsync (profile: Profile) = 
            async {
                let! newId = postAsync profile
                return {profile with Id = newId}    
            }
    
    module Sessions = 
        let postAsync (data : SessionData) = 
            async { 
                let! result = match data.AdminId with
                              | Some aId -> postAsync' sessionsEndpoint <| cardToSession data.TrelloCard data.SpeakerId aId data.ThreadId
                              | None -> postAsync' sessionsEndpoint <| cardToNoAdminSession data.TrelloCard data.SpeakerId data.ThreadId
                return parseQuotedGuid result
            }

    module Threads = 
        let postAsync (correspondence : CorrespondenceItem []) = 
            async {
                let! result = postAsync' threadsEndpoint correspondence 
                return parseQuotedGuid result
            }