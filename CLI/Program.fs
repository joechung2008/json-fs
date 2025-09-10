open System
open Shared

[<EntryPoint>]
let main argv =
    let input = Console.In.ReadToEnd()

    match JSONParser.Parse(input) with
    | Ok token ->
        let json = JsonSerializer.serializeToken token
        printfn "%s" json
        0
    | Error msg ->
        printfn "Error: %s" msg
        1
