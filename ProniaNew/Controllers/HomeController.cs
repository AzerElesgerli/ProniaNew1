﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaNew.DAL;
using ProniaNew.Models;
using ProniaNew.ViewModels;
using System;

namespace ProniaNew.Controllers
{
	public class HomeController : Controller
	{
		private readonly AppDbContext _context;
		public HomeController(AppDbContext context)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{

			//_context.Slides.AddRange(slides);
			//_context.SaveChanges();

			List<Slide> slides = await _context.Slides.OrderBy(s => s.Order).Take(2).ToListAsync();
			List<Product> products = await _context.Products
				.Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
				.ToListAsync();

			HomeVM home = new HomeVM
			{
				Slides = slides,
				Products = products
			};


			return View(home);
		}

		public IActionResult ErrorPage(string error)
		{
			return View(model: error);
		}
		public IActionResult About()
		{
			return View();
		}
	}
}
