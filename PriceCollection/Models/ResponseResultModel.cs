namespace PriceCollection.Models
{
    public class ResponseResultModel
    {
        public string error { get; set; }

        public string error_msg { get; set; }

        public DataModel data { get; set; }
    }

    public class DataModel
    {
        public int cmt_count { get; set; }

        public int historical_sold { get; set; }

        public double price_min { get; set; }

        public double price_max { get; set; }

        public double price { get; set; }

        public string image { get; set; }
    }
}
