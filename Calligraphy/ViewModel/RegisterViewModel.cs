using System.ComponentModel.DataAnnotations;

namespace Calligraphy.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入Email!!")]
        [EmailAddress(ErrorMessage = "請輸入正確的Email格式!!")]
        [Display(Name = "信箱")]
        public string Username { get; set; } = null!;
        [Required(ErrorMessage = "請輸入密碼!!")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "請再次輸入密碼!!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密碼不一致!!")]
        [Display(Name = "確認密碼")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
