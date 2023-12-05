using System.ComponentModel.DataAnnotations;

namespace ProniaNew.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "There is no human without name")]
        [MinLength(3)]
        [MaxLength(25)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname daxil edin ")]
        [MinLength(3)]
        [MaxLength(25)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Email  yazin ")]
        [EmailAddress]
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Username { get; set; }

        [Required(ErrorMessage = "Write Your  password ")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Write Your  password again ")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
