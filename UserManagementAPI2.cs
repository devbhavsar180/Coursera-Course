// UserManagementAPI.cs

using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<User> users = new List<User>();

app.MapGet("/users", () => Results.Ok(users));

app.MapGet("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        return Results.NotFound($"User with ID {id} not found.");
    }
    return Results.Ok(user);
});

app.MapPost("/users", ([FromBody] User newUser) =>
{
    if (!IsValidUser(newUser, out var validationErrors))
    {
        return Results.BadRequest(validationErrors);
    }

    newUser.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
    users.Add(newUser);
    return Results.Created($"/users/{newUser.Id}", newUser);
});

app.MapPut("/users/{id}", (int id, [FromBody] User updatedUser) =>
{
    var existingUser = users.FirstOrDefault(u => u.Id == id);
    if (existingUser == null)
    {
        return Results.NotFound($"User with ID {id} not found.");
    }

    if (!IsValidUser(updatedUser, out var validationErrors))
    {
        return Results.BadRequest(validationErrors);
    }

    existingUser.Name = updatedUser.Name;
    existingUser.Email = updatedUser.Email;
    return Results.Ok(existingUser);
});

app.MapDelete("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null)
    {
        return Results.NotFound($"User with ID {id} not found.");
    }

    users.Remove(user);
    return Results.Ok($"User with ID {id} deleted.");
});

app.Run();

// User model with validation
public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

// Validation method
static bool IsValidUser(User user, out List<string> errors)
{
    errors = new List<string>();

    if (string.IsNullOrWhiteSpace(user.Name))
        errors.Add("Name is required.");
    if (string.IsNullOrWhiteSpace(user.Email) || !new EmailAddressAttribute().IsValid(user.Email))
        errors.Add("A valid email is required.");

    return errors.Count == 0;
}

/*
Debugging and Improvements with Copilot:

- Added validation attributes to User model (e.g., Required, EmailAddress).
- Created a custom IsValidUser method to perform validation manually when needed.
- Implemented try-catch like behavior with checks to avoid unhandled exceptions.
- Improved GET by ID and DELETE by ID to handle cases where the user does not exist.
- Improved POST and PUT to ensure valid data before modifying the list.
- Added meaningful error messages for better API responses.
*/