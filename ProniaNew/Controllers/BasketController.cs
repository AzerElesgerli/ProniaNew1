﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaNew.DAL;
using ProniaNew.Models;
using ProniaNew.ViewModels;

namespace ProniaNew.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;

        public BasketController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<BasketVM> basketVM = new();
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
            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id, string controllerName)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
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

            return RedirectToAction(nameof(Index), controllerName);
        }

        public async Task<IActionResult> RemoveBasket(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            List<BasketsItemsVM> basket;
            if (Request.Cookies["Basket"] is not null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketsItemsVM>>(Request.Cookies["Basket"]);
                var item = basket.FirstOrDefault(b => b.Id == id);
                if (item is not null)
                {
                    basket.Remove(item);

                    string json = JsonConvert.SerializeObject(basket);
                    Response.Cookies.Append("Basket", json);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Decrement(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return NotFound();
            List<BasketsItemsVM> basket;
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
            return RedirectToAction(nameof(Index));
        }
    }
}