namespace Shared

open System
open System.Collections.Generic
open System.Globalization
open System.Text.RegularExpressions

type ArrayParserMode =
    | Scanning
    | Element
    | Delimiter
    | End

and ArrayParser =
    static member private DELIMITERS = new Regex("[ \\n\\r\\t\\],]")

    static member private getSkip =
        function
        | ArrayToken(s, _) -> s
        | FalseToken(s) -> s
        | NullToken(s) -> s
        | NumberToken(s, _, _) -> s
        | ObjectToken(s, _) -> s
        | StringToken(s, _) -> s
        | TrueToken(s) -> s

    static member Parse(s: string) : Result<Token, string> =
        let rec parse mode pos elements =
            if pos >= s.Length || mode = ArrayParserMode.End then
                Ok(ArrayToken(pos, List.rev elements))
            else
                let ch = s.[pos]

                match mode with
                | ArrayParserMode.Scanning ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) elements
                    elif ch = '[' then
                        parse ArrayParserMode.Element (pos + 1) elements
                    else
                        Error $"Expected '[', actual '{ch}'"
                | ArrayParserMode.Element ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) elements
                    elif ch = ']' then
                        if not (List.isEmpty elements) then
                            Error "Unexpected ','"
                        else
                            parse ArrayParserMode.End (pos + 1) elements
                    else
                        let slice = s.Substring(pos)

                        match ValueParser.Parse(slice, Some ArrayParser.DELIMITERS) with
                        | Ok element ->
                            parse ArrayParserMode.Delimiter (pos + ArrayParser.getSkip element) (element :: elements)
                        | Error e -> Error e
                | ArrayParserMode.Delimiter ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) elements
                    elif ch = ',' then
                        parse ArrayParserMode.Element (pos + 1) elements
                    elif ch = ']' then
                        parse ArrayParserMode.End (pos + 1) elements
                    else
                        Error $"Expected ',' or ']', actual '{ch}'"
                | ArrayParserMode.End -> parse mode pos elements

        parse ArrayParserMode.Scanning 0 []

and JSONParser =
    static member WHITESPACE = new Regex "[ \\n\\r\\t]"

    static member Parse(s: string) : Result<Token, string> =
        ValueParser.Parse(s, Some JSONParser.WHITESPACE)

and NumberParserMode =
    | Scanning
    | Characteristic
    | CharacteristicDigit
    | DecimalPoint
    | Mantissa
    | Exponent
    | ExponentSign
    | ExponentFirstDigit
    | ExponentDigits
    | End

and NumberParser =
    static member Parse(s: string, delimiters: Option<Regex>) : Result<Token, string> =
        let rec parse mode pos valueAsString =
            if pos >= s.Length || mode = NumberParserMode.End then
                match mode with
                | NumberParserMode.Characteristic
                | NumberParserMode.ExponentSign
                | NumberParserMode.ExponentFirstDigit -> Error $"Incomplete expression, mode {mode}"
                | _ ->
                    try
                        let value = Double.Parse(valueAsString)
                        Ok(NumberToken(pos, value, valueAsString))
                    with _ ->
                        Error "Invalid number format"
            else
                let ch = s.[pos]

                match mode with
                | NumberParserMode.Scanning ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) valueAsString
                    elif ch = '-' then
                        parse NumberParserMode.Characteristic (pos + 1) (valueAsString + ch.ToString())
                    else
                        parse NumberParserMode.Characteristic pos valueAsString
                | NumberParserMode.Characteristic ->
                    if ch = '0' then
                        parse NumberParserMode.DecimalPoint (pos + 1) (valueAsString + ch.ToString())
                    elif Char.IsDigit(ch) && ch <> '0' then
                        parse NumberParserMode.CharacteristicDigit (pos + 1) (valueAsString + ch.ToString())
                    else
                        Error $"Expected digit, actual '{ch}'"
                | NumberParserMode.CharacteristicDigit ->
                    if Char.IsDigit(ch) then
                        parse mode (pos + 1) (valueAsString + ch.ToString())
                    elif delimiters.IsSome && delimiters.Value.IsMatch(ch.ToString()) then
                        parse NumberParserMode.End pos valueAsString
                    else
                        parse NumberParserMode.DecimalPoint pos valueAsString
                | NumberParserMode.DecimalPoint ->
                    if ch = '.' then
                        parse NumberParserMode.Mantissa (pos + 1) (valueAsString + ch.ToString())
                    else
                        parse NumberParserMode.Exponent pos valueAsString
                | NumberParserMode.Mantissa ->
                    if Char.IsDigit(ch) then
                        parse mode (pos + 1) (valueAsString + ch.ToString())
                    else
                        parse NumberParserMode.Exponent pos valueAsString
                | NumberParserMode.Exponent ->
                    if ch = 'e' || ch = 'E' then
                        parse NumberParserMode.ExponentSign (pos + 1) (valueAsString + "e")
                    elif delimiters.IsSome && delimiters.Value.IsMatch(ch.ToString()) then
                        parse NumberParserMode.End pos valueAsString
                    else
                        Error $"Unexpected character '{ch}'"
                | NumberParserMode.ExponentSign ->
                    if ch = '-' || ch = '+' then
                        parse NumberParserMode.ExponentFirstDigit (pos + 1) (valueAsString + ch.ToString())
                    else
                        parse NumberParserMode.ExponentFirstDigit pos valueAsString
                | NumberParserMode.ExponentFirstDigit ->
                    if Char.IsDigit(ch) then
                        parse NumberParserMode.ExponentDigits (pos + 1) (valueAsString + ch.ToString())
                    else
                        Error $"Expected digit, actual '{ch}'"
                | NumberParserMode.ExponentDigits ->
                    if Char.IsDigit(ch) then
                        parse mode (pos + 1) (valueAsString + ch.ToString())
                    elif delimiters.IsSome && delimiters.Value.IsMatch(ch.ToString()) then
                        parse NumberParserMode.End pos valueAsString
                    else
                        Error $"Expected digit, actual '{ch}'"
                | NumberParserMode.End -> parse mode pos valueAsString

        parse NumberParserMode.Scanning 0 ""

