using PriceCollection.DTOs;

namespace PriceCollection
{
    public static class Utilities
    {
        public static ProductInfoDto GetShopIdAndItemIdFromUrl(string url)
        {
            string reverseUrl = ReverseString(url);
            var indexSecondDot = reverseUrl.IndexOf('.', reverseUrl.IndexOf('.') + 1);
            var shopIdAndItemIdReversed = reverseUrl.Substring(0, indexSecondDot);
            var shopIdAndItemId = ReverseString(shopIdAndItemIdReversed);

            var shopId = shopIdAndItemId.Substring(0, shopIdAndItemId.IndexOf('.'));
            var itemId = shopIdAndItemId.Substring(shopIdAndItemId.IndexOf('.') + 1);

            return new ProductInfoDto
            {
                ShopId = shopId,
                ItemId = itemId
            };
        }

        private static string ReverseString(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
