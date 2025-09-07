namespace Shared

open System

type Token =
    | ArrayToken of skip: int * elements: Token list
    | FalseToken of skip: int
    | NullToken of skip: int
    | NumberToken of skip: int * value: double * valueAsString: string
    | ObjectToken of skip: int * members: PairToken list
    | StringToken of skip: int * value: string
    | TrueToken of skip: int

and PairToken = { skip: int; key: Token; value: Token }
