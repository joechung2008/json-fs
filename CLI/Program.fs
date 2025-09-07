open System
open Shared

let rec prettyPrint (token: Token) (indent: int) : string =
    let indentStr = String.replicate indent "  "

    match token with
    | :? ArrayToken as at ->
        let elementIndent = String.replicate (indent + 1) "  "

        let elements =
            at.elements
            |> Seq.map (fun t ->
                let s = prettyPrint t (indent + 1)

                s.Split('\n')
                |> Array.map (fun line -> elementIndent + line)
                |> String.concat "\n")
            |> String.concat ",\n"

        sprintf "%s[\n%s\n%s]" (if indent > 0 then "" else indentStr) elements indentStr
    | :? ObjectToken as ot ->
        let members =
            ot.members
            |> Seq.mapi (fun i p ->
                sprintf
                    "%s\"%s\": %s"
                    (String.replicate (indent + 1) "  ")
                    p.key.value
                    (prettyPrint p.value (indent + 1)))
            |> String.concat ",\n"

        sprintf "%s{\n%s\n%s}" (if indent > 0 then "" else indentStr) members indentStr
    | :? StringToken as st -> sprintf "\"%s\"" st.value
    | :? NumberToken as nt -> nt.value_as_string
    | :? TrueToken -> "true"
    | :? FalseToken -> "false"
    | :? NullToken -> "null"
    | _ -> "<unknown>"

[<EntryPoint>]
let main argv =
    try
        let input = Console.In.ReadToEnd()
        let token = JSONParser.Parse(input)
        printfn "%s" (prettyPrint token 0)
        0
    with ex ->
        printfn "Error: %s" ex.Message
        1
