namespace ApiDemoShop.Model
{
    public class DeliveryMethod
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price {  get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Order> Orders { get; set; }=new List<Order>();
    }
}
