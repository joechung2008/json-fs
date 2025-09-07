module NumberTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse positive integer`` () =
    match JSONParser.Parse("123") with
    | Ok(NumberToken(_, value, _)) -> Assert.Equal(123.0, value)
    | _ -> Assert.Fail("Expected Ok NumberToken")

[<Fact>]
let ``Parse negative integer`` () =
    match JSONParser.Parse("-456") with
    | Ok(NumberToken(_, value, _)) -> Assert.Equal(-456.0, value)
    | _ -> Assert.Fail("Expected Ok NumberToken")

[<Fact>]
let ``Parse zero`` () =
    match JSONParser.Parse("0") with
    | Ok(NumberToken(_, value, _)) -> Assert.Equal(0.0, value)
    | _ -> Assert.Fail("Expected Ok NumberToken")

[<Fact>]
let ``Parse decimal number`` () =
    match JSONParser.Parse("78.9") with
    | Ok(NumberToken(_, value, _)) -> Assert.Equal(78.9, value)
    | _ -> Assert.Fail("Expected Ok NumberToken")

[<Fact>]
let ``Parse number with exponent`` () =
    match JSONParser.Parse("1e10") with
    | Ok(NumberToken(_, value, _)) -> Assert.Equal(10000000000.0, value)
    | _ -> Assert.Fail("Expected Ok NumberToken")

[<Fact>]
let ``Parse number with negative exponent`` () =
    match JSONParser.Parse("1.23e-4") with
    | Ok(NumberToken(_, value, _)) -> Assert.Equal(0.000123, value)
    | _ -> Assert.Fail("Expected Ok NumberToken")

[<Fact>]
let ``Parse number with positive exponent sign`` () =
    match JSONParser.Parse("2.5e+3") with
    | Ok(NumberToken(_, value, _)) -> Assert.Equal(2500.0, value)
    | _ -> Assert.Fail("Expected Ok NumberToken")

[<Fact>]
let ``Parse large number`` () =
    match JSONParser.Parse("999999999999999") with
    | Ok(NumberToken(_, value, _)) -> Assert.Equal(999999999999999.0, value)
    | _ -> Assert.Fail("Expected Ok NumberToken")
