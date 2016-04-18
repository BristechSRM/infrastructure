module DataCache 

open System.IO
open Newtonsoft.Json

let save data path = 
    let result = JsonConvert.SerializeObject(data, Formatting.Indented)
    File.WriteAllText(path, result)

let load path : 'a = 
    let data = File.ReadAllText(path)
    JsonConvert.DeserializeObject<'a>(data)