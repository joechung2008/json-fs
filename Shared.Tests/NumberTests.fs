module NumberTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse positive integer`` () =
    let token = JSONParser.Parse("123")
    Assert.IsType<NumberToken>(token) |> ignore
    let numberToken = token :?> NumberToken
    Assert.Equal(123.0, numberToken.value)

[<Fact>]
let ``Parse negative integer`` () =
    let token = JSONParser.Parse("-456")
    Assert.IsType<NumberToken>(token) |> ignore
    let numberToken = token :?> NumberToken
    Assert.Equal(-456.0, numberToken.value)

[<Fact>]
let ``Parse zero`` () =
    let token = JSONParser.Parse("0")
    Assert.IsType<NumberToken>(token) |> ignore
    let numberToken = token :?> NumberToken
    Assert.Equal(0.0, numberToken.value)

[<Fact>]
let ``Parse decimal number`` () =
    let token = JSONParser.Parse("78.9")
    Assert.IsType<NumberToken>(token) |> ignore
    let numberToken = token :?> NumberToken
    Assert.Equal(78.9, numberToken.value)

[<Fact>]
let ``Parse number with exponent`` () =
    let token = JSONParser.Parse("1e10")
    Assert.IsType<NumberToken>(token) |> ignore
    let numberToken = token :?> NumberToken
    Assert.Equal(10000000000.0, numberToken.value)

[<Fact>]
let ``Parse number with negative exponent`` () =
    let token = JSONParser.Parse("1.23e-4")
    Assert.IsType<NumberToken>(token) |> ignore
    let numberToken = token :?> NumberToken
    Assert.Equal(0.000123, numberToken.value)

[<Fact>]
let ``Parse number with positive exponent sign`` () =
    let token = JSONParser.Parse("2.5e+3")
    Assert.IsType<NumberToken>(token) |> ignore
    let numberToken = token :?> NumberToken
    Assert.Equal(2500.0, numberToken.value)

[<Fact>]
let ``Parse large number`` () =
    let token = JSONParser.Parse("999999999999999")
    Assert.IsType<NumberToken>(token) |> ignore
    let numberToken = token :?> NumberToken
    Assert.Equal(999999999999999.0, numberToken.value)
