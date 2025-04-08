using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddLogging();

var app = builder.Build();

// Error Handling Middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred.");
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\": \"Internal server error.\"}");
    }
});

// Authentication Middleware
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    if (string.IsNullOrEmpty(token))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("{\"error\": \"Unauthorized. Token is missing.\"}");
        return;
    }

    // Dummy token validation logic for demonstration
    try
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        if (jwtToken == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("{\"error\": \"Unauthorized. Invalid token.\"}");
            return;
        }

        // You can add more validation logic here (issuer, expiry, etc.)

        await next();
    }
    catch (Exception)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("{\"error\": \"Unauthorized. Invalid token format.\"}");
    }
});

// Logging Middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Incoming Request: {Method} {Path}", context.Request.Method, context.Request.Path);

    await next();

    logger.LogInformation("Outgoing Response: {StatusCode}", context.Response.StatusCode);
});

app.MapControllers();

app.Run();
