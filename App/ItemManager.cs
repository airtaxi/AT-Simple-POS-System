using App.DataTypes;

namespace App;

public static class ItemManager
{
	public static List<Item> GetItems()
	{
		// Get the items from the configuration or create a new one
		var items = Configuration.GetValue<List<Item>>("Items") ?? [];
		return items;
	}

	private static void AddItem(Item item)
	{
		var items = GetItems();

		// Add the item and save the configuration
		items.Add(item);
		Configuration.SetValue("Items", items);
        Configuration.WriteBuffer();
    }

	public static void SaveItem(Item item)
	{
		var items = GetItems();

		// Find the item. If the item is not found, index will be -1
		var index = items.FindIndex(x => x.Id == item.Id);
		
		// If the item is not found, add it
		if (index < 0)
		{
			AddItem(item);
			return;
		}

		// If the item is found, replace it. And save the configuration
		items[index] = item;
		Configuration.SetValue("Items", items);
        Configuration.WriteBuffer();
    }

	public static void RemoveItem(string itemId)
	{
		var items = GetItems();

		// Remove the item and save the configuration
		items.RemoveAll(x => x.Id == itemId);
		Configuration.SetValue("Items", items);
        Configuration.WriteBuffer();
    }

	public static void ClearItems()
	{
        // Clear the items and save the configuration
        Configuration.SetValue("Items", new List<Item>());
        Configuration.WriteBuffer();
    }
}
