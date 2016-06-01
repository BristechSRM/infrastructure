module Import

    open System
    open Serilog

    open Cards
    open Members
    open BoardParser
    open SrmApiClient
    open SrmApiModels

    let findSrmProfileId key profiles = 
        profiles
        |> Array.tryFind (fun x -> x.TrelloId = key)
        |> Option.map (fun x -> x.Profile.Id)
    
    let prepSessionAndCorrData (adminProfiles : ProfileWrapper []) (speakerProfiles : ProfileWrapper []) (card : CardWithCorrespondence) = 
        let adminId = 
            match card.TrelloCard.AdminId with
            | Some id -> findSrmProfileId id adminProfiles
            | None -> None
        
        let speakerId = 
            match findSrmProfileId card.TrelloCard.CardId speakerProfiles with 
            | Some id -> id 
            | None -> 
                let message = "A speaker profile corresponding to the speaker attached to this card should have been added. If this code has been reached, an error occured with adding the speaker profile that was not detected."
                Log.Error(message)
                failwith message
        
        let correspondence = 
            // If the card doesn't have a admin email or speakerEmail, then we can't complete all the required information for the correspondence item, so the correspondence won't be processed and inserted. 
            if Array.isEmpty card.Comms then [||]
            else 
                match card.TrelloCard.AdminEmail with
                | Some adminEmail -> 
                    match card.TrelloCard.SpeakerEmail with
                    | "" -> [||]
                    | speakerEmail -> 
                        // A card cannot have an adminEmail without an Admin id, so no need to deal with other case. 
                        let aId = Option.get adminId
                        card.Comms |> Array.map (commsItemToCorrespondenceItem aId adminEmail speakerId speakerEmail)
                | None -> [||]
        
        let sessionData = 
            { TrelloCard = card.TrelloCard
              SpeakerId = speakerId
              AdminId = adminId }
        
        { Correspondence = correspondence
          SessionData = sessionData }

    let importAdminAsync (mem : TrelloMember) = 
        async {
            Log.Information("Importing admin with email: {email}", mem.Email)
            let! profile = Profiles.postAndGetIdAsync <| memberToProfile mem
            return {Profile = profile; Email = mem.Email; TrelloId = mem.Id}
        }

    let importSpeakerAsync (importedAdmins : ProfileWrapper []) (card : CardWithCorrespondence) = 
        async {
            let! profile = 
                let foundAdmin = importedAdmins |> Array.tryFind (fun x -> x.Email = card.TrelloCard.SpeakerEmail)
                match foundAdmin with
                | Some adminProfile ->
                    Log.Information("Found Matching admin profile for speaker. No Import performed. Email: {emai}", adminProfile.Email) 
                    async {return adminProfile.Profile}
                | None -> 
                    Log.Information("Importing speaker with email: {email}", card.TrelloCard.SpeakerEmail)
                    Profiles.postAndGetIdAsync <| cardToProfile card.TrelloCard
            return {Profile = profile; Email = card.TrelloCard.SpeakerEmail; TrelloId = card.TrelloCard.CardId}
        }
                    
    //TODO Error handling for import here and in ApiClient with post requests. Fail as early as possible. 
    let importAll (trelloBoard : TrelloBoard) = 
        let importedAdmins = 
            trelloBoard.Members 
            |> Array.map importAdminAsync
            |> Async.Parallel
            |> Async.RunSynchronously

        let importedSpeakers = 
            trelloBoard.Cards
            |> Array.map (importSpeakerAsync importedAdmins)
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
        let importedCorrespondence =     
            sessionsDataAndCorrespondence
            |> Array.map (fun card -> 
                async {
                    let! result = 
                        card.Correspondence
                        |> Array.map Correspondence.postAsync
                        |> Async.Parallel
                    return result
                })
            |> Async.Parallel
            |> Async.RunSynchronously

        let importedSessionsIds =  
            sessionsDataAndCorrespondence
            |> Array.map (fun card -> Sessions.postAsync card.SessionData)
            |> Async.Parallel
            |> Async.RunSynchronously

        0
