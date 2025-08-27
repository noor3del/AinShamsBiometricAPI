using AinShamsBiometric.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Neurotec.Licensing;
using Scalar.AspNetCore;

#region Obtain Neurotechnology Licenses

// --- This is the new section for obtaining licenses on startup ---
const string Address = "/local"; // Use "/local" for local license server
const string Port = "5000";    // Default port for local server

// All licenses your application might need
string[] licenses =
{
    "Biometrics.FaceDetectionBase",
    "Biometrics.FaceDetection",
    "Biometrics.FaceSegmentation",
    "Biometrics.Standards.Faces",


};

try
{
    Console.WriteLine("Obtaining Neurotechnology licenses...");
    foreach (var license in licenses)
    {
        NLicense.ObtainComponents(Address, Port, license);
    }
    Console.WriteLine("All licenses obtained successfully.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FATAL: Failed to obtain Neurotechnology licenses. The application cannot start.");
    Console.Error.WriteLine($"Error: {ex.Message}");
    // Optionally, prevent the app from starting if licenses are critical
    // return; 
}
// --- End of new section ---

#endregion


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "AinShams Biometric System API",
            Version = "1.0.0",
            TermsOfService = new Uri("https://nasps.org.eg/terms"),
            Contact = new OpenApiContact
            {
                Email = "n.ahmed@nasps.org.eg",

                Name = "AinShams Biometric API Support",
                Url = new Uri("https://nasps.org.eg")
            },
            License = new OpenApiLicense
            {
                Name = "Source Code",
                Url = new Uri("https://github.com/noor3del/AinShamsBiometricAPI")
            },
            Description =
    """
    > This API provides detailed biometric services for students: ICAO compliance checks, fingerprint capture, and national ID scans.  

    > It supports advanced filtering options and is designed for seamless integration with your applications.

 
    ### Postman Collection:

    ---
    [![Run in Postman](https://run.pstmn.io/button.svg)](https://god.gw.postman.com/run-collection/9136127-55d2bde1-a248-473f-95b5-64cfd02fb445?action=collection%2Ffork&collection-url=entityId%3D9136127-55d2bde1-a248-473f-95b5-64cfd02fb445%26entityType%3Dcollection%26workspaceId%3D78beee89-4238-4c5f-bd1f-7e98978744b4#?env%5BBolt%20Sandbox%20Environment%5D=W3sia2V5IjoiYXBpX2Jhc2VfdXJsIiwidmFsdWUiOiJodHRwczovL2FwaS1zYW5kYm94LmJvbHQuY29tIiwidHlwZSI6ImRlZmF1bHQiLCJlbmFibGVkIjp0cnVlfSx7ImtleSI6InRrX2Jhc2UiLCJ2YWx1ZSI6Imh0dHBzOi8vc2FuZGJveC5ib2x0a2suY29tIiwidHlwZSI6ImRlZmF1bHQiLCJlbmFibGVkIjp0cnVlfSx7ImtleSI6ImFwaV9rZXkiLCJ2YWx1ZSI6IjxyZXBsYWNlIHdpdGggeW91ciBCb2x0IFNhbmRib3ggQVBJIGtleT4iLCJ0eXBlIjoic2VjcmV0IiwiZW5hYmxlZCI6dHJ1ZX0seyJrZXkiOiJwdWJsaXNoYWJsZV9rZXkiLCJ2YWx1ZSI6IjxyZXBsYWNlIHdpdGggeW91ciBCb2x0IFNhbmRib3ggcHVibGlzaGFibGUga2V5PiIsInR5cGUiOiJkZWZhdWx0IiwiZW5hYmxlZCI6dHJ1ZX0seyJrZXkiOiJkaXZpc2lvbl9pZCIsInZhbHVlIjoiPHJlcGxhY2Ugd2l0aCB5b3VyIEJvbHQgU2FuZGJveCBwdWJsaWMgZGl2aXNpb24gSUQ+IiwidHlwZSI6ImRlZmF1bHQiLCJlbmFibGVkIjp0cnVlfV0=)
    """


            //Description = "### Contact Information:\n" +
            //              "> **Name**: Nour Adel\n\n" +
            //              "> **Email**: [nour3dell@gmail.com](mailto:nour3dell@gmail.com)\n\n" +
            //              "> **Website**: [https://nour3adel.com](https://nour3adel.com)\n\n" +
            //              "This API provides detailed weather forecasts with various endpoints to access current, hourly, and daily forecast data. " +
            //              "It supports advanced filtering options and is designed for seamless integration with your applications." +
            //              """
            //               Postman Collection:
            //               [![](https://run.pstmn.io/button.svg)](https://god.gw.postman.com/run-collection/9136127-55d2bde1-a248-473f-95b5-64cfd02fb445?action=collection%2Ffork&collection-url=entityId%3D9136127-55d2bde1-a248-473f-95b5-64cfd02fb445%26entityType%3Dcollection%26workspaceId%3D78beee89-4238-4c5f-bd1f-7e98978744b4#?env%5BBolt%20Sandbox%20Environment%5D=W3sia2V5IjoiYXBpX2Jhc2VfdXJsIiwidmFsdWUiOiJodHRwczovL2FwaS1zYW5kYm94LmJvbHQuY29tIiwidHlwZSI6ImRlZmF1bHQiLCJlbmFibGVkIjp0cnVlfSx7ImtleSI6InRrX2Jhc2UiLCJ2YWx1ZSI6Imh0dHBzOi8vc2FuZGJveC5ib2x0dGsuY29tIiwidHlwZSI6ImRlZmF1bHQiLCJlbmFibGVkIjp0cnVlfSx7ImtleSI6ImFwaV9rZXkiLCJ2YWx1ZSI6IjxyZXBsYWNlIHdpdGggeW91ciBCb2x0IFNhbmRib3ggQVBJIGtleT4iLCJ0eXBlIjoic2VjcmV0IiwiZW5hYmxlZCI6dHJ1ZX0seyJrZXkiOiJwdWJsaXNoYWJsZV9rZXkiLCJ2YWx1ZSI6IjxyZXBsYWNlIHdpdGggeW91ciBCb2x0IFNhbmRib3ggcHVibGlzaGFibGUga2V5PiIsInR5cGUiOiJkZWZhdWx0IiwiZW5hYmxlZCI6dHJ1ZX0seyJrZXkiOiJkaXZpc2lvbl9pZCIsInZhbHVlIjoiPHJlcGxhY2Ugd2l0aCB5b3VyIEJvbHQgU2FuZGJveCBwdWJsaWMgZGl2aXNpb24gSUQ+IiwidHlwZSI6ImRlZmF1bHQiLCJlbmFibGVkIjp0cnVlfV0=)
            //              """
        };

        return Task.CompletedTask;
    });
});
builder.Services.Configure<FormOptions>(o =>
{
    o.ValueCountLimit = int.MaxValue;                 // allow many form keys
    o.MultipartBodyLengthLimit = 100 * 1024 * 1024;   // 100 MB
    o.MultipartHeadersCountLimit = 128;
    o.MultipartHeadersLengthLimit = int.MaxValue;
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Authority = "https://YOUR-IDP/";
        o.Audience = "ainshams.biometric.api";
    });

