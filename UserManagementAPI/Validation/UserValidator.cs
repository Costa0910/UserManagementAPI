using System;
using System.Collections.Generic;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using UserManagementAPI.Models;

namespace UserManagementAPI.Validation;

public static class UserValidator
{
    public static IResult? Validate(CreateUserRequest request)
    {
        return Validate(request.Name, request.Email);
    }

    public static IResult? Validate(UpdateUserRequest request)
    {
        return Validate(request.Name, request.Email);
    }

    private static IResult? Validate(string? name, string? email)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var normalizedName = name?.Trim() ?? string.Empty;
        var normalizedEmail = email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            errors["Name"] = ["Name is required."];
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            errors["Email"] = ["Email is required."];
        }
        else if (!IsValidEmail(normalizedEmail))
        {
            errors["Email"] = ["Email must be a valid email address."];
        }

        return errors.Count > 0 ? Results.ValidationProblem(errors) : null;
    }

    public static string Normalize(string value) => value.Trim();

    public static (string name, string email) NormalizeInputs(string name, string email)
    {
        return (name.Trim(), email.Trim());
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

