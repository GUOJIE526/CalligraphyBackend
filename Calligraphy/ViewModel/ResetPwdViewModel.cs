using System.ComponentModel.DataAnnotations;

namespace Calligraphy.ViewModel
{
    public class ResetPwdViewModel
    {
        public string Token { get; set; } = null!;

        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "請輸入新密碼")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "請輸入新密碼")]
        public string ConfirmPassword { get; set; } = null!;

        public bool ResetConfirm { get; set; } = false;
    }
}
