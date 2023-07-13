namespace PriceCollection.ViewModels
{
    public class CompetitorViewModel
    {
        public Guid CompetitorId { get; set; }

        public string Name { get; set; }

        public string Link { get; set; }

        public string? PriceMin { get; set; }

        public string? PriceMax { get; set; }

        public string? Price { get; set; }

        public string? PriceManual { get; set; }

        public string? HistoricalSold { get; set; }

        public string? NumberOfComment { get; set; }

        public bool IsOwner { get; set; }

        public string? Image { get; set; }

        public string? Note { get; set; }
    }
}
