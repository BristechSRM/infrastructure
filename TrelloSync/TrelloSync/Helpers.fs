module Helpers

open System
open Serilog

//Take first string of split as first name, take the rest as last name
let parseToNames (fullName : string) = 
    let split = fullName.Split()
    match split.Length with
    | 1 -> split.[0], ""
    | x when x > 1 -> 
        let lastName = split |> Array.skip 1 |> String.concat " " 
        split.[0],lastName
    | _ -> 
        let message = "Full name of member is somehow missing? Make sure everyone on the trello board enters a full name. Input value was: " + fullName
        Log.Error(message)
        failwith message

(*
        This is essentially guessing the admin's Scott Logic email address. 
        After the split, if one string is found, we assume we've gotten their username, or a single name. 
        We then use it directly. 
        If 2 or more strings are found, we take the first and last strings (Basic attempt to get first and last name). 
        Then we use first letter of the first name, and the last name as the email. 
    *)
let nameToEmail (fullName : string) = 
    let split = fullName.ToLowerInvariant().Split()
    match split.Length with
    | 1 -> split.[0] + "@scottlogic.co.uk"
    | x when x > 1 -> 
        let firstNameFirstLetter = split.[0].Chars(0)
        let lastName = (Array.last split)
        sprintf "%c%s@scottlogic.co.uk" firstNameFirstLetter lastName
    | _ -> 
        let message = "Full name of member is somehow missing? Make sure everyone on the trello board enters a full name. Input value was: " + fullName
        Log.Error(message)
        failwith message

let defaultImage = "https://placebear.com/50/50"

let getImageUrl avatarHash = 
    match avatarHash with
    | Some hash -> sprintf "https://trello-avatars.s3.amazonaws.com/%s/50.png" hash
    | None -> defaultImage

let parseQuotedGuid (guidString : string) = 
    Guid.Parse(guidString.Replace("\"",""))
