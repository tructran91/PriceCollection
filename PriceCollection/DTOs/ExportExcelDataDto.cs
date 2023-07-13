using PriceCollection.Models;

namespace PriceCollection.DTOs
{
    public class ExportExcelDataDto
    {
        public string SKU { get; set; }

        public ProductType ProductType { get; set; }

        public string ProductName { get; set; }

        public string CompetitorName { get; set; }

        public string PriceMin { get; set; }

        public string PriceMax { get; set; }

        public string? PriceManual { get; set; }

        public string HistoricalSold { get; set; }

        public string NumberOfComment { get; set; }

        public string Link { get; set; }
    }
}
