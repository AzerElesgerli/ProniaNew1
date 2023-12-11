using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaNew.DAL;
using ProniaNew.Interfaces;
using ProniaNew.Models;
using ProniaNew.ViewComponents;
using ProniaNew.ViewModels;
using System.Security.Claims;

namespace ProniaNew.Controllers
{
	public class BasketController : Controller
	{
		private readonly AppDbContext _context;
		private readonly UserManager<AppUser> _userManager;
		private readonly IEmailService _emailService;
		public BasketController(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService, HeaderViewComponent headerVC)
		{
			_context = context;
			_userManager = userManager;
			_emailService = emailService;
		}

		public async Task<IActionResult> Index()
		{
			List<BasketVM> basketVM = new();
			if (User.Identity.IsAuthenticated)
			{
				AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(bi => bi.Product).ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
				foreach (BasketItem item in user.BasketItems)
				{
					basketVM.Add(new BasketVM()
					{
						Name = item.Product.Name,
						Price = item.Product.Price,
						Count = item.Count,
						SubTotal = item.Count * item.Product.Price,
						Image = item.Product.ProductImages.FirstOrDefault().Url,
						Id = item.Product.Id,
					});
				}
			}
			else
			{
				if (Request.Cookies["Basket"] is not null)
				{
					List<BasketsItemsVM> basket = JsonConvert.DeserializeObject<List<BasketsItemsVM>>(Request.Cookies["Basket"]);

					foreach (BasketsItemsVM basketCookieItem in basket)
					{
						Product product = await _context.Products.Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(p => p.Id == basketCookieItem.Id);

						if (product is not null)
						{
							BasketVM basketItemVM = new()
							{
								Id = product.Id,
								Name = product.Name,
								Image = product.ProductImages.FirstOrDefault().Url,
								Price = product.Price,
								Count = basketCookieItem.Count,
								SubTotal = product.Price * basketCookieItem.Count,
							};
							basketVM.Add(basketItemVM);

						}
					}
				}
			}

			return View(basketVM);
		}

		public async Task<IActionResult> AddBasket(int id)
		{
			if (id <= 0) return BadRequest();
			Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
			if (product is null) return NotFound();

			if (User.Identity.IsAuthenticated)
			{
				AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
				if (user is null) return NotFound();
				var basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
				if (basketItem is null)
				{
					basketItem = new()
					{
						AppUserId = user.Id,
						ProductId = product.Id,
						Price = product.Price,
						Count = 1,
						OrderId = null
					};
					user.BasketItems.Add(basketItem);
				}
				else
				{
					basketItem.Count++;
				}
				await _context.SaveChangesAsync();

				return Redirect(Request.Headers["Referer"]);
			}
			else
			{
				List<BasketsItemsVM> basket;
				if (Request.Cookies["Basket"] is not null)
				{
					basket = JsonConvert.DeserializeObject<List<BasketsItemsVM>>(Request.Cookies["Basket"]);
					var item = basket.FirstOrDefault(b => b.Id == id);
					if (item is null)
					{
						BasketsItemsVM itemVm = new BasketsItemsVM
						{
							Id = id,
							Count = 1
						};
						basket.Add(itemVm);
					}
					else
					{
						item.Count++;
					}
				}
				else
				{
					basket = new();
					BasketsItemsVM itemVm = new BasketsItemsVM
					{
						Id = id,
						Count = 1
					};
					basket.Add(itemVm);
				}

				string json = JsonConvert.SerializeObject(basket);
				Response.Cookies.Append("Basket", json);

				List<BasketVM> itemList = new();
				foreach (BasketsItemsVM basketCookieItem in basket)
				{
					Product producte = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == basketCookieItem.Id);

					if (producte is not null)
					{
						string image = producte.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Url;
						BasketVM basketItem = new()
						{
							Id = producte.Id,
							Name = producte.Name,
							Image = image,
							Price = producte.Price,
							Count = basketCookieItem.Count,
							SubTotal = producte.Price * basketCookieItem.Count,
						};
						itemList.Add(basketItem);
					}
				}

				return PartialView("_BasketItemsPartial", itemList);

			}
		}

