using babe_algorithms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace babe_algorithms.Pages;

// open Protabla.Models
// open Protabla.DTO
// open Microsoft.AspNetCore.Mvc
// open Microsoft.AspNetCore.Mvc.RazorPages
// open System.ComponentModel.DataAnnotations
public class RegistrationModel : PageModel
{

    [BindProperty]
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 10)]
    [PasswordComplexity(false, false, false, false)]
    public string Password { get; set; }

    [BindProperty]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    public IUserService UserService { get; }
    public ApplicationDbContext AppDbContext { get; }

    public RegistrationModel(
        IUserService userService,
        ApplicationDbContext context)
    {
        this.UserService = userService;
        this.AppDbContext = context;
    }

    public async Task<IActionResult> OnGet()
    {
        var currentUser = this.UserService.GetUser(this.User);
        if (currentUser == null)
        {
            return this.Redirect("/Index");
        }
        if (currentUser.PasswordHash == null)
        {
            return this.Page();
        }
        else
        {
            return this.Redirect("/");
        }
    }
}
// type RegistrationModel
//   ( userManager : IUserManager,
//     appDbContext : ApplicationDbContext ) =
//   inherit PageModel()

//   // Include password validator


//   member this.OnGet() : IActionResult =
//     let caller = Asp.GetCaller userManager this.User
//     match caller with
//     | Asp.Student(appUser) when isNull appUser.PasswordHash  -> 
//       // this case is for students who have not set a password
//       this.Page() :> IActionResult
//     | Asp.Student(_) ->
//       // if you have a password, skip this process and go to your cycles
//       this.Redirect("/Dashboard/Cycles") :> IActionResult
//     | _ ->
//       this.Redirect("/Index") :> IActionResult

//   member this.OnPostName([<Required>] name : string) : IActionResult =
//     this.EditStudent
//       (fun (appUser : ApplicationUser) -> appUser.Student.Name <- name; appUser.Name <- name)
//       "/Registration#major"
  
//   member this.OnPostMajor() : IActionResult =
//     this.EditStudent
//       (fun (appUser : ApplicationUser) -> appUser.Student.Major <- this.Major)
//       "/Registration#year"

//   member this.OnPostYear() : IActionResult =
//     this.EditStudent
//       (fun (appUser : ApplicationUser) -> appUser.Student.Year <- this.Year)
//       "/Registration#password"

//   member this.OnPostPassword() : IActionResult =
//     this.EditStudent
//       (fun (appUser : ApplicationUser) -> userManager.SetPassword(appUser, this.Password).Result |> ignore)
//       "/Dashboard/Bids"

//   member this.EditStudent action nextPage =
//     let caller = Asp.GetCaller userManager this.User
//     match caller with
//     | Asp.Student(appUser) ->
//       action(appUser)
//       // TODO check for errors
//       // Confirm that passwords match and store it, I'm not sure how to hash it
//       appDbContext.SaveChanges() |> ignore
//       this.Redirect(nextPage) :> IActionResult
//       // this.Redirect("/Dashboard/Bids") :> IActionResult
//     | _ -> this.Redirect("/Index") :> IActionResult
