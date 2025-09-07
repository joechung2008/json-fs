namespace Shared

open System
open System.Collections.Generic
open System.Globalization
open System.Text.RegularExpressions

type ArrayParserMode =
    | Scanning = 0
    | Element = 1
    | Delimiter = 2
    | End = 3

and ArrayParser =
    static member private DELIMITERS = new Regex("[ \\n\\r\\t\\],]")
    static member private WHITESPACE = new Regex("[ \\n\\r\\t]")

    static member Parse(s: string) : ArrayToken =
        let elements = new List<Token>()
        let mutable mode = ArrayParserMode.Scanning
        let mutable pos = 0

        while pos < s.Length && mode <> ArrayParserMode.End do
            let ch = s.Substring(pos, 1)

            match mode with
            | ArrayParserMode.Scanning ->
                if ArrayParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = "[" then
                    pos <- pos + 1
                    mode <- ArrayParserMode.Element
                else
                    raise (new Exception(sprintf "Expected '[', actual '%s'" ch))
            | ArrayParserMode.Element ->
                if ArrayParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = "]" then
                    if elements.Count > 0 then
                        raise (new Exception("Unexpected ','"))

                    pos <- pos + 1
                    mode <- ArrayParserMode.End
                else
                    let slice = s.Substring(pos)
                    let element = ValueParser.Parse(slice, ArrayParser.DELIMITERS)
                    elements.Add(element)
                    pos <- pos + element.skip
                    mode <- ArrayParserMode.Delimiter
            | ArrayParserMode.Delimiter ->
                if ArrayParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = "," then
                    pos <- pos + 1
                    mode <- ArrayParserMode.Element
                elif ch = "]" then
                    pos <- pos + 1
                    mode <- ArrayParserMode.End
                else
                    raise (new Exception(sprintf "Expected ',' or ']', actual '%s'" ch))
            | ArrayParserMode.End -> ()
            | _ -> raise (new Exception(sprintf "Unexpected mode %A" mode))

        new ArrayToken(pos, elements)

and JSONParser =
    static member WHITESPACE = new Regex "[ \\n\\r\\t]"

    static member Parse(s: string) : Token =
        ValueParser.Parse(s, JSONParser.WHITESPACE)

and NumberParserMode =
    | Scanning = 0
    | Characteristic = 1
    | CharacteristicDigit = 2
    | DecimalPoint = 3
    | Mantissa = 4
    | Exponent = 5
    | ExponentSign = 6
    | ExponentFirstDigit = 7
    | ExponentDigits = 8
    | End = 9

