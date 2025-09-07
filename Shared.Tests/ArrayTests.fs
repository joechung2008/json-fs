module ArrayTests

open System
open Xunit
open Shared

[<Fact>]
let ``Parse empty array`` () =
    match JSONParser.Parse("[]") with
    | Ok(ArrayToken(_, elements)) -> Assert.Equal(0, List.length elements)
    | _ -> Assert.Fail("Expected Ok ArrayToken")

[<Fact>]
let ``Parse array with numbers`` () =
    match JSONParser.Parse("[1, 2, 3]") with
    | Ok(ArrayToken(_, elements)) ->
        Assert.Equal(3, List.length elements)

        match elements with
        | [ NumberToken(_, _, _); NumberToken(_, _, _); NumberToken(_, _, _) ] -> ()
        | _ -> Assert.Fail("Expected three NumberTokens")
    | _ -> Assert.Fail("Expected Ok ArrayToken")

[<Fact>]
let ``Parse array with strings`` () =
    match JSONParser.Parse("""["hello", "world"]""") with
    | Ok(ArrayToken(_, elements)) ->
        Assert.Equal(2, List.length elements)

        match elements with
        | [ StringToken(_, _); StringToken(_, _) ] -> ()
        | _ -> Assert.Fail("Expected two StringTokens")
    | _ -> Assert.Fail("Expected Ok ArrayToken")

[<Fact>]
let ``Parse nested arrays`` () =
    match JSONParser.Parse("[[1, 2], [3, 4]]") with
    | Ok(ArrayToken(_, elements)) ->
        Assert.Equal(2, List.length elements)

        match elements with
        | [ ArrayToken(_, _); ArrayToken(_, _) ] -> ()
        | _ -> Assert.Fail("Expected two ArrayTokens")
    | _ -> Assert.Fail("Expected Ok ArrayToken")

[<Fact>]
let ``Parse array with mixed types`` () =
    match JSONParser.Parse("""[1, "hello", true, null]""") with
    | Ok(ArrayToken(_, elements)) ->
        Assert.Equal(4, List.length elements)

        match elements with
        | [ NumberToken(_, _, _); StringToken(_, _); TrueToken(_); NullToken(_) ] -> ()
        | _ -> Assert.Fail("Expected mixed types")
    | _ -> Assert.Fail("Expected Ok ArrayToken")

[<Fact>]
let ``Parse array with whitespace`` () =
    match JSONParser.Parse("  [  1  ,  2  ]  ") with
    | Ok(ArrayToken(_, elements)) -> Assert.Equal(2, List.length elements)
    | _ -> Assert.Fail("Expected Ok ArrayToken")
