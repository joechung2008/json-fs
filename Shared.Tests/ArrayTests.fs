module ArrayTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse empty array`` () =
    let token = JSONParser.Parse("[]")
    Assert.IsType<ArrayToken>(token) |> ignore
    let arrayToken = token :?> ArrayToken
    let elementsList = Seq.toList arrayToken.elements
    Assert.Equal(0, elementsList.Length)

[<Fact>]
let ``Parse array with numbers`` () =
    let token = JSONParser.Parse("[1, 2, 3]")
    Assert.IsType<ArrayToken>(token) |> ignore
    let arrayToken = token :?> ArrayToken
    let elementsList = Seq.toList arrayToken.elements
    Assert.Equal(3, elementsList.Length)
    Assert.IsType<NumberToken>(elementsList.[0]) |> ignore
    Assert.IsType<NumberToken>(elementsList.[1]) |> ignore
    Assert.IsType<NumberToken>(elementsList.[2]) |> ignore

[<Fact>]
let ``Parse array with strings`` () =
    let token = JSONParser.Parse("""["hello", "world"]""")
    Assert.IsType<ArrayToken>(token) |> ignore
    let arrayToken = token :?> ArrayToken
    let elementsList = Seq.toList arrayToken.elements
    Assert.Equal(2, elementsList.Length)
    Assert.IsType<StringToken>(elementsList.[0]) |> ignore
    Assert.IsType<StringToken>(elementsList.[1]) |> ignore

[<Fact>]
let ``Parse nested arrays`` () =
    let token = JSONParser.Parse("[[1, 2], [3, 4]]")
    Assert.IsType<ArrayToken>(token) |> ignore
    let arrayToken = token :?> ArrayToken
    let elementsList = Seq.toList arrayToken.elements
    Assert.Equal(2, elementsList.Length)
    Assert.IsType<ArrayToken>(elementsList.[0]) |> ignore
    Assert.IsType<ArrayToken>(elementsList.[1]) |> ignore

[<Fact>]
let ``Parse array with mixed types`` () =
    let token = JSONParser.Parse("""[1, "hello", true, null]""")
    Assert.IsType<ArrayToken>(token) |> ignore
    let arrayToken = token :?> ArrayToken
    let elementsList = Seq.toList arrayToken.elements
    Assert.Equal(4, elementsList.Length)
    Assert.IsType<NumberToken>(elementsList.[0]) |> ignore
    Assert.IsType<StringToken>(elementsList.[1]) |> ignore
    Assert.IsType<TrueToken>(elementsList.[2]) |> ignore
    Assert.IsType<NullToken>(elementsList.[3]) |> ignore

[<Fact>]
let ``Parse array with whitespace`` () =
    let token = JSONParser.Parse("  [  1  ,  2  ]  ")
    Assert.IsType<ArrayToken>(token) |> ignore
    let arrayToken = token :?> ArrayToken
    let elementsList = Seq.toList arrayToken.elements
    Assert.Equal(2, elementsList.Length)
