using System.ComponentModel.DataAnnotations;

namespace PriceCollection.ViewModels
{
    public class ExportExcelViewModel
    {
        [Required]
        public List<string> ProductIds { get; set; }
    }
}
