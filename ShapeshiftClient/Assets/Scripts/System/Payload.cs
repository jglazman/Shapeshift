//
// Copyright (c) 2020 Jeremy Glazman
//

using System.Collections;
using System.Collections.Generic;

namespace Glazman.Shapeshift
{
	public class Payload
	{
		private Dictionary<int, object> items = new Dictionary<int, object>();

		public void SetField(int field, object value)
		{
			items[field] = value;
		}

		public int GetInt(int field)
		{
			if (items.TryGetValue(field, out var value))
				return (int)value;

			return 0;
		}
	}
}
