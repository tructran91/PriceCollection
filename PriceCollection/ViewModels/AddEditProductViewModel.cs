using System.ComponentModel.DataAnnotations;

namespace PriceCollection.ViewModels
{
    public class AddEditProductViewModel
    {
        public string? Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsHeroType { get; set; }
    }
}
