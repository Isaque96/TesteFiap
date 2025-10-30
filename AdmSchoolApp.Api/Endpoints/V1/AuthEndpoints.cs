namespace AdmSchoolApp.Endpoints.V1;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth").WithTags("Autenticação");

        group.MapPost("/login", async () =>
        {
            return Results.Ok();
        });
        
        return group;
    }
}