using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaNew.Areas.ProniaAdmin.ViewModels;
using ProniaNew.DAL;
using ProniaNew.Models;
using System.Collections.Generic;

namespace ProniaNew.Areas.ProniaAdmin.Controllers
{
    [Area("ProniaAdmin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize("Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products
                .Include(p=>p.Category)
                .Include(p=>p.ProductImages.Where(pi=>pi.IsPrimary==true))
                .Include(p=>p.ProductTags)
                .ThenInclude(pt=>pt.Tag)
                .ToListAsync();
            return View(products);
        }
		[Authorize("Admin,Moderator")]
		public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Tags = await _context.Tags.ToListAsync();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                return View();
            }

            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ModelState.AddModelError("CategoryId", $"Bu id li category movcud deyil");
                return View();
            }

            foreach (int tagId in productVM.TagIds)
            {
                bool tagResult = await _context.Tags.AnyAsync(t => t.Id == tagId);
                if (!tagResult)
                {
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Tags = await _context.Tags.ToListAsync();
                    ModelState.AddModelError("TagIds", "Tag melumatlari sehv daxil edilib");
                    return View();
                }
            }

            Product product = new Product
            {
                SKU = productVM.SKU,
                Description = productVM.Description,
                Name = productVM.Name,
                Price = productVM.Price,
                CategoryId = (int)productVM.CategoryId,
                ProductTags=new List<ProductTag>()
            };

           
            foreach (int tagId in productVM.TagIds)
            {
                ProductTag productTag = new ProductTag
                {
                    TagId = tagId
                };
                product.ProductTags.Add(productTag);
               
            }
          
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));


        }
		[Authorize("Admin,Moderator")]
		public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            
            Product product = await _context.Products.Include(p=>p.ProductTags).FirstOrDefaultAsync(p=>p.Id==id);

            if (product is null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                TagIds=product.ProductTags.Select(pt=>pt.TagId).ToList(),
            };



            return View(productVM);      
        }
		[Authorize("Admin,Moderator")]
		[HttpPost]
        public async Task<IActionResult> Update(int id, UpdateProductVM productVM)
        {
            
            Product existed = await _context.Products.Include(p=>p.ProductTags).FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();

            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
              
                ModelState.AddModelError("CategoryId", "Bele bir category movcud deyil");
                return View(productVM);
            }

            foreach (ProductTag pTag in existed.ProductTags)
            {
                if (!productVM.TagIds.Exists(tId=>tId==pTag.TagId))
                {
                    _context.ProductTags.Remove(pTag);
                }
            }
            foreach (int tId in productVM.TagIds)
            {
                if (!existed.ProductTags.Any(pt=>pt.TagId==tId))
                {
                    
                    existed.ProductTags.Add(new ProductTag
                    {
                        TagId = tId
                    });
                }
            }

            existed.Name=productVM.Name;
            existed.Description = productVM.Description;
            existed.Price = productVM.Price;
            existed.SKU = productVM.SKU;
            existed.CategoryId = productVM.CategoryId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
