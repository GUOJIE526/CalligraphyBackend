using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Calligraphy.ViewModel
{
    /// <summary>
    /// 查看最新訪客留言
    /// </summary>
    public class DashboardViewModel
    {
        public Guid dashId { get; set; }
        [Display(Name = "作品")]
        public string artTitle { get; set; } = null!;

        [Display(Name = "訪客名稱")]
        public string userName { get; set; } = null!;

        [Display(Name = "訪客留言")]
        public string comment { get; set; } = null!;

        [Display(Name = "留言時間")]
        public DateTimeOffset commentCreate { get; set; }

        [Display(Name = "作者回覆")]
        public string reply { get; set; }
        // Add other properties as needed
    }
}
