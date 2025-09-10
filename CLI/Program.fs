open System
open System.IO
open Shared

[<EntryPoint>]
let main argv =
    use reader = new StreamReader(Console.OpenStandardInput())
    let input = reader.ReadToEnd()

    match JSONParser.Parse(input) with
    | Ok token ->
        let json = JsonSerializer.serializeToken token
        printfn "%s" json
        0
    | Error msg ->
        printfn "Error: %s" msg
        1
