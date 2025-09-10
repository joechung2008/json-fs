namespace Shared

open System.Text.Json
open System.Text.Json.Serialization

type JsonValueConverter() =
    inherit JsonConverter<obj>()

    override _.CanConvert(typeToConvert: System.Type) = typeToConvert = typeof<obj>

    override _.Read(reader: byref<Utf8JsonReader>, typeToConvert: System.Type, options: JsonSerializerOptions) =
        failwith "Not implemented"

    override this.Write(writer: Utf8JsonWriter, value: obj, options: JsonSerializerOptions) =
        match value with
        | :? (obj list) as items ->
            writer.WriteStartArray()

            for item in items do
                this.Write(writer, item, options)

            writer.WriteEndArray()
        | :? ((string * obj) list) as pairs ->
            writer.WriteStartObject()

            for key, value in pairs do
                writer.WritePropertyName(key)
                this.Write(writer, value, options)

            writer.WriteEndObject()
        | :? string as s -> writer.WriteStringValue(s)
        | :? bool as b -> writer.WriteBooleanValue(b)
        | :? double as d -> writer.WriteNumberValue(d)
        | null -> writer.WriteNullValue()
        | _ -> writer.WriteStringValue(string value)

module JsonSerializer =
    let rec tokenToJson (token: Token) : obj =
        match token with
        | ArrayToken(_, elements) -> box (elements |> List.map tokenToJson)
        | FalseToken _ -> box false
        | NullToken _ -> null
        | NumberToken(_, value, _) -> box value
        | ObjectToken(_, members) ->
            box (
                members
                |> List.map (fun m ->
                    match tokenToJson m.key with
                    | :? string as keyStr -> keyStr, tokenToJson m.value
                    | _ -> failwith "Object keys must be strings")
            )
        | StringToken(_, value) -> box value
        | TrueToken _ -> box true

    let serializeToken (token: Token) : string =
        let jsonObj = tokenToJson token
        let options = JsonSerializerOptions()
        options.Converters.Add(JsonValueConverter())
        JsonSerializer.Serialize(jsonObj, options)
