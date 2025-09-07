module ObjectTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse empty object`` () =
    match JSONParser.Parse("{}") with
    | Ok (ObjectToken(_, members)) ->
        Assert.Equal(0, List.length members)
    | _ -> Assert.Fail("Expected Ok ObjectToken")

[<Fact>]
let ``Parse object with single pair`` () =
    match JSONParser.Parse("""{"key": "value"}""") with
    | Ok (ObjectToken(_, members)) ->
        Assert.Equal(1, List.length members)
        match members with
        | [pair] ->
            match pair.key with
            | StringToken(_, keyValue) -> Assert.Equal("key", keyValue)
            | _ -> Assert.Fail("Expected StringToken for key")
            match pair.value with
            | StringToken(_, valueValue) -> Assert.Equal("value", valueValue)
            | _ -> Assert.Fail("Expected StringToken for value")
        | _ -> Assert.Fail("Expected one pair")
    | _ -> Assert.Fail("Expected Ok ObjectToken")

[<Fact>]
let ``Parse object with multiple pairs`` () =
    match JSONParser.Parse("""{"name": "John", "age": 30}""") with
    | Ok (ObjectToken(_, members)) ->
        Assert.Equal(2, List.length members)
        match members with
        | [pair1; pair2] ->
            match pair1.key with
            | StringToken(_, keyValue) -> Assert.Equal("name", keyValue)
            | _ -> Assert.Fail("Expected StringToken for key")
            match pair2.key with
            | StringToken(_, keyValue) -> Assert.Equal("age", keyValue)
            | _ -> Assert.Fail("Expected StringToken for key")
        | _ -> Assert.Fail("Expected two pairs")
    | _ -> Assert.Fail("Expected Ok ObjectToken")

[<Fact>]
let ``Parse nested objects`` () =
    match JSONParser.Parse("""{"person": {"name": "Jane", "age": 25}}""") with
    | Ok (ObjectToken(_, members)) ->
        Assert.Equal(1, List.length members)
        match members with
        | [pair] ->
            match pair.value with
            | ObjectToken(_, _) -> ()
            | _ -> Assert.Fail("Expected ObjectToken for value")
        | _ -> Assert.Fail("Expected one pair")
    | _ -> Assert.Fail("Expected Ok ObjectToken")

[<Fact>]
let ``Parse object with array value`` () =
    match JSONParser.Parse("""{"numbers": [1, 2, 3]}""") with
    | Ok (ObjectToken(_, members)) ->
        Assert.Equal(1, List.length members)
        match members with
        | [pair] ->
            match pair.value with
            | ArrayToken(_, _) -> ()
            | _ -> Assert.Fail("Expected ArrayToken for value")
        | _ -> Assert.Fail("Expected one pair")
    | _ -> Assert.Fail("Expected Ok ObjectToken")

[<Fact>]
let ``Parse object with mixed value types`` () =
    match JSONParser.Parse("""{"string": "hello", "number": 42, "boolean": true, "null": null}""") with
    | Ok (ObjectToken(_, members)) ->
        Assert.Equal(4, List.length members)
        match members with
        | [pair1; pair2; pair3; pair4] ->
            match pair1.value with
            | StringToken(_, _) -> ()
            | _ -> Assert.Fail("Expected StringToken")
            match pair2.value with
            | NumberToken(_, _, _) -> ()
            | _ -> Assert.Fail("Expected NumberToken")
            match pair3.value with
            | TrueToken(_) -> ()
            | _ -> Assert.Fail("Expected TrueToken")
            match pair4.value with
            | NullToken(_) -> ()
            | _ -> Assert.Fail("Expected NullToken")
        | _ -> Assert.Fail("Expected four pairs")
    | _ -> Assert.Fail("Expected Ok ObjectToken")

[<Fact>]
let ``Parse object with whitespace`` () =
    match JSONParser.Parse("  {  \"key\"  :  \"value\"  }  ") with
    | Ok (ObjectToken(_, members)) ->
        Assert.Equal(1, List.length members)
    | _ -> Assert.Fail("Expected Ok ObjectToken")
