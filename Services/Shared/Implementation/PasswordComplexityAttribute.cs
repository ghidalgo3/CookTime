using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class PasswordComplexityAttribute : ValidationAttribute, IClientModelValidator
{
    public PasswordComplexityAttribute(
        bool requiresNonAlphanumeric,
        bool requiresLower,
        bool requiresUpper,
        bool allowSpaces = false)
    {
        this.RequiresUpper = requiresUpper;
        this.RequiresLower = requiresLower;
        this.RequiresNonAlphanumeric = requiresNonAlphanumeric;
        this.AllowSpaces = allowSpaces;
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

        this.MergeAttribute(context.Attributes, "data-val", "true");
        this.MergeAttribute(context.Attributes, "data-val-password", this.GetErrorMessage(context));
    }

    protected override ValidationResult IsValid(
        object value,
        ValidationContext validationContext)
    {
        string errorMessage = string.Empty;
        var password = value as string;
        if (password == null)
        {
            return new ValidationResult("Password is empty");
        }

        if (this.RequiresNonAlphanumeric && !this.ContainsNonAlphanumeric(password))
        {
            errorMessage += "Must use non-alphanumeric character. ";
        }

        if (this.RequiresLower && !this.ContainsLowerCase(password))
        {
            errorMessage += "Must use lower case character. ";
        }

        if (this.RequiresUpper && !this.ContainsUpperCase(password))
        {
            errorMessage += "Must use upper case character. ";
        }

        if (!this.AllowSpaces && this.ContainsSpaces(password))
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
        var regex = new Regex(@"[A-Z]");
        var match = regex.Match(str);
        return match.Success;
    }

    private bool ContainsLowerCase(string str)
    {
        var regex = new Regex(@"[a-z]");
        var match = regex.Match(str);
        return match.Success;
    }

    private bool ContainsNonAlphanumeric(string str)
    {
        var regex = new Regex(@"\W");
        var match = regex.Match(str);
        return match.Success;
    }

    private bool ContainsSpaces(string str)
    {
        var regex = new Regex(@"\s");
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
}