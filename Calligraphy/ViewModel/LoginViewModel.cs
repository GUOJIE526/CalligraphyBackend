using System.ComponentModel.DataAnnotations;

namespace Calligraphy.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "請輸入帳號!")]
        [Display(Name = "信箱")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "請輸入密碼!")]
        [Display(Name = "密碼")]
        public string Password { get; set; } = null!;

        [Display(Name = "記住我")]
        public bool RememberMe { get; set; }
    }
}