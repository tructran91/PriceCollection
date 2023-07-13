namespace PriceCollection.Models
{
    public class Competitor
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? Link { get; set; }

        public bool IsOwner { get; set; }

        public double? PriceMin { get; set; }

        public double? PriceMax { get; set; }

        public double? Price { get; set; }

        public double? PriceManual { get; set; }

        public string? HistoricalSold { get; set; }

        public string? NumberOfComment { get; set; }

        public string? Image { get; set; }

        public string? ErrorMessage { get; set; }

        public Guid ProductId { get; set; }

        public Product Product { get; set; }

        public string? Note { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
