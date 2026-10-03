using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Models.ViewModel
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
