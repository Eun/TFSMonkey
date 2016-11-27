using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using FontAwesome.WPF;
using LiteMiner.classes;
using TFSCore;

namespace TFSMonkey
{
	public class HistoryItem : TFSCore.HistoryItem, INotifyPropertyChanged
	{

		//public event EventHandler GotWorkItems;

		public ICollection<IWorkItem> CachedWorkItems { get; set; } = new IWorkItem[0];

		public override ICollection<IWorkItem> WorkItems
		{
			get
			{
				var items = base.WorkItems;
				CachedWorkItems = items;
				CachedWorkItemIds = items.Select(x => x.Id).ToArray();
				OnPropertyChanged(nameof(CachedWorkItems));
				return items;
			}
		}

		public int[] CachedWorkItemIds { get; set; } = new int[0];
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private string _indexedWords;

		private static Regex _whitespaceRegex = new Regex(@"\s+");

		private static readonly LanguageDetector LanguageDetector = new LanguageDetector();

		public string IndexedWords
		{
			get
			{
				if (_indexedWords == null)
				{
					UpdateIndexedWords(false);
				}
				return _indexedWords;
			}
			set { _indexedWords = value; }
		}

		public void UpdateIndexedWords(bool detectLanguage)
		{
			var words = new List<string>();
			((List<string>)words).AddRange(_whitespaceRegex.Replace(this.Comment.ToLowerInvariant(), " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			((List<string>)words).AddRange(_whitespaceRegex.Replace(this.Committer.ToLowerInvariant(), " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			((List<string>)words).AddRange(_whitespaceRegex.Replace(this.CommitterDisplayName.ToLowerInvariant(), " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			((List<string>)words).AddRange(_whitespaceRegex.Replace(this.Owner.ToLowerInvariant(), " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			((List<string>)words).AddRange(_whitespaceRegex.Replace(this.OwnerDisplayName.ToLowerInvariant(), " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			((List<string>)words).Add(this.ChangeSetId.ToString());

			words = words.Distinct().ToList();
			if (detectLanguage)
				StopWords.FilterStopWords(words, LanguageDetector.Detect(Comment) ?? "en");
			_indexedWords = string.Join(" ", words);

			foreach (WorkItem workItem in this.CachedWorkItems)
			{
				_indexedWords += workItem.IndexedWords;
			}
		}

		public override string CommitterDisplayName
		{
			get { return FriendlyUserNames.Resolve(base.CommitterDisplayName); }
			set { base.CommitterDisplayName = value; }
		}
	}
}
