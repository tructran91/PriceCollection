using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using PriceCollection.Configurations;
using PriceCollection.Models;
using System.Net;
using System.Security.Policy;
using System.Text;

namespace PriceCollection
{
    public interface IPriceService
    {
        Task GetCompetitorPriceAllProducts();

        Task<ResponseResultModel> GetProductInfo(string url);
    }

    public class PriceService : IPriceService
    {
        private readonly PriceCollectionContext _context;

        public PriceService(PriceCollectionContext context)
        {
            _context = context;
        }

        public async Task GetCompetitorPriceAllProducts()
        {
            var competitors = await _context.Competitors.ToListAsync();

            foreach (var competitor in competitors)
            {
                var productInfo = await GetProductInfo(competitor.Link);

                if (productInfo.error != null || productInfo.error_msg != null)
                {
                    competitor.ErrorMessage = productInfo.error_msg;
                    competitor.ModifiedDate = DateTime.UtcNow;

                    _context.Entry(competitor).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    continue;
                }

                if (competitor.PriceMin != productInfo.data.price_min || competitor.PriceMax != productInfo.data.price_max || competitor.Price != productInfo.data.price)
                {
                    competitor.Price = productInfo.data.price;
                    competitor.PriceMin = productInfo.data.price_min;
                    competitor.PriceMax = productInfo.data.price_max;
                    competitor.PriceManual = null;
                }

                competitor.HistoricalSold = productInfo.data.historical_sold.ToString();
                competitor.NumberOfComment = productInfo.data.cmt_count.ToString();
                competitor.Image = productInfo.data.image;
                competitor.ErrorMessage = null;
                competitor.ModifiedDate = DateTime.UtcNow;

                _context.Entry(competitor).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ResponseResultModel> GetProductInfo(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var shopInfo = Utilities.GetShopIdAndItemIdFromUrl(url);
                    if (string.IsNullOrEmpty(shopInfo.ShopId) || string.IsNullOrEmpty(shopInfo.ItemId))
                    {
                        return new ResponseResultModel { error_msg = "ShopId or ItemId is wrong." };
                    }

                    var urlApi = string.Format("https://shopee.vn/api/v4/item/get?shopid={0}&itemid={1}", shopInfo.ShopId, shopInfo.ItemId);

                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
                    client.DefaultRequestHeaders.Add("Af-Ac-Enc-Dat", "AAcyLjguMS0yAAABiP+rdz0AABCSAyAAAAAAAAAAAlkZhtesQ10lVqY3YiM+q8pIkTzn0/lH6G4dKNMBG9vGRC7WHZAV3Pz4tdRDdk3zIEsLMwfRHJJ2HKErNOh5euwKv/tWe3jEDgc/b4YJqq6C4YjhOkvhqTon5R7JEOvbjnxwGWImhBGVTUHJzE1dXr0V/YLnrwogDvUM8pc5CYXz1pDADhyjdDoubftNqVf744rZi4qSccV8/6Vvz5Qv+LxQBgPZ2Dx2OcsWXIVIneHkb8ulhBcn/l6/D67UjkAMSl/ai/OLrpntri8gk6HSvD8K57gxaQ36xI5xyTi/ZejWZzp1EEq08zASYkoKJCzKWw6ymDSavjGHBHQfw1tKIBMAZs/zBzDE+c6HN687yGhgQuEUz5f/NhMnw8f21UXD2mkb2JvcbsCCD1438aDU6acEaD4pfyRVhk2rWReXCLKJg7nHMyS5Xiy7g9cVrQ5Ij038ATfbWHg6kaZe59YyHXIthKwuHY5feaDy116QNyMizysQRyUK6wdAQ8xH9DlP8vmsnWF9P1CZPMj3pOsas2WcVr1a6MKcNUdwC4O4o8rn0rNXOHGrJWHTpqPaFqMlbOWprrpFqfijrDBuPVpVH2ndGmU9u1pI+I5hqMRx80PAw/eO5BbzWghGh6BZEUcdJ646iZkambmsiDuTPBlO3H4zxywAJrnls2dFM3zNRt+OSNo+pszoUhKAg6vJXYPkXE5ho+pc0XG8frvsLhQK78fs7www8WJ6JV771K7M3S4Ty2Ncm8vFui5C+Cokhc47s9IEFIsGDUtpEpNSSI0oOt3tagOTHkenEhnNI10zaevKlBjvvsvFui5C+Cokhc47s9IEFIsGDUtpEpNSSI0oOt3tagOTUWJy02xIVh23BkGRTuyWQfap+xOCy0qf6FceYCNc1JRGrbfJWuRYlFp+J0tcUk6vRQg7nCQX6c8aLMHSU1udIlj4f3pW05can3G4luWGcOFGrbfJWuRYlFp+J0tcUk6vgCDxiBpIlUwbzBjpEgESiJf/NhMnw8f21UXD2mkb2JvGxJsAHg8YOTDoomXzOhPDOWAbosnO1+hOu102z4A5Ww==");

                    using (HttpResponseMessage response = await client.GetAsync(urlApi))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            return new ResponseResultModel { error_msg = "Cannot get Data" };
                        }

                        var byteArray = await response.Content.ReadAsByteArrayAsync();
                        var content = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);

                        return JsonConvert.DeserializeObject<ResponseResultModel>(content);
                    }
                }
                catch (Exception ex)
                {
                    return new ResponseResultModel { error_msg = "Cannot get Data" };
                    throw;
                }

            }
        }
    }
}
