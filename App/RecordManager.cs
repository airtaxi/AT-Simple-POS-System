using App.DataTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace App;

public static class RecordManager
{
	public static List<Record> GetAllRecords()
	{
		var transactions = TransactionManager.GetTransactions();
		var recordIds = transactions.Select(x => x.RecordId).Distinct();
		var records = recordIds.Select(x => new Record(x)).ToList();
		return records;
	}

	public static List<Record> GetRecordsBy(Func<Transaction, bool> predicate)
	{
		var transactions = TransactionManager.GetTransactions();
		var recordIds = transactions.Where(predicate).Select(x => x.RecordId).Distinct();
		var records = recordIds.Select(x => new Record(x)).ToList();
		return records;
	}
}
