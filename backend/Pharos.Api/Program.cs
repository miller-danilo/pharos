using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Pharos.Core.Services;
using Pharos.Core.Interfaces;
using Pharos.Api.Authentication;
using System;
using System.Threading.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Firestore and Gemini Services
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<IUserRepository>(sp => sp.GetRequiredService<FirestoreService>());
builder.Services.AddSingleton<ICreditRepository>(sp => sp.GetRequiredService<FirestoreService>());
builder.Services.AddSingleton<IConfigRepository>(sp => sp.GetRequiredService<FirestoreService>());
builder.Services.AddSingleton<IScanRepository>(sp => sp.GetRequiredService<FirestoreService>());
if (builder.Configuration.GetValue<bool>("Gemini:UseMock"))
{
    builder.Services.AddSingleton<IAiService, MockAiService>();
}
else
{
    builder.Services.AddHttpClient<IAiService, GeminiService>();
}
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IShieldService, ShieldService>();
builder.Services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true) // Replace with frontend URL in prod if needed
              .AllowCredentials();
    });
});

// Configure Rate Limiting by IP (5 requests per min)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PublicScanPolicy", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

// Configure JWT Bearer Authentication for Firebase
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string firebaseProjectId = builder.Configuration["Firebase:ProjectId"] 
                                   ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID") 
                                   ?? "pharos-ai-demo";

        string? authEmulatorHost = Environment.GetEnvironmentVariable("FIREBASE_AUTH_EMULATOR_HOST");
        if (string.IsNullOrEmpty(authEmulatorHost) && builder.Environment.IsDevelopment())
        {
            authEmulatorHost = "localhost:9099";
        }

        if (!string.IsNullOrEmpty(authEmulatorHost))
        {
            Console.WriteLine($"[Dev Mode] Firebase Auth Emulator configured at: http://{authEmulatorHost}");
            options.RequireHttpsMetadata = false;
            options.UseSecurityTokenValidators = true;
            options.SecurityTokenValidators.Clear();
            options.SecurityTokenValidators.Add(new DevTokenValidator());
        }
        else
        {
            options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://securetoken.google.com/{firebaseProjectId}",
                ValidateAudience = true,
                ValidAudience = firebaseProjectId,
                ValidateLifetime = true
            };
        }
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
