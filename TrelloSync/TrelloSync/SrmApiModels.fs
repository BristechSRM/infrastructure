module SrmApiModels

open System
open Members
open Cards
open CommsActionParser

type CorrespondenceItem = 
    { SenderId : Guid
      ReceiverId : Guid
      Type : string
      SenderHandle : string
      ReceiverHandle : string
      Date : DateTime
      Message : string }

//Using 2 session details so that I don't need to write custom serialization code to deal with when an admin isn't assigned. 
type SessionDetail = 
    { Id : Guid
      Title : string
      Status : String
      SpeakerId : Guid
      AdminId : Guid
      ThreadId : Guid 
      Date : DateTime option}

type SessionDetailNoAdmin = 
    { Id : Guid
      Title : string
      Status : String
      SpeakerId : Guid
      ThreadId : Guid 
      Date : DateTime option}

type SessionData = 
    { TrelloCard : TrelloCard
      SpeakerId : Guid
      AdminId : Guid Option
      ThreadId : Guid }

type Rating = 
    | Zero = 0
    | One = 1
    | Two = 2
    | Three = 3
    | Four = 4
    | Five = 5

type Handle = 
    { Type : string
      Identifier : string }

type Profile = 
    { Id : Guid
      Forename : string
      Surname : string
      Rating : Rating
      ImageUrl : string
      Handles : Handle seq }

type ProfileAndTrelloId = 
    { Profile : Profile
      TrelloId : string }

type SessionAndCorrespondence = 
    { Correspondence : CorrespondenceItem []
      SessionData : SessionData }

let emailToHandle inputEmail = 
    match inputEmail with
    | "" -> Seq.empty
    | email -> 
        seq { 
            yield { Type = "Email"
                    Identifier = email }
        }

let memberToProfile (mem : TrelloMember) = 
    { Id = Guid.Empty
      Forename = mem.Forename
      Surname = mem.Surname
      Rating = Rating.Zero
      ImageUrl = mem.ImageUrl
      Handles = emailToHandle mem.Email }

let cardToProfile (card : TrelloCard) = 
    { Id = Guid.Empty
      Forename = card.SpeakerForename
      Surname = card.SpeakerSurname
      Rating = Rating.Zero
      ImageUrl = Helpers.defaultImage
      Handles = emailToHandle card.SpeakerEmail }

let cardToSession (card : TrelloCard) speakerId adminId threadId = 
    { SessionDetail.Id = Guid.Empty
      Title = card.TalkData
      Status = "assigned"
      SpeakerId = speakerId
      AdminId = adminId
      ThreadId = threadId 
      Date = card.Date }

let cardToNoAdminSession (card : TrelloCard) speakerId threadId = 
    { SessionDetailNoAdmin.Id = Guid.Empty
      Title = card.TalkData
      Status = "unassigned"
      SpeakerId = speakerId
      ThreadId = threadId 
      Date = card.Date }

let commsItemToCorrespondenceItem (adminId : Guid) (adminEmail : string) (speakerId : Guid) (speakerEmail : string) (item : CommsItem) = 
    let senderId, receiverId = 
        if item.From = adminEmail && item.To = speakerEmail then adminId, speakerId
        else speakerId, adminId
    { SenderId = senderId
      ReceiverId = receiverId
      Type = "Email"
      SenderHandle = item.From
      ReceiverHandle = item.To
      Date = item.Date
      Message = item.Message }