and ObjectParserMode =
    | Scanning
    | Pair
    | Delimiter
    | End

and ObjectParser =
    static member private DELIMITERS = new Regex("[ \\n\\r\\t\\},]")

    static member Parse(s: string) : Result<Token, string> =
        let rec parse mode pos members =
            if pos >= s.Length || mode = ObjectParserMode.End then
                Ok(ObjectToken(pos, List.rev members))
            else
                let ch = s.[pos]

                match mode with
                | ObjectParserMode.Scanning ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) members
                    elif ch = '{' then
                        parse ObjectParserMode.Pair (pos + 1) members
                    else
                        Error $"Expected '{{', actual '{ch}'"
                | ObjectParserMode.Pair ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) members
                    elif ch = '}' then
                        if not (List.isEmpty members) then
                            Error "Unexpected ','"
                        else
                            parse ObjectParserMode.End (pos + 1) members
                    else
                        let slice = s.Substring(pos)

                        match PairParser.Parse(slice) with
                        | Ok pair -> parse ObjectParserMode.Delimiter (pos + pair.skip) (pair :: members)
                        | Error e -> Error e
                | ObjectParserMode.Delimiter ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) members
                    elif ch = ',' then
                        parse ObjectParserMode.Pair (pos + 1) members
                    elif ch = '}' then
                        parse ObjectParserMode.End (pos + 1) members
                    else
                        Error $"Expected ',' or '}}', actual '{ch}'"
                | ObjectParserMode.End -> parse mode pos members

        parse ObjectParserMode.Scanning 0 []

and PairParserMode =
    | Scanning
    | Key
    | Delimiter
    | Value
    | End

and PairParser =
    static member private DELIMITERS = new Regex("[ \\n\\r\\t\\},]")

    static member private getSkip =
        function
        | ArrayToken(s, _) -> s
        | FalseToken(s) -> s
        | NullToken(s) -> s
        | NumberToken(s, _, _) -> s
        | ObjectToken(s, _) -> s
        | StringToken(s, _) -> s
        | TrueToken(s) -> s

    static member Parse(s: string) : Result<PairToken, string> =
        let rec parse mode pos key value =
            if pos >= s.Length || mode = PairParserMode.End then
                Ok { skip = pos; key = key; value = value }
            else
                let ch = s.[pos]

                match mode with
                | PairParserMode.Scanning ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) key value
                    else
                        parse PairParserMode.Key pos key value
                | PairParserMode.Key ->
                    let slice = s.Substring(pos)

                    match StringParser.Parse(slice) with
                    | Ok k -> parse PairParserMode.Delimiter (pos + PairParser.getSkip k) k value
                    | Error e -> Error e
                | PairParserMode.Delimiter ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) key value
                    elif ch = ':' then
                        parse PairParserMode.Value (pos + 1) key value
                    else
                        Error $"Expected ':', actual '{ch}'"
                | PairParserMode.Value ->
                    let slice = s.Substring(pos)

                    match ValueParser.Parse(slice, Some PairParser.DELIMITERS) with
                    | Ok v -> parse PairParserMode.End (pos + PairParser.getSkip v) key v
                    | Error e -> Error e
                | PairParserMode.End -> parse mode pos key value

        parse PairParserMode.Scanning 0 Unchecked.defaultof<Token> Unchecked.defaultof<Token>

and StringParserMode =
    | Scanning
    | Char
    | EscapedChar
    | Unicode
    | End

