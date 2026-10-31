using AuthApi.Data;
using AuthApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Reflection.Metadata;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Auth Api Server",
            Version = "v1",
            Description = "Standalone API for Authentication"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["AccessCookie"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Cookie,
            Name = "access_token",
            Description = "JWT authentication using as HttpOnly cookie."
        };

        document.Components.SecuritySchemes["RefreshCookie"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Cookie,
            Name = "refresh_token",
            Description = "Refresh Token stores in an HttpOnly cookie."
        };

        //// Configuration by Header
        //document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        //{
        //    Type = SecuritySchemeType.Http,
        //    Scheme = "bearer",
        //    BearerFormat = "JWT",
        //    Description = "Enter your JWT token (without the 'Bearer prefix)."
        //};

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, ct) =>
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var path = context.Description.RelativePath?.Trim('/');

        if (string.Equals(path, "api/auth/refresh", StringComparison.OrdinalIgnoreCase))
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("RefreshCookie", context.Document)] = []
                // Configuration by header
                //[new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });
        }
        else
        {
            var requiresAuth = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();

            if (requiresAuth)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                var alreadyConfigured = operation.Security.Any(requirement => requirement.Keys.Any(scheme => scheme is OpenApiSecuritySchemeReference reference && reference.Reference.Id == "Bearer"));

                if (!alreadyConfigured)
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("AccessCookie", context.Document)] = []
                        // Configuration by header
                        //[new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                    });
            }
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddSingleton<JwtService>(); //Utilidades
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddScoped<UserService>(); //Instancia por cada solicitud que se haga

var jwtSettings = builder.Configuration.GetSection("Jwt");
var issuer = jwtSettings["Issuer"]!;
var audience = jwtSettings["Audience"]!;
var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("Secret not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    //options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        //As soon as the request reaches that Authorize attribute, the OnMessageReceived event will be triggered.
        //Then, we simply want to extract the token’s value from the cookie and set it as the Token for the validation process.
        OnMessageReceived = context =>
        {
            context.Request.Cookies.TryGetValue("access_token", out var accessToken);

            if (!string.IsNullOrEmpty(accessToken))
                context.Token = accessToken;

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT ERROR: {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddPolicy("AngularPolicy", policy => policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
        .WithTitle("")
        .WithTheme(ScalarTheme.BluePlanet)
        .AddPreferredSecuritySchemes("Bearer")
        .AddHttpAuthentication("Bearer", http => { });
    });
}

app.UseHttpsRedirection();
app.UseCors("AngularPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
