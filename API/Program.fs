open System
open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi.Models

type ErrorResponse = { error: string; code: int }

module Program =
    open System.Text.Json
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddEndpointsApiExplorer() |> ignore

        builder.Services.AddSwaggerGen(fun options ->
            options.SwaggerDoc(
                "v2",
                OpenApiInfo(
                    Title = "API (Swagger 2.0)",
                    Version = "2.0",
                    Description = "OpenAPI 2.0 (Swagger 2.0) documentation"
                )
            )

            options.SwaggerDoc(
                "v3",
                OpenApiInfo(Title = "API (OpenAPI 3.0)", Version = "3.0", Description = "OpenAPI 3.0 documentation")
            ))
        |> ignore

        let app = builder.Build()

        if app.Environment.IsDevelopment() then
            app.UseSwagger() |> ignore

            app.UseSwaggerUI(fun options ->
                options.SwaggerEndpoint("/swagger/v2/swagger.json", "API (Swagger 2.0)")
                options.SwaggerEndpoint("/swagger/v3/swagger.json", "API (OpenAPI 3.0)"))
            |> ignore

        let handler =
            Func<HttpRequest, Task<IResult>>(fun (request: HttpRequest) ->
                task {
                    if isNull request.ContentType || request.ContentType <> "text/plain" then
                        let error =
                            { error = "Unsupported Media Type"
                              code = 415 }

                        return Results.Json<ErrorResponse>(error, statusCode = 415)
                    else
                        use reader = new StreamReader(request.Body)
                        let! body = reader.ReadToEndAsync()

                        match Shared.JSONParser.Parse(body) with
                        | Ok token ->
                            let options = JsonSerializerOptions()
                            options.Converters.Add(Shared.JsonValueConverter())

                            let jsonString =
                                JsonSerializer.Serialize(Shared.JsonSerializer.tokenToJson token, options)

                            return Results.Content(jsonString, "application/json")
                        | Error msg ->
                            let error = { error = msg; code = 400 }
                            return Results.Json<ErrorResponse>(error, statusCode = 400)
                })

        let route = app.MapPost("/api/v1/parse", handler)
        route.WithDisplayName "Parse JSON" |> ignore
        route.WithName "ParseJSON" |> ignore
        route.WithSummary "Parse a JSON string" |> ignore
        route.WithTags "JSON" |> ignore

        route.WithDescription "Parses a JSON string and returns its structured representation."
        |> ignore

        route.Accepts<string> "text/plain" |> ignore
        route.Produces<string>(200, "application/json") |> ignore
        route.Produces<ErrorResponse>(400, "application/json") |> ignore
        route.Produces<ErrorResponse>(415, "application/json") |> ignore

        app.Run()

        exitCode
