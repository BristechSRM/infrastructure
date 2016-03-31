module DbRepository

open TrelloReader
open Dapper
open Dapper.Contrib.Extensions
open MySql.Data.MySqlClient

[<Table("admins")>]
type Admin = {
    [<Key>]
    Id : int
    Name : string
    Email : string
    ImageUrl : string
}

[<Table("speakers")>]
type Speaker = { 
    [<Key>]
    Id : int 
    Name : string
    Email : string
    Rating : sbyte
}

[<Table("talks")>]
type Talk = {
    [<Key>]
    Id : int
    Title : string
    Status : sbyte
    SpeakerId : int
    AdminId : int
}

type TalkAndRefs = {
    Talk : Talk
    SpeakerName : string
    AdminEmail : string
}

type TalkOutlineEntity = {
    TalkId : int
    Title: string
    Status: sbyte
    SpeakerName: string
    SpeakerEmail: string
    SpeakerRating: sbyte
    AdminName: string
    AdminImageUrl: string
}

let replaceDbData connectionString (board : BoardMetaData) =

    let extractSpeakerAndTalk (card : TrelloCard) =
        let speaker = 
            {   Speaker.Id = -1
                Name = card.SpeakerName
                Email = card.SpeakerEmail
                Rating = (sbyte) 0 }
               
        let talk = 
            {   Talk.Id = -1
                Title = card.TalkData
                Status = (sbyte) 1
                SpeakerId = -1
                AdminId =  -1} 
                
        let adminEmail = board.Members |> Array.pick (fun memb -> if memb.Id = card.AdminId then Some memb.Email else None ) 
        let talkAndTrelloRefs = {Talk = talk; SpeakerName = speaker.Name; AdminEmail = adminEmail;}
               
        speaker, talkAndTrelloRefs

    let getTalkWithForeignKeys (speakers : Speaker seq) (admins : Admin seq) (talkAndRef : TalkAndRefs) = 
        { talkAndRef.Talk with 
            SpeakerId = 
                speakers |> Seq.pick (fun speaker -> 
                    if speaker.Name = talkAndRef.SpeakerName then 
                        Some speaker.Id
                    else None)
            AdminId = 
                admins |> Seq.pick (fun admin -> 
                    if admin.Email = talkAndRef.AdminEmail then 
                        Some admin.Id
                    else None) }

    let inline insertAllAndGetAll (connection: MySqlConnection) (insertCommand:string) (collection : 'Entity seq) : 'Entity seq = 
        let rowsInserted = connection.Execute(insertCommand,collection)
        let typeName = typeof<'Entity>.Name
        let failString = sprintf "Incorrect number of %s(s) inserted. Failed" typeName
        if rowsInserted <> Seq.length collection then failwith failString
        connection.GetAll<'Entity>()

    let admins = 
        board.Members |> Array.map (fun memb -> 
                             { Admin.Id = -1
                               Name = memb.Name
                               Email = memb.Email
                               ImageUrl = memb.ImageUrl })  

    let speakers, talkAndTrelloRefs = 
        board.Cards
        |> Array.map extractSpeakerAndTalk
        |> Array.unzip


    use connection = new MySqlConnection(connectionString)
    connection.Open() 
    use transaction = connection.BeginTransaction()

    try 
        let talkDeleteSuccess = connection.DeleteAll<Talk>()
        let adminDeleteSuccess = connection.DeleteAll<Admin>()
        let speakerDeleteSuccess = connection.DeleteAll<Speaker>()

        let somethingFailed = 
            [adminDeleteSuccess; speakerDeleteSuccess;talkDeleteSuccess]
            |> List.exists not

        if somethingFailed then
            failwith "A delete failed"       
            
        let admins = insertAllAndGetAll connection @"insert admins(name,email,imageurl) values (@Name,@Email,@ImageUrl)" admins
        let speakers = insertAllAndGetAll connection @"insert speakers(name,email,rating) values (@Name,@Email,@Rating)" speakers

        let talks = Array.map (getTalkWithForeignKeys speakers admins) talkAndTrelloRefs

        let talks = insertAllAndGetAll connection @"insert talks(title,status,speakerid,adminid) values (@Title,@Status,@SpeakerId,@AdminId)" talks

        let talkOutlines = connection.Query<TalkOutlineEntity>("select * from talk_outlines")
        if Seq.length talkOutlines <> Seq.length talks then failwith "Incorrect number of talkOutlines found. Failed"

        transaction.Commit()
        connection.Close()
        "Success"
    with
        | :? System.Exception as ex-> 
            transaction.Rollback()
            
            connection.Close()
            printfn "Rolledback due to error!"
            printfn "%s" ex.Message
            "Failure"