and NumberParser =
    static member private DIGITS = new Regex("\d")
    static member private NONZERODIGITS = new Regex("[1-9]")
    static member private WHITESPACE = new Regex("[ \\n\\r\\t]")

    static member Parse(s: string, delimiters: Regex) : NumberToken =
        let mutable mode = NumberParserMode.Scanning
        let mutable pos = 0
        let mutable value_as_string = ""

        while pos < s.Length && mode <> NumberParserMode.End do
            let ch = s.Substring(pos, 1)

            match mode with
            | NumberParserMode.Scanning ->
                if NumberParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = "-" then
                    pos <- pos + 1
                    value_as_string <- value_as_string + ch
                    mode <- NumberParserMode.Characteristic
                else
                    mode <- NumberParserMode.Characteristic
            | NumberParserMode.Characteristic ->
                if ch = "0" then
                    pos <- pos + 1
                    value_as_string <- value_as_string + ch
                    mode <- NumberParserMode.DecimalPoint
                elif NumberParser.NONZERODIGITS.IsMatch(ch) then
                    pos <- pos + 1
                    value_as_string <- value_as_string + ch
                    mode <- NumberParserMode.CharacteristicDigit
                else
                    raise (new Exception(sprintf "Expected digit, actual '%s'" ch))

            | NumberParserMode.CharacteristicDigit ->
                if NumberParser.DIGITS.IsMatch(ch) then
                    pos <- pos + 1
                    value_as_string <- value_as_string + ch
                elif delimiters <> null && delimiters.IsMatch(ch) then
                    mode <- NumberParserMode.End
                else
                    mode <- NumberParserMode.DecimalPoint
            | NumberParserMode.DecimalPoint ->
                if ch = "." then
                    pos <- pos + 1
                    value_as_string <- value_as_string + ch
                    mode <- NumberParserMode.Mantissa
                else
                    mode <- NumberParserMode.Exponent
            | NumberParserMode.Mantissa ->
                if NumberParser.DIGITS.IsMatch(ch) then
                    pos <- pos + 1
                    value_as_string <- value_as_string + ch
                else
                    mode <- NumberParserMode.Exponent
            | NumberParserMode.Exponent ->
                if ch = "e" || ch = "E" then
                    pos <- pos + 1
                    value_as_string <- value_as_string + "e"
                    mode <- NumberParserMode.ExponentSign
                elif delimiters <> null && delimiters.IsMatch(ch) then
                    mode <- NumberParserMode.End
                else
                    raise (new Exception(sprintf "Unexpected character '%s'" ch))
            | NumberParserMode.ExponentSign ->
                if ch = "-" || ch = "+" then
                    pos <- pos + 1
                    value_as_string <- value_as_string + ch
                    mode <- NumberParserMode.ExponentFirstDigit
                else
                    mode <- NumberParserMode.ExponentFirstDigit
            | NumberParserMode.ExponentFirstDigit ->
                if NumberParser.DIGITS.IsMatch(ch) then
                    pos <- pos + 1
                    value_as_string <- value_as_string + ch
                    mode <- NumberParserMode.ExponentDigits
                else
                    raise (new Exception(sprintf "Expected digit, actual '%s'" ch))
            | NumberParserMode.ExponentDigits ->
                if NumberParser.DIGITS.IsMatch(ch) then
                    pos <- pos + 1
                    value_as_string <- value_as_string + ch
                elif delimiters <> null && delimiters.IsMatch(ch) then
                    mode <- NumberParserMode.End
                else
                    raise (new Exception(sprintf "Expected digit, actual '%s'" ch))
            | NumberParserMode.End -> ()
            | _ -> raise (new Exception(sprintf "Unexpected mode %A" mode))

        let number =
            match mode with
            | NumberParserMode.Characteristic
            | NumberParserMode.ExponentSign
            | NumberParserMode.ExponentFirstDigit ->
                raise (new Exception(sprintf "Incomplete expression, mode %A" mode))
            | _ ->
                let value = Double.Parse(value_as_string)
                new NumberToken(pos, value, value_as_string)

        number

and ObjectParserMode =
    | Scanning = 0
    | Pair = 1
    | Delimiter = 2
    | End = 3

and ObjectParser =
    static member private DELIMITERS = new Regex("[ \\n\\r\\t\\},]")
    static member private WHITESPACE = new Regex("[ \\n\\r\\t]")

    static member Parse(s: string) : ObjectToken =
        let members = new List<PairToken>()
        let mutable mode = ObjectParserMode.Scanning
        let mutable pos = 0

        while pos < s.Length && mode <> ObjectParserMode.End do
            let ch = s.Substring(pos, 1)

            match mode with
            | ObjectParserMode.Scanning ->
                if ObjectParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = "{" then
                    pos <- pos + 1
                    mode <- ObjectParserMode.Pair
                else
                    raise (new Exception(sprintf "Expected '{', actual '%s'" ch))
            | ObjectParserMode.Pair ->
                if ObjectParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = "}" then
                    if members.Count > 0 then
                        raise (new Exception("Unexpected ','"))

                    pos <- pos + 1
                    mode <- ObjectParserMode.End
                else
                    let slice = s.Substring(pos)
                    let pair = PairParser.Parse(slice)
                    members.Add(pair)
                    pos <- pos + pair.skip
                    mode <- ObjectParserMode.Delimiter
            | ObjectParserMode.Delimiter ->
                if ObjectParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = "," then
                    pos <- pos + 1
                    mode <- ObjectParserMode.Pair
                elif ch = "}" then
                    pos <- pos + 1
                    mode <- ObjectParserMode.End
                else
                    raise (new Exception(sprintf "Expected ',' or '}', actual '%s'" ch))
            | ObjectParserMode.End -> ()
            | _ -> raise (new Exception(sprintf "Unexpected mode %A" mode))

        new ObjectToken(pos, members)

