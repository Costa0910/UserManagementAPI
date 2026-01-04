using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserManagementAPI.Models;
using UserManagementAPI.Repositories;
using UserManagementAPI.Validation;
using UserManagementAPI.Middleware;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var initialTokens = config.GetSection("Auth:Tokens").Get<string[]>() ?? Array.Empty<string>();
    return new TokenStore(initialTokens);
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<TokenAuthenticationMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var repo = app.Services.GetRequiredService<UserRepository>();

app.MapPost("/auth/token", (AuthRequest request, IConfiguration config, TokenStore tokenStore) =>
    {
        // Simple demo: validate against configured users/secrets
        var configuredUsers = config.GetSection("Auth:Users").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>(StringComparer.Ordinal);
        if (!configuredUsers.TryGetValue(request.Username, out var expectedSecret) || !string.Equals(expectedSecret, request.Secret, StringComparison.Ordinal))
        {
            return Results.Unauthorized();
        }

        var token = tokenStore.FirstOrDefault() ?? tokenStore.Add(Guid.NewGuid().ToString("N"));
        return Results.Ok(new AuthResponse(token));
    })
    .WithName("GetToken");

app.MapGet("/users", () => Results.Ok(repo.GetAll()))
    .WithName("GetUsers");

app.MapGet("/users/{id:int}", (int id) =>
    {
        var user = repo.GetById(id);
        return user is null ? Results.NotFound() : Results.Ok(user);
    })
    .WithName("GetUserById");

app.MapPost("/users", (CreateUserRequest request) =>
    {
        var validation = UserValidator.Validate(request);
        if (validation is not null)
        {
            return validation;
        }

        var (name, email) = UserValidator.NormalizeInputs(request.Name, request.Email);

        if (repo.EmailExists(email))
        {
            return Results.Conflict(new { message = "Email already exists." });
        }

        var user = repo.Add(name, email);
        return Results.Created($"/users/{user.Id}", user);
    })
    .WithName("CreateUser");

app.MapPut("/users/{id:int}", (int id, UpdateUserRequest request) =>
    {
        var validation = UserValidator.Validate(request);
        if (validation is not null)
        {
            return validation;
        }

        var (name, email) = UserValidator.NormalizeInputs(request.Name, request.Email);

        if (repo.EmailExists(email, id))
        {
            return Results.Conflict(new { message = "Email already exists." });
        }

        var updated = repo.Update(id, name, email);
        return updated is null ? Results.NotFound() : Results.Ok(updated);
    })
    .WithName("UpdateUser");

app.MapDelete("/users/{id:int}", (int id) =>
    {
        var removed = repo.Delete(id);
        return removed ? Results.NoContent() : Results.NotFound();
    })
    .WithName("DeleteUser");

app.Run();
