module ValueTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse true`` () =
    let token = JSONParser.Parse("true")
    Assert.IsType<TrueToken>(token) |> ignore

[<Fact>]
let ``Parse false`` () =
    let token = JSONParser.Parse("false")
    Assert.IsType<FalseToken>(token) |> ignore

[<Fact>]
let ``Parse null`` () =
    let token = JSONParser.Parse("null")
    Assert.IsType<NullToken>(token) |> ignore

[<Fact>]
let ``Parse complex JSON`` () =
    let json =
        """{"users": [{"name": "Alice", "age": 30}, {"name": "Bob", "age": 25}], "active": true, "count": 42}"""

    let token = JSONParser.Parse(json)
    Assert.IsType<ObjectToken>(token) |> ignore
    let objectToken = token :?> ObjectToken
    let membersList = Seq.toList objectToken.members
    Assert.Equal(3, membersList.Length)

[<Fact>]
let ``Parse deeply nested structure`` () =
    let json = """{"level1": {"level2": {"level3": [1, 2, {"deep": "value"}]}}}"""
    let token = JSONParser.Parse(json)
    Assert.IsType<ObjectToken>(token) |> ignore

[<Fact>]
let ``Parse array of objects`` () =
    let json = """[{"id": 1, "name": "Item1"}, {"id": 2, "name": "Item2"}]"""
    let token = JSONParser.Parse(json)
    Assert.IsType<ArrayToken>(token) |> ignore
    let arrayToken = token :?> ArrayToken
    let elementsList = Seq.toList arrayToken.elements
    Assert.Equal(2, elementsList.Length)
    Assert.IsType<ObjectToken>(elementsList.[0]) |> ignore
    Assert.IsType<ObjectToken>(elementsList.[1]) |> ignore

[<Fact>]
let ``Parse mixed array`` () =
    let json = """[42, "string", true, null, {"key": "value"}, [1, 2, 3]]"""
    let token = JSONParser.Parse(json)
    Assert.IsType<ArrayToken>(token) |> ignore
    let arrayToken = token :?> ArrayToken
    let elementsList = Seq.toList arrayToken.elements
    Assert.Equal(6, elementsList.Length)

[<Fact>]
let ``Parse with extra whitespace`` () =
    let json = """  {   "key"   :   "value"   ,   "number"   :   123   }   """
    let token = JSONParser.Parse(json)
    Assert.IsType<ObjectToken>(token) |> ignore
    let objectToken = token :?> ObjectToken
    let membersList = Seq.toList objectToken.members
    Assert.Equal(2, membersList.Length)
