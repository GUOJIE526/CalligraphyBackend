using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Calligraphy.ViewModel
{
    /// <summary>
    /// 查看最新訪客留言
    /// </summary>
    public class DashboardViewModel
    {
        [Display(Name = "作品")]
        public string ArtTitle { get; set; } = null!;

        [Display(Name = "訪客留言")]
        public List<string> Comment { get; set; } = null!;

        [Display(Name = "回覆留言")]
        public List<string> Reply { get; set; } = new();
        // Add other properties as needed
    }

    /// <summary>
    /// 回覆留言
    /// </summary>
    public class DashboardItemViewModel()
    {
        public string? Reply { get; set; }
    }
}
