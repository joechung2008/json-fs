module ObjectTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse empty object`` () =
    let token = JSONParser.Parse("{}")
    Assert.IsType<ObjectToken>(token) |> ignore
    let objectToken = token :?> ObjectToken
    let membersList = Seq.toList objectToken.members
    Assert.Equal(0, membersList.Length)

[<Fact>]
let ``Parse object with single pair`` () =
    let token = JSONParser.Parse("""{"key": "value"}""")
    Assert.IsType<ObjectToken>(token) |> ignore
    let objectToken = token :?> ObjectToken
    let membersList = Seq.toList objectToken.members
    Assert.Equal(1, membersList.Length)
    Assert.Equal("key", membersList.[0].key.value)
    Assert.IsType<StringToken>(membersList.[0].value) |> ignore

[<Fact>]
let ``Parse object with multiple pairs`` () =
    let token = JSONParser.Parse("""{"name": "John", "age": 30}""")
    Assert.IsType<ObjectToken>(token) |> ignore
    let objectToken = token :?> ObjectToken
    let membersList = Seq.toList objectToken.members
    Assert.Equal(2, membersList.Length)
    Assert.Equal("name", membersList.[0].key.value)
    Assert.Equal("age", membersList.[1].key.value)

[<Fact>]
let ``Parse nested objects`` () =
    let token = JSONParser.Parse("""{"person": {"name": "Jane", "age": 25}}""")
    Assert.IsType<ObjectToken>(token) |> ignore
    let objectToken = token :?> ObjectToken
    let membersList = Seq.toList objectToken.members
    Assert.Equal(1, membersList.Length)
    Assert.IsType<ObjectToken>(membersList.[0].value) |> ignore

[<Fact>]
let ``Parse object with array value`` () =
    let token = JSONParser.Parse("""{"numbers": [1, 2, 3]}""")
    Assert.IsType<ObjectToken>(token) |> ignore
    let objectToken = token :?> ObjectToken
    let membersList = Seq.toList objectToken.members
    Assert.Equal(1, membersList.Length)
    Assert.IsType<ArrayToken>(membersList.[0].value) |> ignore

[<Fact>]
let ``Parse object with mixed value types`` () =
    let token =
        JSONParser.Parse("""{"string": "hello", "number": 42, "boolean": true, "null": null}""")

    Assert.IsType<ObjectToken>(token) |> ignore
    let objectToken = token :?> ObjectToken
    let membersList = Seq.toList objectToken.members
    Assert.Equal(4, membersList.Length)
    Assert.IsType<StringToken>(membersList.[0].value) |> ignore
    Assert.IsType<NumberToken>(membersList.[1].value) |> ignore
    Assert.IsType<TrueToken>(membersList.[2].value) |> ignore
    Assert.IsType<NullToken>(membersList.[3].value) |> ignore

[<Fact>]
let ``Parse object with whitespace`` () =
    let token = JSONParser.Parse("  {  \"key\"  :  \"value\"  }  ")
    Assert.IsType<ObjectToken>(token) |> ignore
    let objectToken = token :?> ObjectToken
    let membersList = Seq.toList objectToken.members
    Assert.Equal(1, membersList.Length)
