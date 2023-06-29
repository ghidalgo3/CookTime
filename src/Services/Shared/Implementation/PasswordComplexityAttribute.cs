using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace babe_algorithms;

public partial class PasswordComplexityAttribute : ValidationAttribute, IClientModelValidator
{
    public PasswordComplexityAttribute(
        bool requiresNonAlphanumeric,
        bool requiresLower,
        bool requiresUpper,
        bool allowSpaces = false)
    {
        RequiresUpper = requiresUpper;
        RequiresLower = requiresLower;
        RequiresNonAlphanumeric = requiresNonAlphanumeric;
        AllowSpaces = allowSpaces;
    }

    public bool AllowSpaces { get; }

    public bool RequiresNonAlphanumeric { get; }

    public bool RequiresLower { get; }

    public bool RequiresUpper { get; }

    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-password", GetErrorMessage(context));
    }

    protected override ValidationResult IsValid(
        object value,
        ValidationContext validationContext)
    {
        string errorMessage = string.Empty;
        if (value is not string password)
        {
            return new ValidationResult("Password is empty");
        }

        if (RequiresNonAlphanumeric && !ContainsNonAlphanumeric(password))
        {
            errorMessage += "Must use non-alphanumeric character. ";
        }

        if (RequiresLower && !ContainsLowerCase(password))
        {
            errorMessage += "Must use lower case character. ";
        }

        if (RequiresUpper && !ContainsUpperCase(password))
        {
            errorMessage += "Must use upper case character. ";
        }

        if (!AllowSpaces && ContainsSpaces(password))
        {
            errorMessage += "Cannot contains spaces. ";
        }

        if (string.IsNullOrEmpty(errorMessage))
        {
            return ValidationResult.Success;
        }
        else
        {
            return new ValidationResult(errorMessage);
        }
    }

    private bool ContainsUpperCase(string str)
    {
        var regex = UpperCase();
        var match = regex.Match(str);
        return match.Success;
    }

    private bool ContainsLowerCase(string str)
    {
        var regex = LowerCase();
        var match = regex.Match(str);
        return match.Success;
    }

    private bool ContainsNonAlphanumeric(string str)
    {
        var regex = NonAlpha();
        var match = regex.Match(str);
        return match.Success;
    }

    private bool ContainsSpaces(string str)
    {
        var regex = Spaces();
        var match = regex.Match(str);
        return match.Success;
    }

    private string GetErrorMessage(ClientModelValidationContext context)
    {
        return "Not complex enough";
    }

    private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (attributes.ContainsKey(key))
        {
            return false;
        }

        attributes.Add(key, value);
        return true;
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex UpperCase();
    [GeneratedRegex("[a-z]")]
    private static partial Regex LowerCase();
    [GeneratedRegex("\\W")]
    private static partial Regex NonAlpha();
    [GeneratedRegex("\\s")]
    private static partial Regex Spaces();
}