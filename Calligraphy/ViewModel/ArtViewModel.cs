using System.ComponentModel.DataAnnotations;

namespace Calligraphy.ViewModel
{
    public class ArtViewModel
    {
        //string title, string content, string year, string style, string material, string size, bool isVisible
        public IFormFile File { get; set; } = null!;

        [Required(ErrorMessage = "請輸入作品名稱")]
        [Display(Name = "作品名稱")]
        public string Title { get; set; } = null!;

        [Display(Name = "作品簡介")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "請輸入創作日期")]
        [Display(Name = "創作年份")]
        public string Year { get; set; } = null!;

        [Display(Name = "作品風格")]
        public string? Style { get; set; }

        [Display(Name = "作品材質")]
        public string? Material { get; set; }

        [Display(Name = "作品尺寸")]
        public string? Size { get; set; }

        [Display(Name = "展示")]
        public bool IsVisible { get; set; } = false;
    }
}
