namespace Shop___Code_first.Model.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Client { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime Ordered { get; set; }
        public virtual Product Product { get; set; }
    }
}
