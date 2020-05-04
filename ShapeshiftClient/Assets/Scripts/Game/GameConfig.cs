//
// Copyright (c) 2020 Jeremy Glazman
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Glazman.Shapeshift
{
	public struct MatchRulesConfig
	{
		public enum Headers
		{
			ID = 0,
			Category,
			IsDefault,
			MinSelection,
			MaxSelection,
			WordCheck
		}

		public const string ResourceName = "MatchRules";

		public string ID;
		public string Category;
		public bool IsDefault;
		public int MinSelection;
		public int MaxSelection;
		public bool WordCheck;
	}
	
	public struct GridNodeConfig
	{
		public enum Headers
		{
			ID = 0,
			ImageName,
			IsOpen
		}
		
		public const string ResourceName = "GridNode";
		
		public string ID;
		public string ImageName;
		public bool IsOpen;
	}

	public enum GridItemMatchType
	{
		None = 0,
		Exact,
		Category
	}
	
	public struct GridItemConfig
	{
		public enum Headers
		{
			ID = 0,
			ImageName,
			Category,
			MatchType,
			MatchIndex,
			DropFrequency
		}
		
		public const string ResourceName = "GridItem";
		
		public string ID;
		public string ImageName;
		public string Category;
		public GridItemMatchType MatchType;
		public int MatchIndex;
		public int DropFrequency;
	}

	public class GridItemDropDistribution
	{
		private string _category = null;
		private List<string> _itemIds = new List<string>();		// the items in the bag
		private Queue<int> _shuffled = new Queue<int>();		// shuffled indices to pull from the bag next

		public GridItemDropDistribution(string category, IEnumerable<GridItemConfig> gridItems)
		{
			_category = category;
			
			foreach (var item in gridItems)
			{
				int freq = item.DropFrequency;
				for (int i = 0; i < freq; i++)
					_itemIds.Add(item.ID);
			}
		}
		
		public string Next()
		{
			Assert.IsTrue(_itemIds.Count > 0, $"Tried to get random item from empty distribution: '{_category}'");
			
			if (_shuffled.Count == 0)
				Shuffle();

			var nextIndex = _shuffled.Dequeue();
			Assert.IsTrue(nextIndex >= 0 && nextIndex < _itemIds.Count, $"Generated invalid index for distribution: '{_category}'");

			return _itemIds[nextIndex];
		}

		private void Shuffle()
		{
			_shuffled.Clear();

			int[] indices = new int[_itemIds.Count];
			for (int i = 0; i < _itemIds.Count; i++)
				indices[i] = i;

			for (int i = 0; i < _itemIds.Count; i++)
			{
				int randomIndex = Random.Range(0, _itemIds.Count);
				int temp = indices[i];
				indices[i] = indices[randomIndex];
				indices[randomIndex] = temp;
			}
			
			for (int i = 0; i < _itemIds.Count; i++)
				_shuffled.Enqueue(indices[i]);
		}
	}

	public static class GameConfig
	{
		private static Dictionary<string,MatchRulesConfig> MatchRules;	// key = ID
		private static Dictionary<string,GridNodeConfig> GridNodes;	// key = ID
		private static Dictionary<string,GridItemConfig> GridItems; // key = ID

		public static IEnumerable<MatchRulesConfig> AllMatchRules => MatchRules.Values;
		public static IEnumerable<GridNodeConfig> AllGridNodes => GridNodes.Values;
		public static IEnumerable<GridItemConfig> AllGridItems => GridItems.Values;


		public static MatchRulesConfig GetMatchRules(string id)
		{
			Assert.IsTrue(MatchRules.ContainsKey(id), $"Unknown MatchRules ID: '{id}'");
			return MatchRules[id];
		}
		
		public static GridNodeConfig GetGridNode(string id)
		{
			Assert.IsTrue(GridNodes.ContainsKey(id), $"Unknown GridNode ID: '{id}'");
			return GridNodes[id];
		}

		public static GridItemConfig GetGridItem(string id)
		{
			Assert.IsTrue(GridItems.ContainsKey(id), $"Unknown GridItem ID: '{id}'");
			return GridItems[id];
		}

		public static IEnumerable<string> GetAllGridItemCategories()
		{
			return AllGridItems.Select(item => item.Category).Distinct();
		}

		public static IEnumerable<GridItemConfig> GetAllGridItemsInCategory(string category, string[] exludeItems=null)
		{
			return GridItems.Values.Where(item => item.Category == category && (exludeItems == null || !exludeItems.Contains(item.ID)));
		}

		public static GridItemConfig GetDefaultLayoutGridItem(string category)
		{
			var allCategoryItems = GameConfig.AllGridItems.Where(item => item.Category == category).ToList();
			var minIndex = allCategoryItems.Select(item => item.MatchIndex).Min();
			return allCategoryItems.First(item => item.MatchIndex == minIndex);
		}

		public static void Initialize()
		{
			MatchRules = new Dictionary<string, MatchRulesConfig>();
			var matchRulesConfig = Resources.Load<TextAsset>(MatchRulesConfig.ResourceName);
			Assert.IsNotNull(matchRulesConfig, $"Missing data resource file: {MatchRulesConfig.ResourceName}");
			var matchRulesLines = matchRulesConfig.text.Split('\n');
			try
			{
				for (int i = 0; i < matchRulesLines.Length; i++)
				{
					var config = matchRulesLines[i].TrimEnd().Split(',');	// trim endline garbage
					if (i == 0)
					{
						var headers = Enum.GetNames(typeof(MatchRulesConfig.Headers));
						Assert.IsTrue(config.Length == headers.Length, "Incompatible MatchRulesConfig data: headers mismatch");
						for (int n = 0; n < config.Length; n++)
							Assert.IsTrue(config[n] == headers[n], $"Incompatible MatchRulesConfig data: unknown header '{config[n]}', expected '{headers[n]}'");
					}
					else
					{
						var matchRules = new MatchRulesConfig()
						{
							ID = config[(int)MatchRulesConfig.Headers.ID],
							Category = config[(int)MatchRulesConfig.Headers.Category],
							IsDefault = bool.Parse(config[(int)MatchRulesConfig.Headers.IsDefault]),
							MinSelection = int.Parse(config[(int)MatchRulesConfig.Headers.MinSelection]),
							MaxSelection = int.Parse(config[(int)MatchRulesConfig.Headers.MaxSelection]),
							WordCheck = bool.Parse(config[(int)MatchRulesConfig.Headers.WordCheck])
						};
						Assert.IsTrue(!MatchRules.ContainsKey(matchRules.ID), $"[MatchRulesConfig] IDs must be unique: {matchRules.ID}");
						MatchRules[matchRules.ID] = matchRules;
					}
				}
			}
			catch (System.Exception e)
			{
				Logger.LogError($"Failed to load MatchRulesConfig: {e.Message}");
			}
			
			GridNodes = new Dictionary<string, GridNodeConfig>();
			var gridNodeConfig = Resources.Load<TextAsset>(GridNodeConfig.ResourceName);
			Assert.IsNotNull(gridNodeConfig, $"Missing data resource file: {GridNodeConfig.ResourceName}");
			var gridNodeLines = gridNodeConfig.text.Split('\n');
			try
			{
				for (int i = 0; i < gridNodeLines.Length; i++)
				{
					var config = gridNodeLines[i].TrimEnd().Split(',');	// trim endline garbage
					if (i == 0)
					{
						var headers = Enum.GetNames(typeof(GridNodeConfig.Headers));
						Assert.IsTrue(config.Length == headers.Length, "Incompatible GridNodeConfig data: headers mismatch");
						for (int n = 0; n < config.Length; n++)
							Assert.IsTrue(config[n] == headers[n], $"Incompatible GridNodeConfig data: unknown header '{config[n]}', expected '{headers[n]}'");
					}
					else
					{
						var gridNode = new GridNodeConfig()
						{
							ID = config[(int)GridNodeConfig.Headers.ID],
							ImageName = config[(int)GridNodeConfig.Headers.ImageName],
							IsOpen = bool.Parse(config[(int)GridNodeConfig.Headers.IsOpen])
						};
						Assert.IsTrue(!GridNodes.ContainsKey(gridNode.ID), $"[GridNodeConfig] IDs must be unique: {gridNode.ID}");
						GridNodes[gridNode.ID] = gridNode;
					}
				}
			}
			catch (System.Exception e)
			{
				Logger.LogError($"Failed to load GridNodeConfig: {e.Message}");
			}
			
			GridItems = new Dictionary<string, GridItemConfig>();
			var gridItemConfig = Resources.Load<TextAsset>(GridItemConfig.ResourceName);
			Assert.IsNotNull(gridItemConfig, $"Missing data resource file: {GridItemConfig.ResourceName}");
			var gridItemLines = gridItemConfig.text.Split('\n');
			try
			{
				for (int i = 0; i < gridItemLines.Length; i++)
				{
					var config = gridItemLines[i].TrimEnd().Split(',');	// trim endline garbage
					if (i == 0)
					{
						var headers = Enum.GetNames(typeof(GridItemConfig.Headers));
						Assert.IsTrue(config.Length == headers.Length, "Incompatible GridItemConfig data: headers mismatch");
						for (int n = 0; n < config.Length; n++)
							Assert.IsTrue(config[n] == headers[n], $"Incompatible GridItemConfig data: unknown header '{config[n]}', expected '{headers[n]}'");
					}
					else
					{
						var gridItem = new GridItemConfig()
						{
							ID = config[(int)GridItemConfig.Headers.ID],
							ImageName = config[(int)GridItemConfig.Headers.ImageName],
							Category = config[(int)GridItemConfig.Headers.Category],
							MatchType = (GridItemMatchType)Enum.Parse(typeof(GridItemMatchType), config[(int)GridItemConfig.Headers.MatchType]),
							MatchIndex = int.Parse(config[(int)GridItemConfig.Headers.MatchIndex]),
							DropFrequency = int.Parse(config[(int)GridItemConfig.Headers.DropFrequency])
						};
						Assert.IsTrue(!GridItems.ContainsKey(gridItem.ID), $"[GridItemConfig] IDs must be unique: {gridItem.ID}");
						GridItems[gridItem.ID] = gridItem;
					}
				}
			}
			catch (System.Exception e)
			{
				Logger.LogError($"Failed to load GridItemConfig: {e.Message}");
			}
		}
	}
}
