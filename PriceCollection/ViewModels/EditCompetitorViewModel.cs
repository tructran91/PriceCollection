using System.ComponentModel.DataAnnotations;

namespace PriceCollection.ViewModels
{
    public class EditCompetitorViewModel
    {
        [Required]
        public string Id { get; set; }

        public string? NewPrice { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Link { get; set; }

        public string? Note { get; set; }
    }
}
