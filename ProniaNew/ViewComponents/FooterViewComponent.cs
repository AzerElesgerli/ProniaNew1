using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaNew.DAL;

namespace ProniaNew.ViewComponents
{
    public class FooterViewComponent
    {
        private readonly AppDbContext _context;

        public FooterViewComponent(AppDbContext context)
        {
            _context = context;
        }

        // public async Task<IViewComponentResult> Index()
        // {
        //     Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(d => d.Key, d => d.Value);
        //   return View(settings);
        // }
    }
}
