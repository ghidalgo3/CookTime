public class UserSignUp
{
    [Required]
    [Display(Name = "Name")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
    public string Name { get; set; }

    public string Email { get; set; }

    [Required]
    public Role Role { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [PasswordComplexity(
        requiresNonAlphanumeric: true,
        requiresLower: true,
        requiresUpper: true)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    public bool BypassEmailVerification { get; set; }
}