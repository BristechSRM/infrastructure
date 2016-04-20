module Import

    open System
    open Serilog

    open Cards
    open BoardParser
    open SrmApiClient
    open ImportModels

    let findSrmProfileId key profiles = 
        profiles
        |> Array.tryFind (fun x -> x.TrelloId = key)
        |> Option.map (fun x -> x.Profile.Id)
    
    let prepSessionAndCorrData (adminProfiles : ProfileAndTrelloId []) (speakerProfiles : ProfileAndTrelloId []) (card : CardWithCorrespondence) = 
        let adminId = 
            match card.TrelloCard.AdminId with
            | Some id -> findSrmProfileId id adminProfiles
            | None -> None
        
        let speakerId = findSrmProfileId card.TrelloCard.CardId speakerProfiles |> Option.get
        
        let correspondence = 
            // If the card doesn't have a admin email or speakerEmail, then we can't complete all the required information for the correspondence item, so the correspondence won't be processed and inserted. 
            if Array.isEmpty card.Comms then [||]
            else 
                match card.TrelloCard.AdminEmail with
                | Some adminEmail -> 
                    match card.TrelloCard.SpeakerEmail with
                    | "" -> [||]
                    | speakerEmail -> 
                        let aId = Option.get adminId
                        card.Comms |> Array.map (commsItemToCorrespondenceItem aId adminEmail speakerId speakerEmail)
                | None -> [||]
        
        let sessionData = 
            { TrelloCard = card.TrelloCard
              SpeakerId = speakerId
              AdminId = adminId
              ThreadId = Guid.Empty }
        
        { Correspondence = correspondence
          SessionData = sessionData }
                    
    //TODO Error handling for import here and in ApiClient with post requests. Fail as early as possible. 
    let importAll (trelloBoard : TrelloBoard) = 
        let importedAdmins = 
            trelloBoard.Members 
            |> Array.map (fun mem -> 
                async {
                    let! profile = Profiles.postAndGetIdAsync <| memberToProfile mem
                    return {Profile = profile; TrelloId = mem.Id}
                })
            |> Async.Parallel
            |> Async.RunSynchronously

        let importedSpeakers = 
            trelloBoard.Cards
            |> Array.map (fun card -> 
                async {
                    let! profile = Profiles.postAndGetIdAsync <| cardToProfile card.TrelloCard
                    return {Profile = profile; TrelloId = card.TrelloCard.CardId}
                })
            |> Async.Parallel
            |> Async.RunSynchronously

        let sessionsDataAndCorrespondence = 
            trelloBoard.Cards 
            |> Array.map (prepSessionAndCorrData importedAdmins importedSpeakers)

        (*
            The comms and session information starts off in a combined state, but without the required Ids. The prepSessionAndCorrData above adds in the ids for the admins and speakers, if they exist. 
            Below we send off each set of comms data, then add the returned threadId to the SessionData record. The sessions are then posted. 

            This is done in combined steps since the data is already combined, and it means that we don't have to do a search over the threads to connect them with the right session. 
        *)
        let sessionsForImport =     
            sessionsDataAndCorrespondence
            |> Array.map (fun combo -> 
                async {
                    let! threadId = Threads.postAsync combo.Correspondence
                    return {combo.SessionData with ThreadId = threadId}
                })
            |> Async.Parallel
            |> Async.RunSynchronously

        let importedSessionsIds =  
            sessionsForImport
            |> Array.map Sessions.postAsync
            |> Async.Parallel
            |> Async.RunSynchronously
        Log.Information("Migration via services complete")
        0
