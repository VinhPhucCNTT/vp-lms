using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Backend.Api.Core.Helpers;

// https://stackoverflow.com/questions/79265776/how-to-add-jwt-token-support-globally-in-scalar-for-a-net-9-application
internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider = authenticationSchemeProvider;

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
        if (!authenticationSchemes.Any(a => a.Name == "Bearer"))
            return;

        var bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        };

        document.Components ??= new OpenApiComponents();

        document.AddComponent("Bearer", bearerScheme);

        var securityRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        };

        foreach (var path in document.Paths.Values)
        {
            if (path.Operations is not null)
                foreach (var operation in path.Operations.Values)
                {
                    operation.Security ??= [];
                    operation.Security.Add(securityRequirement);
                }
        }
    }
}
