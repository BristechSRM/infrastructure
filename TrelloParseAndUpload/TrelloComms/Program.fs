open CommsReader
let trelloExport = @"bristech-speakers-export.json"

[<EntryPoint>]
let main argv = 
    let result = parseBoard()
    printfn "Complete"
    stdin.ReadLine() |> ignore
    printfn "%A" argv
    0 // return an integer exit code