		public async Task<IActionResult> RemoveBasket(int id)
		{
			if (id <= 0) return BadRequest();
			Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
			if (product is null) return NotFound();
			List<BasketsItemsVM> basket;
			if (User.Identity.IsAuthenticated)
			{
				AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
				if (user is null) return NotFound();
				var basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
				if (basketItem is null) return NotFound();
				else
				{
					user.BasketItems.Remove(basketItem);
				}
				await _context.SaveChangesAsync();


			}
			else
			{
				if (Request.Cookies["Basket"] is not null)
				{
					basket = JsonConvert.DeserializeObject<List<BasketsItemsVM>>(Request.Cookies["Basket"]);
					var item = basket.FirstOrDefault(b => b.Id == id);
					if (item is not null)
					{
						basket.Remove(item);

						string json = JsonConvert.SerializeObject(basket);
						Response.Cookies.Append("Basket", json);


						List<BasketsItemsVM> itemList = new();
						foreach (BasketsItemsVM BasketsitemsVM in basket)
						{
							Product producte = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == BasketsitemsVM.Id);

							if (producte is not null)
							{
								string image = product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Url;
								BasketsItemsVM basketItem = new()
								{

								};
								itemList.Add(basketItem);
							}
						}

						return PartialView("_BasketItemsPartial", itemList);
					}

				}

			}
			return Redirect(Request.Headers["Referer"]);
		}

		public async Task<IActionResult> Decrement(int id)
		{
			if (id <= 0) return BadRequest();
			Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
			if (product is null) return NotFound();
			List<BasketsItemsVM> basket;
			if (User.Identity.IsAuthenticated)
			{
				AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
				if (user is null) return NotFound();
				var basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
				if (basketItem is not null)
				{
					basketItem.Count--;
					if (basketItem.Count == 0)
					{
						user.BasketItems.Remove(basketItem);
					}
					await _context.SaveChangesAsync();
				}
			}
			else
			{
				if (Request.Cookies["Basket"] is not null)
				{
					basket = JsonConvert.DeserializeObject<List<BasketsItemsVM>>(Request.Cookies["Basket"]);
					var item = basket.FirstOrDefault(b => b.Id == id);
					if (item is not null)
					{
						item.Count--;
						if (item.Count == 0)
						{
							basket.Remove(item);
						}
						string json = JsonConvert.SerializeObject(basket);
						Response.Cookies.Append("Basket", json);
					}
				}
			}

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Increment(int id)
		{
			if (id <= 0) return BadRequest();
			Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
			if (product is null) return NotFound();

			if (User.Identity.IsAuthenticated)
			{
				AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).FirstOrDefaultAsync(u => u.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value);
				if (user is null) return NotFound();
				var basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);
				if (basketItem is null)
				{
					basketItem = new()
					{
						AppUserId = user.Id,
						ProductId = product.Id,
						Price = product.Price,
						Count = 1,
						OrderId = null
					};
					user.BasketItems.Add(basketItem);
				}
				else
				{
					basketItem.Count++;
				}
				await _context.SaveChangesAsync();


			}
			else
			{
				if (Request.Cookies["Basket"] is not null)
				{
					var basket = JsonConvert.DeserializeObject<List<BasketsItemsVM>>(Request.Cookies["Basket"]);
					var item = basket.FirstOrDefault(b => b.Id == id);
					if (item is not null)
					{
						item.Count++;
						string json = JsonConvert.SerializeObject(basket);
						Response.Cookies.Append("Basket", json);

						Product producte = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == item.Id);
						string image = producte.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Url;
						BasketVM basketItem = new()
						{
							Id = producte.Id,
							Name = producte.Name,
							Image = image,
							Price = producte.Price,
							Count = item.Count,
							SubTotal = producte.Price * item.Count,
						};
						return PartialView("_BasketPartial", basketItem);
					}

				}
			}
			return Redirect(Request.Headers["Referer"]);
		}

		public async Task<IActionResult> Checkout()
		{
			AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(pi => pi.Product).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
			OrderVM orderVM = new OrderVM
			{
				BasketItems = user.BasketItems
			};
			return View(orderVM);
		}
		[HttpPost]
		public async Task<IActionResult> Checkout(OrderVM orderVM)
		{
			AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).ThenInclude(pi => pi.Product).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

			if (!ModelState.IsValid)
			{
				orderVM.BasketItems = user.BasketItems;
				return View(orderVM);
			}
			decimal total = 0;
			foreach (BasketItem item in user.BasketItems)
			{
				item.Price = item.Product.Price;
				total += item.Count * item.Price;
			}
			Order order = new Order
			{
				Status = null,
				Address = orderVM.Address,
				PurchaseAt = DateTime.UtcNow,
				AppUserId = user.Id,
				BasketItems = user.BasketItems,
				TotalPrice = total
			};
			await _context.Orders.AddAsync(order);
			await _context.SaveChangesAsync();
			string body = $@"<table class=""table"">
  <thead>
    <tr>
      <th scope=""col"">#</th>
      <th scope=""col"">Name</th>
      <th scope=""col"">Price</th>
      <th scope=""col"">Count</th>
    </tr>
  </thead>
  <tbody>";
			foreach (BasketItem item in order.BasketItems)
			{
				body += $@"<tr>
      <th scope=""row"">${item.Id}</th>
      <td>${item.Product.Name}</td>
      <td>${item.Price}</td>
      <td>{item.Count}</td>
    </tr>
";
			};
			body += @"</tbody>
</table>";
			await _emailService.SendEmailAsync(user.Email, "Your Order", body, true);
			return RedirectToAction("Index", "Home");
		}
	}
}