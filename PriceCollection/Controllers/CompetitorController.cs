using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCollection.Configurations;
using PriceCollection.Models;
using PriceCollection.ViewModels;
using System.Text;
using System.Web;
using System;
using System.Reflection;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using PriceCollection.Extensions;
using Hangfire;
using Microsoft.Data.SqlClient;

namespace PriceCollection.Controllers
{
    public class CompetitorController : Controller
    {
        private readonly PriceCollectionContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IPriceService _priceService;

        public CompetitorController(PriceCollectionContext context, IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IPriceService backgroundJobService)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
            _priceService = backgroundJobService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCompetitors(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return NotFound();
            }

            var competitors = await _context.Competitors
                .Where(t => t.ProductId == Guid.Parse(productId))
                .Select(t => new CompetitorViewModel
                {
                    CompetitorId = t.Id,
                    Name = t.Name,
                    Link = t.Link,
                    Price = t.Price.ConvertPriceToString(),
                    PriceMin = t.PriceMin.ConvertPriceToString(),
                    PriceMax = t.PriceMax.ConvertPriceToString(),
                    PriceManual = t.PriceManual.ConvertPriceToString(),
                    IsOwner = t.IsOwner,
                    HistoricalSold = t.HistoricalSold,
                    Image = t.Image,
                    NumberOfComment = t.NumberOfComment,
                    Note = t.Note
                })
                .OrderBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();

            return PartialView(competitors);
        }

        [HttpPost]
        public async Task<IActionResult> AddCompetitor([FromBody] AddCompetitorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var competitor = new Competitor
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name.Trim(),
                    Link = model.Link.Trim(),
                    ProductId = Guid.Parse(model.ProductId),
                    IsOwner = model.IsOwner,
                    Note = model.Note.Trim(),
                    CreatedDate = DateTime.UtcNow
                };
                _context.Add(competitor);
                await _context.SaveChangesAsync();

                if (model.IsOwner)
                {
                    var shopInfo = Utilities.GetShopIdAndItemIdFromUrl(model.Link);
                    var product = await _context.Products.FirstOrDefaultAsync(t => t.Id == Guid.Parse(model.ProductId));
                    product.SKU = shopInfo.ItemId;
                    product.ModifiedDate = DateTime.UtcNow;

                    _context.Entry(product).State = EntityState.Modified;
                    _context.SaveChangesAsync();
                }

                return Json(competitor);
            }
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCompetitor([FromBody] EditCompetitorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var competitor = await _context.Competitors.FirstOrDefaultAsync(t => t.Id == Guid.Parse(model.Id));
                if (!string.IsNullOrEmpty(model.NewPrice))
                    competitor.PriceManual = model.NewPrice.ConvertPriceToDouble();
                competitor.Name = model.Name.Trim();
                competitor.Link = model.Link.Trim();
                competitor.Note = model.Note.Trim();
                competitor.ModifiedDate = DateTime.UtcNow;

                _context.Entry(competitor).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Json(competitor);
            }
            return Json(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCompetitor(string competitorId)
        {
            if (string.IsNullOrEmpty(competitorId))
            {
                return NotFound();
            }

            var competitor = await _context.Competitors.FirstOrDefaultAsync(t => t.Id == Guid.Parse(competitorId));
            _context.Entry(competitor).State = EntityState.Deleted;
            _context.SaveChanges();

            return Json(null);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrice(string competitorId)
        {
            if (string.IsNullOrEmpty(competitorId))
            {
                return Json(null);
            }

            var competitor = await _context.Competitors.FirstOrDefaultAsync(t => t.Id == Guid.Parse(competitorId));
            var productInfo = await _priceService.GetProductInfo(competitor.Link);

            if (productInfo.error != null || productInfo.error_msg != null) return Json(null);

            if (competitor.PriceMin != productInfo.data.price_min || competitor.PriceMax != productInfo.data.price_max || competitor.Price != productInfo.data.price)
            {
                competitor.Price = productInfo.data.price;
                competitor.PriceMin = productInfo.data.price_min;
                competitor.PriceMax = productInfo.data.price_max;
                competitor.PriceManual = null;
                competitor.HistoricalSold = productInfo.data.historical_sold.ToString();
                competitor.NumberOfComment = productInfo.data.cmt_count.ToString();
                competitor.Image = productInfo.data.image;
                competitor.ModifiedDate = DateTime.UtcNow;

                _context.Entry(competitor).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return Json(competitor);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPrice()
        {
            await _priceService.GetCompetitorPriceAllProducts();

            return Json(null);
        }

        [HttpGet("/job")]
        public ActionResult CreateReccuringJob()
        {
            _recurringJobManager.AddOrUpdate("GetPriceDaily", () => _priceService.GetCompetitorPriceAllProducts(), Cron.Daily);
            return Ok();
        }
    }
}
