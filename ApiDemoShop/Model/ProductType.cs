namespace ApiDemoShop.Model
{
    public class ProductType
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;

        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
