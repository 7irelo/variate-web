namespace Variate.Entities;

public class Product
{
    public int Id { get; set; }
    public reqiured string Name { get; set; }
    public int GenreId { get; set; }
    public Genre? Genre{ get; set; }
    public decimal Price { get; set; }
    public DateOnly Release { get; set; }
}
