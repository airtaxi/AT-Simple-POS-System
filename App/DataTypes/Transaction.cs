namespace App.DataTypes;

public class Transaction
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    // Item related properties
	public string ItemId { get; init; }
    public int Quantity { get; init; }

    // Record related properties
    public string RecordId { get; init; }
    public DateTime Timestamp { get; init; }

    public Transaction(string recordId, string itemId, int quantity, DateTime timestamp)
    {
        // Setup item related properties
		ItemId = itemId;
		Quantity = quantity;

        // Setup record related properties
        Timestamp = timestamp;
		RecordId = recordId;
	}
}
