using PriceCollection.Models;

namespace PriceCollection.ViewModels
{
    public class ProductViewModel
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public string? PriceMin { get; set; }

        public string? PriceMax { get; set; }

        public string? Price { get; set; }

        public string? PriceManual { get; set; }

        public string? HistoricalSold { get; set; }

        public string? NumberOfComment { get; set; }

        public string? Image { get; set; }

        public string? SKU { get; set; }

        public ProductType ProductType { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
