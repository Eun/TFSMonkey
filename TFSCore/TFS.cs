using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TFSCore
{
    public class TFS
    {

	    private TfsTeamProjectCollection _connection;
	    private VersionControlServer _versionControlServer;
		private ILinking _linkingService;

		public delegate void CommitCheckinHandler(object sender, HistoryItem args);

	    public HistoryItemFactory HistoryItemFactory { get; set; } = new DefaultHistoryItemFactory();
		public WorkItemFactory WorkItemFactory { get; set; } = new DefaultWorkItemFactory();


		public TFS(Uri server, NetworkCredential credential)
		{
			BasicAuthCredential basicCred = new BasicAuthCredential(credential);
			var tfsCred = new TfsClientCredentials(basicCred);
			tfsCred.AllowInteractive = false;
			_connection.ClientCredentials = tfsCred;
			Initialize(server, tfsCred);	
		}

		public TFS(Uri server)
	    {
			Initialize(server, null);
		}

		public TFS(Uri server, string username, SecureString password)
		{
			Initialize(server, new TfsClientCredentials(new BasicAuthCredential(new NetworkCredential(username, password))));
		}

		public TFS(Uri server, string username, SecureString password, string domain)
		{
			Initialize(server, new TfsClientCredentials(new BasicAuthCredential(new NetworkCredential(username, password, domain))));
		}

	    private void Initialize(Uri server, TfsClientCredentials credential)
	    {

			_connection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(server);

			if (credential != null)
			{
				_connection.ClientCredentials = credential;
			}

			_connection.EnsureAuthenticated();

			_versionControlServer = _connection.GetService<VersionControlServer>();
			_linkingService = _connection.GetService<ILinking>();
			Url = server.AbsoluteUri;
			
		}




	    ~TFS()
	    {
		    try
		    {
			    _connection.Disconnect();
		    }
		    catch
		    {
			    // ignored
		    }
	    }

	    public string Url { get; set; }


	    public IEnumerable<TeamProject> GetTeamProjects()
	    {
		    return _versionControlServer.GetAllTeamProjects(true).Select(project => new TeamProject()
		    {
			    Name = project.Name, ServerItem = project.ServerItem
		    });
	    }





	    public IEnumerable<IHistoryItem> GetHistory(string serverPath)
	    {
		    return GetHistory(serverPath, null, Int32.MaxValue);
	    }

		public IEnumerable<IHistoryItem> GetHistory(string serverPath, int count)
		{
			return GetHistory(serverPath, null, count);
		}



		public IEnumerable<IHistoryItem> GetHistory(string serverPath, int? lastId, int count)
	    {
			Debug.WriteLine($"Getting History for {serverPath} {lastId} {count}");
			var col = new List<IHistoryItem>();
		    var items = _versionControlServer.QueryHistory(serverPath, VersionSpec.Latest, 0, RecursionType.Full, null, lastId.HasValue ? new ChangesetVersionSpec(lastId.Value) : null, null, count, false, true, false, false);

			foreach (var item in items)
			{
				var changeSet = item as Changeset;
				if (changeSet != null)
				{
					var historyItem = HistoryItemFactory.CreateItem();
					historyItem.ChangeSet = changeSet;
					historyItem.LinkingService = _linkingService;
					historyItem.ChangeSetId = changeSet.ChangesetId;
					historyItem.Committer = changeSet.Committer;
					historyItem.CommitterDisplayName = changeSet.CommitterDisplayName;
					historyItem.Comment = changeSet.Comment;
					historyItem.CreationDate = changeSet.CreationDate;
					historyItem.Owner = changeSet.Owner;
					historyItem.OwnerDisplayName = changeSet.OwnerDisplayName;
					historyItem.WorkItemFactory = WorkItemFactory;
					col.Add(historyItem);
				}
				else
				{
					Debug.Assert(true, "Item is not a Changeset, it is a " + typeof(Changeset).Name);
				}
			}
			Debug.WriteLine($"{col.Count} HistoryItems Fetched");
			return col.OrderByDescending(item => item.ChangeSetId);
	    }

		public IEnumerable<IWorkItem> GetWorkItems(ICollection<HistoryItem> historyItems)
		{
			Debug.WriteLine($"Getting WorkItems for {historyItems.Count} HistoryItems");
			LinkFilter linkFilter = new LinkFilter();
			linkFilter.FilterType = FilterType.ToolType;
			linkFilter.FilterValues = new String[1] { ToolNames.WorkItemTracking };

			Artifact[] artifacts = _linkingService.GetReferencingArtifacts(historyItems.Select(item => item.ChangeSet.ArtifactUri.AbsoluteUri).ToArray(), new LinkFilter[1] { linkFilter });
			var col = IWorkItem.FromArtifacts(artifacts, WorkItemFactory);
			Debug.WriteLine($"{col.Count} WorkItems Fetched");
			return col.OrderByDescending(item => item.Id);
		}

	}
}
