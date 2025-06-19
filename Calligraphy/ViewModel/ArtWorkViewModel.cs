using System.ComponentModel.DataAnnotations;

namespace Calligraphy.ViewModel
{
    public class ArtWorkViewModel
    {
        public Guid ArtWorkId { get; set; }

        [Required(ErrorMessage = "請輸入作品名稱")]
        [Display( Name = "作品")]
        public string Title { get; set; } = null!;

        [Display( Name = "瀏覽人次")]
        public int Views { get; set; }

        [Display( Name = "作品簡介")]
        public string? Description { get; set; }

        [Display( Name = "創作日期")]
        public DateTime CreatedYear { get; set; }

        [Display( Name = "創作風格")]
        public string? Style { get; set; }

        [Display( Name = "創作材質")]
        public string? Material { get; set; }

        [Display( Name = "作品尺寸")]
        public string? Size { get; set; }

        [Display( Name = "是否顯示前台")]
        public bool IsVisible { get; set; }
        public DateTimeOffset ModifyDate { get; set; }
        public string? ModifyFrom { get; set; }
        public string? Modifier { get; set; }
    }
}
