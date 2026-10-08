using LocationInfoService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ILocationRepository, PostgresLocationRepository>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/locations/{location}", async (string location, ILocationRepository repository, CancellationToken ct) =>
    {
        if (string.IsNullOrWhiteSpace(location) || location.Length > LocationRepositoryLimits.MaxLocationLength)
        {
            return Results.Problem(
                $"Location must be 1-{LocationRepositoryLimits.MaxLocationLength} characters.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await repository.GetDescriptionAsync(location.Trim(), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    })
    .WithName("GetLocationDescription")
    .WithSummary("Returns the stored text for the given location.")
    .Produces<LocationResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapGet("/api/health", () => Results.Ok("ok")).ExcludeFromDescription();

app.Run();
