namespace PriceCollection.Extensions
{
    public static class PriceExtension
    {
        public static string ConvertPriceToString(this double? price)
        {
            if (price == null) return string.Empty;

            var priceStr = price.Value.ToString().Substring(0, price.Value.ToString().Length - 5);
            var priceDouble = double.Parse(priceStr);
            return priceDouble.ToString("N0");
        }

        public static double ConvertPriceToDouble(this string? price)
        {
            if (price == null) return 0;

            var newPrice = price + "00000";
            return double.Parse(newPrice);
        }
    }
}
