namespace Practica1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Artcl { get; set; }
        public int TitleId { get; set; }
        public string TitleName { get; set; }
        public string UnitOfMeasurement { get; set; }
        public decimal Price { get; set; }
        public int SuppliersId { get; set; }
        public string SupplierName { get; set; }
        public int ManufacturesId { get; set; }
        public string ManufactureName { get; set; }
        public int CategoriesProducts { get; set; }
        public string CategoryName { get; set; }
        public int Discount { get; set; }
        public int Qty { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}