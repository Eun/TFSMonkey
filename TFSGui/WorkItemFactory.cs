namespace TFSMonkey
{
	internal class WorkItemFactory : TFSCore.WorkItemFactory
	{
		public Settings Settings { get; set; }

		public WorkItemFactory(Settings settings)
		{
			Settings = settings;
		}

		public TFSCore.IWorkItem CreateItem()
		{
			return new WorkItem {Settings = Settings};
		}
	}
}