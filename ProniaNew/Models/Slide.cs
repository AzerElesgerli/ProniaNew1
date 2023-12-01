using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ProniaNew.Models
{
	public class Slide
	{
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is lazimdi")]
        [MaxLength(25, ErrorMessage = "Max uzunlug is 25")]
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Order { get; set; }
        [NotMapped]
        public IFormFile? Photo { get; set; }
    }
}
