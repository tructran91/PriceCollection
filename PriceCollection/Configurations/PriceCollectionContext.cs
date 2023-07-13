using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PriceCollection.Models;
using System.Reflection.Metadata;

namespace PriceCollection.Configurations
{
    public class PriceCollectionContext : DbContext
    {
        public PriceCollectionContext(DbContextOptions<PriceCollectionContext> options) : base(options)
        {
        }

        public DbSet<Competitor> Competitors { get; set; }

        public DbSet<Product> Products { get; set; }

    }
}
