namespace RedirectRequestsServer.Model
{
    public class Product
    {
        public int ID { get; set; }
        public string VendorCode { get; set; }
        public string Photo { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Manufacturer { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
