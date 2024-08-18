namespace OrderService.Model{
public class Order
{
    public int ID { get; set; }
    public required string ProductType { get; set; }
    public DateTime Date { get; set; }

    public required string Address { get; set; }
}
}