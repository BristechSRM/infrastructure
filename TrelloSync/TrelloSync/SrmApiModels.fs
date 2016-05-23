module SrmApiModels

open System
open Members
open Cards
open CommsActionParser

type CorrespondenceItem = 
    { Id : Guid
      ExternalId : string
      SenderId : Guid
      ReceiverId : Guid
      Date : DateTime
      Message : string
      Type : string
      SenderHandle : string
      ReceiverHandle : string }

//Using 2 session details so that I don't need to write custom serialization code to deal with when an admin isn't assigned. 
type SessionDetail = 
    { Id : Guid
      Title : string
      Status : String
      SpeakerId : Guid
      AdminId : Guid
      Date : DateTime option}

type SessionDetailNoAdmin = 
    { Id : Guid
      Title : string
      Status : String
      SpeakerId : Guid
      Date : DateTime option}

type SessionData = 
    { TrelloCard : TrelloCard
      SpeakerId : Guid
      AdminId : Guid Option }

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
      Bio : string
      Handles : Handle seq }

type ProfileWrapper = 
    { Profile : Profile
      Email : string
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
      Bio = String.Empty
      Handles = emailToHandle mem.Email }

let cardToProfile (card : TrelloCard) = 
    { Id = Guid.Empty
      Forename = card.SpeakerForename
      Surname = card.SpeakerSurname
      Rating = Rating.Zero
      ImageUrl = Helpers.defaultImage
      Bio = String.Empty
      Handles = emailToHandle card.SpeakerEmail }

let cardToSession (card : TrelloCard) speakerId adminId = 
    { SessionDetail.Id = Guid.Empty
      Title = card.TalkData
      Status = "assigned"
      SpeakerId = speakerId
      AdminId = adminId
      Date = card.Date }

let cardToNoAdminSession (card : TrelloCard) speakerId = 
    { SessionDetailNoAdmin.Id = Guid.Empty
      Title = card.TalkData
      Status = "unassigned"
      SpeakerId = speakerId
      Date = card.Date }

let commsItemToCorrespondenceItem (adminId : Guid) (adminEmail : string) (speakerId : Guid) (speakerEmail : string) (item : CommsItem) = 
    let senderId, receiverId = 
        if item.From = adminEmail && item.To = speakerEmail then adminId, speakerId
        else speakerId, adminId
    { Id = Guid.Empty
      ExternalId = sprintf "%A:%s:%s:Trello" (Guid.NewGuid()) item.From item.To 
      SenderId = senderId
      ReceiverId = receiverId
      Type = "Email"
      SenderHandle = item.From
      ReceiverHandle = item.To
      Date = item.Date
      Message = item.Message }
