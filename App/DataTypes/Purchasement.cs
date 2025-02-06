namespace App.DataTypes;

public class Record
{
	public string Id { get; init; }
	public List<Transaction> Transactions { get; init; }

	public int TotalPrice { get; init; }
	public int TotalQuantity { get; init; }

	// Uses the latest timestamp of the transactions
	public DateTime Timestamp => Transactions.Max(x => x.Timestamp);

	public Record(string recordId)
	{
		// Initialize the id
		Id = recordId;

		// Initialize the transactions
		var allTransactions = TransactionManager.GetTransactions();
		Transactions = allTransactions.Where(x => x.RecordId == recordId).ToList();

		// Initialize the total price and quantity
		var items = ItemManager.GetItems();
		int totalPrice = 0;
		int totalQuantity = 0;

		// Calculate the total price and quantity
		foreach(var transaction in Transactions)
		{
			var item = items.FirstOrDefault(x => x.Id == transaction.ItemId);
			if(item == null) continue;

			totalPrice += item.Price * transaction.Quantity;
			totalQuantity += transaction.Quantity;
		}

		// Apply the total price and quantity
		TotalPrice = totalPrice;
		TotalQuantity = totalQuantity;
	}
}
