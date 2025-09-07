namespace Shared

open System
open System.Collections.Generic

[<AllowNullLiteral>]
type ArrayToken(skip: int, elements: IEnumerable<Token>) =
    inherit Token(skip)
    member this.elements = elements

and FalseToken(skip: int) =
    inherit Token(skip)
    member this.value = false

and NullToken(skip: int) =
    inherit Token(skip)
    member this.value = null

and NumberToken(skip: int, value: double, value_as_string: string) =
    inherit Token(skip)
    member this.value = value
    member this.value_as_string = value_as_string

and PairToken(skip: int, key: StringToken, value: Token) =
    inherit Token(skip)
    member this.key = key
    member this.value = value

and ObjectToken(skip: int, members: IEnumerable<PairToken>) =
    inherit Token(skip)
    member this.members = members

and [<AllowNullLiteral>] StringToken(skip: int, value: string) =
    inherit Token(skip)
    member this.value = value

and [<AllowNullLiteral>] Token(skip: int) =
    member this.skip = skip

and TrueToken(skip: int) =
    inherit Token(skip)
    member this.value = true
