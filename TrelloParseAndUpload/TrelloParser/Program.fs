open System.Configuration
open TrelloReader
open DbRepository
open JsonExporter

let connectionString = ConfigurationManager.ConnectionStrings.Item("DefaultConnection").ConnectionString
let trelloExport = @"../bristech-speakers-export.json"
let parseExport = @"outlines-import.json"

[<EntryPoint>]
let main __ = 
    let board = Board.Load(trelloExport)
    let boardMetaData = parseBoard board
    let dbResult =  replaceDbData connectionString boardMetaData
    //let exportResult = exportBoardAsSrmTypes boardMetaData parseExport
    printfn "Completed with %s" dbResult
    stdin.ReadLine() |> ignore
    0 
