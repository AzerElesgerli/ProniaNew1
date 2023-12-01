using System.ComponentModel.DataAnnotations;

namespace ProniaNew.Models
{
	public class Category
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<Category>? Products { get; set; }
	}
}
