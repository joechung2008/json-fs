module ValueTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse true`` () =
    match JSONParser.Parse("true") with
    | Ok (TrueToken(_)) -> ()
    | _ -> Assert.Fail("Expected Ok TrueToken")

[<Fact>]
let ``Parse false`` () =
    match JSONParser.Parse("false") with
    | Ok (FalseToken(_)) -> ()
    | _ -> Assert.Fail("Expected Ok FalseToken")

[<Fact>]
let ``Parse null`` () =
    match JSONParser.Parse("null") with
    | Ok (NullToken(_)) -> ()
    | _ -> Assert.Fail("Expected Ok NullToken")

[<Fact>]
let ``Parse complex JSON`` () =
    let json =
        """{"users": [{"name": "Alice", "age": 30}, {"name": "Bob", "age": 25}], "active": true, "count": 42}"""

    match JSONParser.Parse(json) with
    | Ok (ObjectToken(_, members)) ->
        Assert.Equal(3, List.length members)
    | _ -> Assert.Fail("Expected Ok ObjectToken")

[<Fact>]
let ``Parse deeply nested structure`` () =
    let json = """{"level1": {"level2": {"level3": [1, 2, {"deep": "value"}]}}}"""
    match JSONParser.Parse(json) with
    | Ok (ObjectToken(_, _)) -> ()
    | _ -> Assert.Fail("Expected Ok ObjectToken")

[<Fact>]
let ``Parse array of objects`` () =
    let json = """[{"id": 1, "name": "Item1"}, {"id": 2, "name": "Item2"}]"""
    match JSONParser.Parse(json) with
    | Ok (ArrayToken(_, elements)) ->
        Assert.Equal(2, List.length elements)
        match elements with
        | [ObjectToken(_, _); ObjectToken(_, _)] -> ()
        | _ -> Assert.Fail("Expected two ObjectTokens")
    | _ -> Assert.Fail("Expected Ok ArrayToken")

[<Fact>]
let ``Parse mixed array`` () =
    let json = """[42, "string", true, null, {"key": "value"}, [1, 2, 3]]"""
    match JSONParser.Parse(json) with
    | Ok (ArrayToken(_, elements)) ->
        Assert.Equal(6, List.length elements)
    | _ -> Assert.Fail("Expected Ok ArrayToken")

[<Fact>]
let ``Parse with extra whitespace`` () =
    let json = """  {   "key"   :   "value"   ,   "number"   :   123   }   """
    match JSONParser.Parse(json) with
    | Ok (ObjectToken(_, members)) ->
        Assert.Equal(2, List.length members)
    | _ -> Assert.Fail("Expected Ok ObjectToken")

[<Fact>]
let ``Parse invalid JSON`` () =
    match JSONParser.Parse("{invalid") with
    | Error _ -> ()
    | _ -> Assert.Fail("Expected Error")
