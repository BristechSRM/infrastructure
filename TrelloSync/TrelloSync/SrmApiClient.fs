module SrmApiClient
    open System
    open SrmApiModels
    open System.Text
    open System.Net.Http
    open Newtonsoft.Json
    open Helpers
    open Config
    
    let profilesEndpoint = Uri(sessionsUri, "Profiles")
    let sessionsEndpoint = Uri(sessionsUri, "Sessions")
    let correspondenceEndpoint = Uri(commsUri, "Correspondence")

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
                              | Some aId -> postAsync' sessionsEndpoint <| cardToSession data.TrelloCard data.SpeakerId aId
                              | None -> postAsync' sessionsEndpoint <| cardToNoAdminSession data.TrelloCard data.SpeakerId
                return parseQuotedGuid result
            }

    module Correspondence = 
        let postAsync (correspondence : CorrespondenceItem ) = 
            async {
                let! result = postAsync' correspondenceEndpoint correspondence 
                return parseQuotedGuid result
            }