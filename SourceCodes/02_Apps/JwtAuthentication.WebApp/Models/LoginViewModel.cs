using System.ComponentModel.DataAnnotations;

namespace JwtAuthentication.WebApp.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class MyProfileViewModel
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}