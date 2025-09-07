module StringTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse simple string`` () =
    let token = JSONParser.Parse("\"hello\"")
    Assert.IsType<StringToken>(token) |> ignore
    let stringToken = token :?> StringToken
    Assert.Equal("hello", stringToken.value)

[<Fact>]
let ``Parse empty string`` () =
    let token = JSONParser.Parse("\"\"")
    Assert.IsType<StringToken>(token) |> ignore
    let stringToken = token :?> StringToken
    Assert.Equal("", stringToken.value)

[<Fact>]
let ``Parse string with spaces`` () =
    let token = JSONParser.Parse("\"hello world\"")
    Assert.IsType<StringToken>(token) |> ignore
    let stringToken = token :?> StringToken
    Assert.Equal("hello world", stringToken.value)

[<Fact>]
let ``Parse string with escaped quote`` () =
    let token = JSONParser.Parse("\"He said \\\"hello\\\"")
    Assert.IsType<StringToken>(token) |> ignore
    let stringToken = token :?> StringToken
    Assert.Equal("He said \"hello\"", stringToken.value)

[<Fact>]
let ``Parse string with backslash`` () =
    let token = JSONParser.Parse("\"path\\\\to\\\\file\"")
    Assert.IsType<StringToken>(token) |> ignore
    let stringToken = token :?> StringToken
    Assert.Equal("path\\to\\file", stringToken.value)

[<Fact>]
let ``Parse string with newline escape`` () =
    let token = JSONParser.Parse("\"line1\\nline2\"")
    Assert.IsType<StringToken>(token) |> ignore
    let stringToken = token :?> StringToken
    Assert.Equal("line1\nline2", stringToken.value)

[<Fact>]
let ``Parse string with tab escape`` () =
    let token = JSONParser.Parse("\"col1\\tcol2\"")
    Assert.IsType<StringToken>(token) |> ignore
    let stringToken = token :?> StringToken
    Assert.Equal("col1\tcol2", stringToken.value)

[<Fact>]
let ``Parse string with Unicode escape`` () =
    let token = JSONParser.Parse("\"\\u0041\"")
    Assert.IsType<StringToken>(token) |> ignore
    let stringToken = token :?> StringToken
    Assert.Equal("A", stringToken.value)

[<Fact>]
let ``Parse string with forward slash`` () =
    let token = JSONParser.Parse("\"http:\\/\\/example.com\"")
    Assert.IsType<StringToken>(token) |> ignore
    let stringToken = token :?> StringToken
    Assert.Equal("http://example.com", stringToken.value)
