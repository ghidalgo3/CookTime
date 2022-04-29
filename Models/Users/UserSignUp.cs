using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace babe_algorithms.Models.Users;

public class UserSignUp
{
    [Required]
    [Display(Name = "UserName")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [PasswordComplexity(
        requiresNonAlphanumeric: false,
        requiresLower: false,
        requiresUpper: false,
        allowSpaces: true)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [BindNever]
    [JsonIgnore]
    public bool BypassEmailVerification { get; set; }
}