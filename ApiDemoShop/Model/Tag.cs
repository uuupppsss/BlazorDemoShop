namespace ApiDemoShop.Model
{
    public class Tag
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int TypeId { get; set; }

        public virtual ProductType Type { get; set; } = null!;
        public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
    }
}
