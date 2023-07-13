using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Table;
using OfficeOpenXml;
using PriceCollection.Configurations;
using PriceCollection.Extensions;
using PriceCollection.Models;
using PriceCollection.ViewModels;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection.Metadata;
using Microsoft.Extensions.Caching.Memory;
using PriceCollection.DTOs;

namespace PriceCollection.Controllers
{
    public class ProductController : Controller
    {
        private readonly PriceCollectionContext _context;
        private readonly IMemoryCache _memoryCache;

        public ProductController(PriceCollectionContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();
            var competitors = await _context.Competitors.Where(t => t.IsOwner == true).AsNoTracking().ToListAsync();

            var result = new List<ProductViewModel>();
            foreach (var product in products)
            {
                var competitor = competitors.FirstOrDefault(t => t.ProductId == product.Id);
                result.Add(new ProductViewModel
                {
                    ProductId = product.Id,
                    SKU = product.SKU,
                    Name = product.Name,
                    ProductType = product.ProductType,
                    Image = competitor != null ? competitor.Image : "",
                    Price = competitor != null ? competitor.Price.ConvertPriceToString() : "N/A",
                    PriceMin = competitor != null ? competitor.PriceMin.ConvertPriceToString() : "N/A",
                    PriceMax = competitor != null ? competitor.PriceMax.ConvertPriceToString() : "N/A",
                    PriceManual = competitor != null ? competitor.PriceManual.ConvertPriceToString() : "N/A",
                    HistoricalSold = competitor != null ? competitor.HistoricalSold : "N/A",
                    NumberOfComment = competitor != null ? competitor.NumberOfComment : "N/A",
                    CreatedDate = product.CreatedDate
                });
            }

            return PartialView(result.OrderBy(t => t.CreatedDate).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> AddEditProduct([FromBody] AddEditProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(model);
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                var addProduct = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name.Trim(),
                    ProductType = model.IsHeroType ? ProductType.Hero : ProductType.Nomal,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Add(addProduct);
                await _context.SaveChangesAsync();

                return Json(addProduct);
            }
            else
            {
                var editProduct = await _context.Products.FirstOrDefaultAsync(t => t.Id == Guid.Parse(model.Id));
                editProduct.Name = model.Name.Trim();
                editProduct.ProductType = model.IsHeroType ? ProductType.Hero : ProductType.Nomal;
                editProduct.ModifiedDate = DateTime.UtcNow;

                _context.Entry(editProduct).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Json(editProduct);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromBody] ExportExcelViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(null);
            }

            var products = await _context.Competitors
                .Where(t => model.ProductIds.Contains(t.Product.Id.ToString()))
                .Select(t => new ExportExcelDataDto
                {
                    SKU = t.Product.SKU,
                    ProductType = t.Product.ProductType,
                    ProductName = t.Product.Name,
                    CompetitorName = t.Name,
                    Link = t.Link,
                    PriceMin = t.PriceMin.ConvertPriceToString(),
                    PriceMax = t.PriceMax.ConvertPriceToString(),
                    PriceManual = t.PriceManual.ConvertPriceToString(),
                    HistoricalSold = t.HistoricalSold,
                    NumberOfComment = t.NumberOfComment
                })
                .OrderBy(t => t.ProductName)
                .AsNoTracking()
                .ToListAsync();

            if (!products.Any())
            {
                return Json(null);
            }

            var fileGuid = Guid.NewGuid().ToString();
            var fileName = $"Danh sach SP {DateTime.Now.ToString("yyyy-MM-dd HH-mm")}.xlsx";
            var memoryStream = new MemoryStream();
            using (var xlPackage = new ExcelPackage(memoryStream))
            {
                var worksheet = xlPackage.Workbook.Worksheets.Add("Users");
                var namedStyle = xlPackage.Workbook.Styles.CreateNamedStyle("HyperLink");
                namedStyle.Style.Font.UnderLine = true;
                namedStyle.Style.Font.Color.SetColor(Color.Blue);
                const int startRow = 2;
                var row = startRow;

                //Create Headers and format them
                worksheet.Cells["A1"].Value = "STT";
                worksheet.Cells["B1"].Value = "Sản phẩm Hero";
                worksheet.Cells["C1"].Value = "Mã SP";
                worksheet.Cells["D1"].Value = "Tên SP";
                worksheet.Cells["E1"].Value = "Đối Thủ";
                worksheet.Cells["F1"].Value = "Giá Bán";
                worksheet.Cells["G1"].Value = "Lượt Bán";
                worksheet.Cells["H1"].Value = "Lượt Đánh Giá";
                worksheet.Cells["I1"].Value = "Link SP";

                worksheet.Cells["A1:I1"].Style.Font.Bold = true;

                row = 2;

                for (int i = 0; i < products.Count; i++)
                {
                    worksheet.Cells[row, 1].Value = i + 1;
                    worksheet.Cells[row, 2].Value = products[i].ProductType == ProductType.Hero ? 1 : 0;
                    worksheet.Cells[row, 3].Value = products[i].SKU;
                    worksheet.Cells[row, 4].Value = products[i].ProductName;
                    worksheet.Cells[row, 5].Value = products[i].CompetitorName;

                    worksheet.Cells[row, 7].Value = products[i].HistoricalSold;
                    worksheet.Cells[row, 8].Value = products[i].NumberOfComment;
                    worksheet.Cells[row, 9].Value = products[i].Link;

                    if (!string.IsNullOrEmpty(products[i].PriceManual))
                    {
                        worksheet.Cells[row, 6].Value = products[i].PriceManual;
                    }
                    else if (products[i].PriceMin == products[i].PriceMax)
                    {
                        worksheet.Cells[row, 6].Value = products[i].PriceMin;
                    }
                    else
                    {
                        worksheet.Cells[row, 6].Value = $"{products[i].PriceMin} - {products[i].PriceMax}";
                    }

                    row++;
                }
                xlPackage.Save();

            }
            memoryStream.Position = 0;
            _memoryCache.Set(fileGuid, memoryStream.ToArray());

            return Json($"?fileGuid={fileGuid}&filename={fileName}");
        }

        public ActionResult Download(string fileGuid, string fileName)
        {
            if (_memoryCache.Get(fileGuid) != null)
            {
                byte[] data = _memoryCache.Get(fileGuid) as byte[];
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                return new EmptyResult();
            }
        }
    }
}
