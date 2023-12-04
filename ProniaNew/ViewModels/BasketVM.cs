namespace ProniaNew.ViewModels
{
    public class BasketVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal SubTotal { get; set; }
    }
}
