/*
 * @file WordMap.cs
 *
 * Copyright (C) 2016 Jeremy Glazman
 * All Rights Reserved.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Glazman.Shapeshift
{
	public enum WordType
	{
		None = 0x0000,
		Adjective = 0x0001,
		Adverb = 0x0002,
		Combining = 0x0004,
		Conjunction = 0x0010,
		Definite = 0x0020,
		Determiner = 0x0040,
		Indefinite = 0x0080,
		Interjection = 0x0100,
		Noun = 0x0200,
		Plural = 0x0400,
		Preposition = 0x0800,
		Pronoun = 0x1000,
		Verb = 0x2000,
		NotFound = 0x4000
	}

	public class WordMap
	{
		private Dictionary<char, WordMap> _map;

		private int _wordTypes = (int)WordType.NotFound;
		private int WordTypes { get { return _wordTypes; } }
		

		public static WordMap Words { get; private set; }

		public static void Initialize()
		{
			CoroutineRunner.Run(LoadWords_Coroutine((wm) => { Words = wm; }));
		}

		private static IEnumerator LoadWords_Coroutine(System.Action<WordMap> callback)
		{
			WordMap wordMap = new WordMap();

			TextAsset wordFile = Resources.Load<TextAsset>("words");
			string[] lines = wordFile.text.Split('\n');
			int numLines = lines.Length;
			Resources.UnloadAsset(wordFile);

			Logger.LogEditor("Found words: " + numLines);

			int count = 0;
			float startTime = Time.time;
			for (int i = 0; i < numLines; i++)
			{
				var line = lines[i];
				var length = line.Length;

				if (length < 2)
					continue;

				wordMap.AddWord(line);

				count++;
				if (count % 5000 == 0)
					yield return null;
			}

			Logger.LogEditor($"Added {count} words to the WordMap. ({(Time.time - startTime)}s)");

			callback(wordMap);
		}

		private void SetWordTypes(int types)
		{
			_wordTypes = types;
		}

		private void AddWord(string word)
		{
			char c = word[0];

			if (c == '=')
			{
				SetWordTypes(WordMap.GetEncodedWordTypes(word.Substring(1)));
			}
			else
			{
				WordMap m;

				if (_map == null)
					_map = new Dictionary<char, WordMap>();

				if (!_map.TryGetValue(c, out m))
				{
					m = new WordMap();
					_map.Add(c, m);
				}

				if (word.Length > 1)
					m.AddWord(word.Substring(1));
				else
					m.SetWordTypes((int)WordType.None);
			}
		}

		public int FindWord(string word)
		{
			if (_map != null && !string.IsNullOrEmpty(word))
			{
				char c = word[0];
				WordMap m = null;

				if (_map.TryGetValue(c, out m))
				{
					if (word.Length == 1)
					{
						return m.WordTypes;
					}

					return m.FindWord(word.Substring(1));
				}
			}

			return (int)WordType.NotFound;
		}

		private static int GetEncodedWordTypes(string str)
		{
			int wordTypes = 0;
			string[] types = str.Split(',');

			for (int i = 0; i < types.Length; i++)
			{
				WordType foundType;
				if (ENCODED_WORD_TYPES.TryGetValue(types[i], out foundType))
				{
					wordTypes |= (int)foundType;
				}
			}

			return wordTypes;
		}

		public static bool IsWordType(int wordTypes, WordType isType)
		{
			return (wordTypes & (int)isType) != 0;
		}

		private static Dictionary<string, WordType> ENCODED_WORD_TYPES = new Dictionary<string, WordType>()
		{
			{"a", WordType.Adjective},
			{"c", WordType.Conjunction},
			{"d", WordType.Adverb},
			{"e", WordType.Indefinite},
			{"f", WordType.Definite},
			{"g", WordType.Combining},
			{"i", WordType.Interjection},
			{"m", WordType.Determiner},
			{"n", WordType.Noun},
			{"o", WordType.Pronoun},
			{"p", WordType.Plural},
			{"r", WordType.Preposition},
			{"v", WordType.Verb}
		};
	}
}
