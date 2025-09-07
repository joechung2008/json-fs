module StringTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse simple string`` () =
    match JSONParser.Parse("\"hello\"") with
    | Ok(StringToken(_, value)) -> Assert.Equal("hello", value)
    | _ -> Assert.Fail("Expected Ok StringToken")

[<Fact>]
let ``Parse empty string`` () =
    match JSONParser.Parse("\"\"") with
    | Ok(StringToken(_, value)) -> Assert.Equal("", value)
    | _ -> Assert.Fail("Expected Ok StringToken")

[<Fact>]
let ``Parse string with spaces`` () =
    match JSONParser.Parse("\"hello world\"") with
    | Ok(StringToken(_, value)) -> Assert.Equal("hello world", value)
    | _ -> Assert.Fail("Expected Ok StringToken")

[<Fact>]
let ``Parse string with escaped quote`` () =
    match JSONParser.Parse("\"He said \\\"hello\\\"\"") with
    | Ok(StringToken(_, value)) -> Assert.Equal("He said \"hello\"", value)
    | _ -> Assert.Fail("Expected Ok StringToken")

[<Fact>]
let ``Parse string with backslash`` () =
    match JSONParser.Parse("\"path\\\\to\\\\file\"") with
    | Ok(StringToken(_, value)) -> Assert.Equal("path\\to\\file", value)
    | _ -> Assert.Fail("Expected Ok StringToken")

[<Fact>]
let ``Parse string with newline escape`` () =
    match JSONParser.Parse("\"line1\\nline2\"") with
    | Ok(StringToken(_, value)) -> Assert.Equal("line1\nline2", value)
    | _ -> Assert.Fail("Expected Ok StringToken")

[<Fact>]
let ``Parse string with tab escape`` () =
    match JSONParser.Parse("\"col1\\tcol2\"") with
    | Ok(StringToken(_, value)) -> Assert.Equal("col1\tcol2", value)
    | _ -> Assert.Fail("Expected Ok StringToken")

[<Fact>]
let ``Parse string with Unicode escape`` () =
    match JSONParser.Parse("\"\\u0041\"") with
    | Ok(StringToken(_, value)) -> Assert.Equal("A", value)
    | _ -> Assert.Fail("Expected Ok StringToken")

[<Fact>]
let ``Parse string with forward slash`` () =
    match JSONParser.Parse("\"http:\\/\\/example.com\"") with
    | Ok(StringToken(_, value)) -> Assert.Equal("http://example.com", value)
    | _ -> Assert.Fail("Expected Ok StringToken")
