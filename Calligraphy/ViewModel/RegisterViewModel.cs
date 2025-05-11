using System.ComponentModel.DataAnnotations;

namespace Calligraphy.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入Email!!")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = "請輸入密碼!!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "請再次輸入密碼!!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密碼不一致!!")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
