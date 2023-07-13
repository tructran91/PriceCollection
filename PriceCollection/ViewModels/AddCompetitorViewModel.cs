using System.ComponentModel.DataAnnotations;

namespace PriceCollection.ViewModels
{
    public class AddCompetitorViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Link { get; set; }

        [Required]
        public string ProductId { get; set; }

        public bool IsOwner { get; set; }

        public string? Note { get; set; }
    }
}
