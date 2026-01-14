using UnityEngine;
using System.Collections.Generic;

namespace ShiftStoneRandomizer
{
	public static class Extensions
	{
		public static Transform DeepFind(this Transform parent, string name)
		{
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(parent);

			while (queue.Count > 0)
			{
				Transform current = queue.Dequeue();
				if (current.name == name) return current;

				for (int i = 0; i < current.childCount; i++)
				{
					queue.Enqueue(current.GetChild(i));
				}
			}
			return null;
		}
	}
}