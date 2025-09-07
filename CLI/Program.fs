open System
open Shared

let rec prettyPrint (token: Token) (indent: int) : string =
    let indentStr = String.replicate indent "  "

    match token with
    | ArrayToken(_, elements) ->
        let elementIndent = String.replicate (indent + 1) "  "

        let elementsStr =
            elements
            |> List.map (fun t -> prettyPrint t (indent + 1))
            |> List.map (fun s ->
                s.Split('\n')
                |> Array.map (fun line -> elementIndent + line)
                |> String.concat "\n")
            |> String.concat ",\n"

        sprintf "%s[\n%s\n%s]" (if indent > 0 then "" else indentStr) elementsStr indentStr
    | ObjectToken(_, members) ->
        let membersStr =
            members
            |> List.mapi (fun i p ->
                match p.key with
                | StringToken(_, keyValue) ->
                    sprintf
                        "%s\"%s\": %s"
                        (String.replicate (indent + 1) "  ")
                        keyValue
                        (prettyPrint p.value (indent + 1))
                | _ -> "<unknown key>")
            |> String.concat ",\n"

        sprintf "%s{\n%s\n%s}" (if indent > 0 then "" else indentStr) membersStr indentStr
    | StringToken(_, value) -> sprintf "\"%s\"" value
    | NumberToken(_, _, valueAsString) -> valueAsString
    | TrueToken(_) -> "true"
    | FalseToken(_) -> "false"
    | NullToken(_) -> "null"

[<EntryPoint>]
let main argv =
    let input = Console.In.ReadToEnd()

    match JSONParser.Parse(input) with
    | Ok token ->
        printfn "%s" (prettyPrint token 0)
        0
    | Error msg ->
        printfn "Error: %s" msg
        1
