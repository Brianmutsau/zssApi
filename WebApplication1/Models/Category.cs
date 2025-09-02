namespace WebApplication1.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public List<Book> Books { get; set; } = new();
    }
}