and PairParserMode =
    | Scanning = 0
    | Key = 1
    | Delimiter = 2
    | Value = 3
    | End = 4

and PairParser =
    static member private DELIMITERS = new Regex("[ \\n\\r\\t\\},]")
    static member private WHITESPACE = new Regex("[ \\n\\r\\t]")

    static member Parse(s: string) : PairToken =
        let mutable key: StringToken = null
        let mutable value: Token = null
        let mutable mode = PairParserMode.Scanning
        let mutable pos = 0

        while pos < s.Length && mode <> PairParserMode.End do
            let ch = s.Substring(pos, 1)

            match mode with
            | PairParserMode.Scanning ->
                if PairParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                else
                    mode <- PairParserMode.Key
            | PairParserMode.Key ->
                let slice = s.Substring(pos)
                key <- StringParser.Parse(slice)
                pos <- pos + key.skip
                mode <- PairParserMode.Delimiter
            | PairParserMode.Delimiter ->
                if PairParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = ":" then
                    pos <- pos + 1
                    mode <- PairParserMode.Value
                else
                    raise (new Exception(sprintf "Expected ':', actual '%s'" ch))
            | PairParserMode.Value ->
                let slice = s.Substring(pos)
                value <- ValueParser.Parse(slice, PairParser.DELIMITERS)
                pos <- pos + value.skip
                mode <- PairParserMode.End
            | PairParserMode.End -> ()
            | _ -> raise (new Exception(sprintf "Unexpected mode %A" mode))

        new PairToken(pos, key, value)

and StringParserMode =
    | Scanning = 0
    | Char = 1
    | EscapedChar = 2
    | Unicode = 3
    | End = 4

and StringParser =
    static member private WHITESPACE = new Regex("[ \\n\\r\\t]")

    static member Parse(s: string) : StringToken =
        let mutable mode = StringParserMode.Scanning
        let mutable pos = 0
        let mutable value = ""

        while pos < s.Length && mode <> StringParserMode.End do
            let ch = s.Substring(pos, 1)

            match mode with
            | StringParserMode.Scanning ->
                if StringParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = "\"" then
                    pos <- pos + 1
                    mode <- StringParserMode.Char
                else
                    raise (new Exception(sprintf "Expected '\"', actual '%s'" ch))
            | StringParserMode.Char ->
                if ch = "\\" then
                    pos <- pos + 1
                    mode <- StringParserMode.EscapedChar
                elif ch = "\"" then
                    pos <- pos + 1
                    mode <- StringParserMode.End
                elif ch <> "\n" && ch <> "\r" then
                    pos <- pos + 1
                    value <- value + ch
                else
                    raise (new Exception(sprintf "Unexpected character '%s'" ch))
            | StringParserMode.EscapedChar ->
                if ch = "\\" || ch = "\"" || ch = "/" then
                    pos <- pos + 1
                    value <- value + ch
                    mode <- StringParserMode.Char
                elif ch = "b" then
                    pos <- pos + 1
                    value <- value + "\b"
                    mode <- StringParserMode.Char
                elif ch = "f" then
                    pos <- pos + 1
                    value <- value + "\f"
                    mode <- StringParserMode.Char
                elif ch = "n" then
                    pos <- pos + 1
                    value <- value + "\n"
                    mode <- StringParserMode.Char
                elif ch = "r" then
                    pos <- pos + 1
                    value <- value + "\r"
                    mode <- StringParserMode.Char
                elif ch = "t" then
                    pos <- pos + 1
                    value <- value + "\t"
                    mode <- StringParserMode.Char
                elif ch = "u" then
                    pos <- pos + 1
                    mode <- StringParserMode.Unicode
                else
                    raise (new Exception(sprintf "Unexpected escape character '%s'" ch))
            | StringParserMode.Unicode ->
                let slice = s.Substring(pos, 4)

                if slice.Length < 4 then
                    raise (new Exception(sprintf "Incomplete Unicode code '%s'" slice))
                else
                    let hex = Int32.Parse(slice, NumberStyles.HexNumber)
                    value <- value + Convert.ToChar(hex).ToString()
                    pos <- pos + 4
                    mode <- StringParserMode.Char
            | StringParserMode.End -> ()
            | _ -> raise (new Exception(sprintf "Unexpected mode %A" mode))

        new StringToken(pos, value)