builder.Services.AddAuthorization();

#region Allow CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

#endregion

#region SQL SERVER CONNECTION



#endregion

#region Dependency injections

builder.Services.AddServiceDependencies();

#endregion

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    #region  Swagger 
    app.UseSwaggerUI(options =>
    {

        options.SwaggerEndpoint("/openapi/v1.json", "v1");

    });
    #endregion

    #region Scalar

    app.MapScalarApiReference(options =>
    {

        options
        .WithTitle("AinShams Biometric API")
        .WithTheme(ScalarTheme.Kepler) //Kepler, Purple, Saturn, Alternate, Mars, BluePlanet,DeepSpace, Blue
        .WithLayout(ScalarLayout.Modern) //Classic, Modern
        .WithDarkMode(true)
        .WithDarkModeToggle(true)
        .WithDefaultFonts(false)
        .WithTagSorter(TagSorter.Alpha)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithClientButton(false)
         .WithFavicon("/logo/logo.png");
    });
    #endregion
}

app.MapControllers();

app.Run();

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemes)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var all = await schemes.GetAllSchemesAsync();
        if (all.Any(a => a.Name == JwtBearerDefaults.AuthenticationScheme))
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

            document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] =
                new OpenApiSecurityScheme
                {
                    Description = "JWT Bearer auth (Example: `Bearer 12345abcdef`)",
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
                    In = ParameterLocation.Header,
                    BearerFormat = "JWT"
                };

            document.SecurityRequirements = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };
        }
    }
}
