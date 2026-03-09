namespace LibDemoShop
{
    public class PagedResultDTO<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