and StringParser =
    static member Parse(s: string) : Result<Token, string> =
        let rec parse mode pos value =
            if pos >= s.Length || mode = StringParserMode.End then
                Ok(StringToken(pos, value))
            else
                let ch = s.[pos]

                match mode with
                | StringParserMode.Scanning ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1) value
                    elif ch = '"' then
                        parse StringParserMode.Char (pos + 1) value
                    else
                        Error $"Expected '\"', actual '{ch}'"
                | StringParserMode.Char ->
                    if ch = '\\' then
                        parse StringParserMode.EscapedChar (pos + 1) value
                    elif ch = '"' then
                        parse StringParserMode.End (pos + 1) value
                    elif ch <> '\n' && ch <> '\r' then
                        parse mode (pos + 1) (value + ch.ToString())
                    else
                        Error $"Unexpected character '{ch}'"
                | StringParserMode.EscapedChar ->
                    if ch = '\\' || ch = '"' || ch = '/' then
                        parse StringParserMode.Char (pos + 1) (value + ch.ToString())
                    elif ch = 'b' then
                        parse StringParserMode.Char (pos + 1) (value + "\b")
                    elif ch = 'f' then
                        parse StringParserMode.Char (pos + 1) (value + "\f")
                    elif ch = 'n' then
                        parse StringParserMode.Char (pos + 1) (value + "\n")
                    elif ch = 'r' then
                        parse StringParserMode.Char (pos + 1) (value + "\r")
                    elif ch = 't' then
                        parse StringParserMode.Char (pos + 1) (value + "\t")
                    elif ch = 'u' then
                        parse StringParserMode.Unicode (pos + 1) value
                    else
                        Error $"Unexpected escape character '{ch}'"
                | StringParserMode.Unicode ->
                    let slice = s.Substring(pos, 4)

                    if slice.Length < 4 then
                        Error $"Incomplete Unicode code '{slice}'"
                    else
                        try
                            let hex = Int32.Parse(slice, NumberStyles.HexNumber)
                            let char = Convert.ToChar(hex).ToString()
                            parse StringParserMode.Char (pos + 4) (value + char)
                        with _ ->
                            Error "Invalid Unicode code"
                | StringParserMode.End -> parse mode pos value

        parse StringParserMode.Scanning 0 ""

and ValueParserMode =
    | Scanning
    | Array
    | False
    | Null
    | Number
    | Object
    | String
    | True
    | End

and ValueParser =
    static member private getSkip =
        function
        | ArrayToken(s, _) -> s
        | FalseToken(s) -> s
        | NullToken(s) -> s
        | NumberToken(s, _, _) -> s
        | ObjectToken(s, _) -> s
        | StringToken(s, _) -> s
        | TrueToken(s) -> s

    static member Parse(s: string, delimiters: Option<Regex>) : Result<Token, string> =
        let rec parse mode pos =
            if pos >= s.Length || mode = ValueParserMode.End then
                Error "Unexpected end of input"
            else
                let ch = s.[pos]

                match mode with
                | ValueParserMode.Scanning ->
                    if Char.IsWhiteSpace(ch) then
                        parse mode (pos + 1)
                    elif ch = '[' then
                        let slice = s.Substring(pos)

                        match ArrayParser.Parse(slice) with
                        | Ok token ->
                            match token with
                            | ArrayToken(s, e) -> Ok(ArrayToken(s + pos, e))
                            | _ -> Error "Unexpected token type from ArrayParser"
                        | Error e -> Error e
                    elif ch = 'f' then
                        let slice = s.Substring(pos, 5)

                        if slice = "false" then
                            Ok(FalseToken(pos + 5))
                        else
                            Error $"Expected 'false', actual '{slice}'"
                    elif ch = 'n' then
                        let slice = s.Substring(pos, 4)

                        if slice = "null" then
                            Ok(NullToken(pos + 4))
                        else
                            Error $"Expected 'null', actual '{slice}'"
                    elif Char.IsDigit(ch) || ch = '-' then
                        let slice = s.Substring(pos)

                        match NumberParser.Parse(slice, delimiters) with
                        | Ok token ->
                            match token with
                            | NumberToken(s, v, vas) -> Ok(NumberToken(s + pos, v, vas))
                            | _ -> Error "Unexpected token type from NumberParser"
                        | Error e -> Error e
                    elif ch = '{' then
                        let slice = s.Substring(pos)

                        match ObjectParser.Parse(slice) with
                        | Ok token ->
                            match token with
                            | ObjectToken(s, m) -> Ok(ObjectToken(s + pos, m))
                            | _ -> Error "Unexpected token type from ObjectParser"
                        | Error e -> Error e
                    elif ch = '"' then
                        let slice = s.Substring(pos)

                        match StringParser.Parse(slice) with
                        | Ok token ->
                            match token with
                            | StringToken(s, v) -> Ok(StringToken(s + pos, v))
                            | _ -> Error "Unexpected token type from StringParser"
                        | Error e -> Error e
                    elif ch = 't' then
                        let slice = s.Substring(pos, 4)

                        if slice = "true" then
                            Ok(TrueToken(pos + 4))
                        else
                            Error $"Expected 'true', actual '{slice}'"
                    elif delimiters.IsSome && delimiters.Value.IsMatch(ch.ToString()) then
                        Error "Unexpected delimiter"
                    else
                        Error $"Unexpected character '{ch}'"
                | _ -> Error $"Unexpected mode {mode}"

        parse ValueParserMode.Scanning 0
