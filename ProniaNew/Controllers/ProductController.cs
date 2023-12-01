using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaNew.DAL;
using ProniaNew.Models;
using ProniaNew.ViewModels;

namespace ProniaNew.Controllers
{
	public class ProductController : Controller
	{
		private readonly AppDbContext _context;

		public ProductController(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Detail(int id)
		{
			if (id <= 0) return BadRequest();

			Product product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

			_context.Products.Where(p => p.CategoryId == product.CategoryId);

			if (product is null) return NotFound();

			return View(product);
		}
		DetailVM detailVm =new DetailVM
		{ 
             Product = product,
                RelatedProducts = await _context.Products
			.Where(p=>p.CategoryId==product.CategoryId$$p.Id!=Product.id)
			.Take(12)
			.toListAsync():
    };
         
    }




}
