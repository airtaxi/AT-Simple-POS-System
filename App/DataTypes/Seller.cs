namespace App.DataTypes;

public class Seller
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; }
    public string Contact { get; set; } = "";
}
