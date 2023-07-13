using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCollection.Configurations;
using PriceCollection.Models;
using System.Diagnostics;

namespace PriceCollection.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PriceCollectionContext _context;

        public HomeController(PriceCollectionContext context, ILogger<HomeController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Test()
        {
            try
            {
                var products = await _context.Products.AsNoTracking().ToListAsync();
                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message + ex.InnerException);
                return Json(ex.Message + ex.InnerException);
                throw;
            }
        }
    }
}