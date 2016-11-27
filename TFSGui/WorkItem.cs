using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using FontAwesome.WPF;
using LiteMiner.classes;

namespace TFSMonkey
{
	public class WorkItem : TFSCore.WorkItem
	{

		public string StateIcon => Settings.GetStateIcon(State);

		public Color StateColor => Settings.GetStateColor(State);

		public string WorkItemTypeIcon => Settings.GetWorkItemTypeIcon(WorkItemType);
		public Color WorkItemTypeColor => Settings.GetWorkItemTypeColor(WorkItemType);

		public Settings Settings { get; set; }


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
			((List<string>)words).AddRange(_whitespaceRegex.Replace(this.AssignedTo.ToLowerInvariant(), " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			((List<string>)words).AddRange(_whitespaceRegex.Replace(this.State.ToLowerInvariant(), " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			((List<string>)words).AddRange(_whitespaceRegex.Replace(this.Title.ToLowerInvariant(), " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			((List<string>)words).AddRange(_whitespaceRegex.Replace(this.WorkItemType.ToLowerInvariant(), " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			((List<string>)words).Add(this.Id.ToString());

			words = words.Distinct().ToList();
			if (detectLanguage)
				StopWords.FilterStopWords(words, LanguageDetector.Detect(Title) ?? "en");
			_indexedWords = string.Join(" ", words);
		}

		private static IDictionary<string, string> friendlyAssignedNames = new Dictionary<string, string>();

		public override string AssignedTo
		{
			get { return FriendlyUserNames.Resolve(base.AssignedTo); }
			set { base.AssignedTo = value; }
		}
	}
}
