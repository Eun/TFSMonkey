using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TFSMonkey
{
	class StopWords
	{

		private static readonly IDictionary<string, IEnumerable<string>> filterWordDictionary = new Dictionary<string, IEnumerable<string>>();

		public static void FilterStopWords(List<string> words, string language)
		{
			IEnumerable<string> stopWords;
			if (!filterWordDictionary.TryGetValue(language, out stopWords))
			{
				stopWords = GetStopWords(language);
				filterWordDictionary.Add(language, stopWords);
			}
			
			foreach (var stopWord in stopWords)
			{
				for (var i = words.Count - 1; i >= 0; --i)
				{
					if (string.Compare(words[i], stopWord, StringComparison.CurrentCultureIgnoreCase) == 0)
					{
						words.RemoveAt(i);
					}
				}

			}
		}

		private static IEnumerable<string> GetStopWords(string language)
		{
			var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".stopwords-all.json");
			{
				using (var textReader = new StreamReader(manifestResourceStream))
				{
					using (var jsonReader = new JsonTextReader(textReader))
					{
						var words = new List<string>();
						while (jsonReader.Read())
						{
							if (jsonReader.TokenType == JsonToken.PropertyName && jsonReader.Value is string && string.CompareOrdinal((string)jsonReader.Value, language) == 0)
							{
								while (jsonReader.Read())
								{
									if (jsonReader.TokenType == JsonToken.EndArray)
									{
										return words;
									}
									else if (jsonReader.TokenType == JsonToken.String && jsonReader.Value is string)
									{
										words.Add(((string) jsonReader.Value).ToLowerInvariant());
									}
								}
							}
						}
						return words;
					}
				}
			}
		}
	}
}
