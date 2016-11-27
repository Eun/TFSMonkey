using System;

namespace TFSCore
{
	public class DefaultWorkItemFactory : WorkItemFactory
	{
		public IWorkItem CreateItem()
		{
			return new WorkItem();
		}
	}
}