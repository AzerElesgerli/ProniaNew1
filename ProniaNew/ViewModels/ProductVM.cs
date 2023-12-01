using ProniaNew.Models;

namespace ProniaNew.ViewModels
{
	public class ProductVM
	{
		public Product Product { get; set; }
		public List<Product> RelatedProducts { get; set; }
	}
}
