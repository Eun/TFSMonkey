namespace TFSMonkey
{
	internal class HistoryItemFactory : TFSCore.HistoryItemFactory
	{
		public TFSCore.IHistoryItem CreateItem()
		{
			return new HistoryItem();
		}
	}
}