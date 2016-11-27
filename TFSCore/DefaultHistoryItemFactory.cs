using System;

namespace TFSCore
{
	public class DefaultHistoryItemFactory : HistoryItemFactory
	{
		public IHistoryItem CreateItem()
		{
			return new HistoryItem();
		}
	}
}