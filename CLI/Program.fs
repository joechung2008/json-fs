open System
open Shared

let rec printObjectModel (token: Token) (indent: int) : unit =
    let indentStr = String.replicate indent "  "

    match token with
    | ArrayToken(_, elements) ->
        printfn "%sArrayToken:" indentStr
        elements |> List.iter (fun t -> printObjectModel t (indent + 1))
    | ObjectToken(_, members) ->
        printfn "%sObjectToken:" indentStr

        members
        |> List.iter (fun p ->
            printfn "%s  Key:" indentStr
            printObjectModel p.key (indent + 2)
            printfn "%s  Value:" indentStr
            printObjectModel p.value (indent + 2))
    | StringToken(_, value) -> printfn "%sStringToken: \"%s\"" indentStr value
    | NumberToken(_, _, valueAsString) -> printfn "%sNumberToken: %s" indentStr valueAsString
    | TrueToken(_) -> printfn "%sTrueToken" indentStr
    | FalseToken(_) -> printfn "%sFalseToken" indentStr
    | NullToken(_) -> printfn "%sNullToken" indentStr

[<EntryPoint>]
let main argv =
    let input = Console.In.ReadToEnd()

    match JSONParser.Parse(input) with
    | Ok token ->
        printObjectModel token 0
        0
    | Error msg ->
        printfn "Error: %s" msg
        1
