using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TFSCore
{
	public abstract class IWorkItem
	{
		public abstract int Id { get; set; }
		public abstract string Title { get; set; }
		public abstract string AssignedTo { get; set; }
		public abstract string WorkItemType { get; set; }
		public abstract string State { get; set; }
		public abstract DateTime ChangedDate { get; set; }
		public abstract string ArtifactUrl { get; set; }

		internal static ICollection<IWorkItem> FromArtifacts(IEnumerable<Artifact> artifacts, WorkItemFactory workItemFactory)
		{
			if (null == artifacts)
			{
				return new IWorkItem[0];
			}

			ICollection<IWorkItem> col = new List<IWorkItem>();

			foreach (var artifact in artifacts)
			{
				if (artifact == null)
				{
					continue;
				}

				IWorkItem workItem = workItemFactory.CreateItem();

				workItem.ArtifactUrl = artifact.Uri;

				foreach (ExtendedAttribute ea in artifact.ExtendedAttributes)
				{
					if (string.Equals(ea.Name, "System.Id", StringComparison.OrdinalIgnoreCase))
					{
						int workItemId;

						if (Int32.TryParse(ea.Value, out workItemId))
						{
							workItem.Id = workItemId;
						}
					}
					else if (string.Equals(ea.Name, "System.Title", StringComparison.OrdinalIgnoreCase))
					{
						workItem.Title = ea.Value;
					}
					else if (string.Equals(ea.Name, "System.AssignedTo", StringComparison.OrdinalIgnoreCase))
					{
						workItem.AssignedTo = ea.Value;
					}
					else if (string.Equals(ea.Name, "System.State", StringComparison.OrdinalIgnoreCase))
					{
						workItem.State = ea.Value;
					}
					else if (string.Equals(ea.Name, "System.WorkItemType", StringComparison.OrdinalIgnoreCase))
					{
						workItem.WorkItemType = ea.Value;
					}
					else if (string.Equals(ea.Name, "System.ChangedDate", StringComparison.OrdinalIgnoreCase))
					{
						DateTime changedDate;
						if (DateTime.TryParse(ea.Value, out changedDate))
						{
							workItem.ChangedDate = changedDate;
						}
					}
				}

				Debug.Assert(workItem.Id != 0, "Unable to decode artifact into AssociatedWorkItemInfo object.");

				if (workItem.Id != 0)
				{
					col.Add(workItem);
				}
			}

			return col;
		}

	}
	public class WorkItem : IWorkItem
	{
		public override int Id {get; set; }
		public override string Title { get; set; }
		public override string AssignedTo { get; set; }
		public override string WorkItemType { get; set; }
		public override string State { get; set; }
		public override DateTime ChangedDate { get; set; }
		public override string ArtifactUrl { get; set; }

		
	}
}