and ValueParserMode =
    | Scanning = 0
    | Array = 1
    | False = 2
    | Null = 3
    | Number = 4
    | Object = 5
    | String = 6
    | True = 7
    | End = 8

and ValueParser =
    static member private NUMBER = new Regex("[-\\d]")
    static member private WHITESPACE = new Regex("[ \\n\\r\\t]")

    static member Parse(s: string, delimiters) : Token =
        let elements = new List<Token>()
        let mutable mode = ValueParserMode.Scanning
        let mutable pos = 0
        let mutable value: Token = null

        while pos < s.Length && mode <> ValueParserMode.End do
            let ch = s.Substring(pos, 1)

            match mode with
            | ValueParserMode.Scanning ->
                if ValueParser.WHITESPACE.IsMatch(ch) then
                    pos <- pos + 1
                elif ch = "[" then
                    mode <- ValueParserMode.Array
                elif ch = "f" then
                    mode <- ValueParserMode.False
                elif ch = "n" then
                    mode <- ValueParserMode.Null
                elif ValueParser.NUMBER.IsMatch(ch) then
                    mode <- ValueParserMode.Number
                elif ch = "{" then
                    mode <- ValueParserMode.Object
                elif ch = "\"" then
                    mode <- ValueParserMode.String
                elif ch = "t" then
                    mode <- ValueParserMode.True
                elif delimiters <> null && delimiters.IsMatch(ch) then
                    mode <- ValueParserMode.End
                else
                    raise (new Exception(sprintf "Unexpected character '%s'" ch))
            | ValueParserMode.Array ->
                let slice = s.Substring(pos)
                let token = ArrayParser.Parse(slice)
                value <- new ArrayToken(token.skip + pos, token.elements)
                mode <- ValueParserMode.End
            | ValueParserMode.False ->
                let slice = s.Substring(pos, 5)

                if slice = "false" then
                    value <- new FalseToken(pos + 5)
                    mode <- ValueParserMode.End
                else
                    raise (new Exception(sprintf "Expected 'false', actual '%s'" slice))
            | ValueParserMode.Null ->
                let slice = s.Substring(pos, 4)

                if slice = "null" then
                    value <- new NullToken(pos + 4)
                    mode <- ValueParserMode.End
                else
                    raise (new Exception(sprintf "Expected 'null', actual '%s'" slice))
            | ValueParserMode.Number ->
                let slice = s.Substring(pos)
                let token = NumberParser.Parse(slice, delimiters)
                value <- new NumberToken(token.skip + pos, token.value, token.value_as_string)
                mode <- ValueParserMode.End
            | ValueParserMode.Object ->
                let slice = s.Substring(pos)
                let token = ObjectParser.Parse(slice)
                value <- new ObjectToken(token.skip + pos, token.members)
                mode <- ValueParserMode.End
            | ValueParserMode.String ->
                let slice = s.Substring(pos)
                let stringToken = StringParser.Parse(slice)
                value <- new StringToken(stringToken.skip + pos, stringToken.value)
                mode <- ValueParserMode.End
            | ValueParserMode.True ->
                let slice = s.Substring(pos, 4)

                if slice = "true" then
                    value <- new TrueToken(pos + 4)
                    mode <- ValueParserMode.End
                else
                    raise (new Exception(sprintf "Expected 'true', actual '%s'" slice))
            | ValueParserMode.End -> ()
            | _ -> raise (new Exception(sprintf "Unexpected mode %A" mode))

        value
