open CommsReader
open System.IO

let trelloExport = @"bristech-speakers-export.json"

[<EntryPoint>]
let main argv = 
    let result = parseBoard()
    File.WriteAllLines("corrs.csv",result)
    printfn "Complete"
    stdin.ReadLine() |> ignore
    printfn "%A" argv
    0 // return an integer exit code
