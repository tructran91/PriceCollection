namespace PriceCollection.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public ProductType ProductType { get; set; }

        public string? SKU { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }

    public enum ProductType
    {
        Nomal = 1,
        Hero
    }
}
