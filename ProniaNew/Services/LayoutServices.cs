using Microsoft.AspNet.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaNew.DAL;
using ProniaNew.Models;
using ProniaNew.ViewModels;
using System.Security.Claims;

namespace ProniaNew.Services
{
    public class LayoutServices
    {
		private readonly AppDbContext _context;
		private readonly UserManager<AppUser> _usermanager;
		private readonly IHttpContextAccessor _http;
		
		public LayoutServices(AppDbContext context, IHttpContextAccessor http, UserManager<AppUser> userManager)
		{
			_context = context;
			_http = http;
			_usermanager = userManager;
		}
		public async Task<Dictionary<string, string>> GetSettingsAsync()
		{
			Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);

			return settings;
		}
		public async Task<List<BasketVM>> GetBasketsAsync()
		{
			List<BasketVM> items = new List<BasketVM>();

			if (_http.HttpContext.User.Identity.IsAuthenticated)
			{
			

				foreach (BasketItem basketItem in AppUser.BasketItems)
				{
					items.Add(new BasketVM
					{
						Id = basketItem.ProductId,
						Price = basketItem.Product.Price,
						Count = basketItem.Count,
						Name = basketItem.Product.Name,
						SubTotal = basketItem.Count * basketItem.Product.Price,
						Image = basketItem.Product.ProductImages.FirstOrDefault()?.Url
					});
				}
			}
			else
			{
				if (_http.HttpContext.Request.Cookies["Basket"] is not null)
				{
					List<BasketsItemsVM> cookies = JsonConvert.DeserializeObject<List<BasketsItemsVM>>(_http.HttpContext.Request.Cookies["Basket"]);

					foreach (var cookie in cookies)
					{
						Product product = await _context.Products
							.Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
							.FirstOrDefaultAsync(p => p.Id == cookie.Id);
						if (product != null)
						{
							BasketVM item = new BasketVM
							{
								Id = product.Id,
								Name = product.Name,
								Image = product.ProductImages.FirstOrDefault().Url,
								Price = product.Price,
								Count = cookie.Count,
								SubTotal = product.Price * cookie.Count,
							};
							items.Add(item);
						}
					}
				}
			}

			return items;
		}
	}
}
