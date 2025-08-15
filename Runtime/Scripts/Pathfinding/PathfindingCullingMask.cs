using NUnit.Framework.Constraints;
using System;
using UnityEngine;

namespace DT.GridSystem.Pathfinding
{

	[Serializable]
	public struct PathfindingCullingMask
	{
		[SerializeField] private int mask;
		public readonly int MaskValue => mask;

		public static readonly PathfindingCullingMask AllMask = new(~0);

		public PathfindingCullingMask(int mask)
		{
			this.mask = mask;
		}
		public PathfindingCullingMask(PathfindingLayer layer)
		{
			this.mask = 1 << NameToLayerIndex(layer.LayerName);
		}


		public PathfindingCullingMask AddMask(int bit)
		{
			mask |= bit;
			return this;
		}
		public PathfindingCullingMask AddMask(PathfindingLayer Layer)
		{
			mask |= 1 << NameToLayerIndex(Layer.LayerName);

			return this;
		}

		/// <summary>
		/// Creates a mask from given layer names.
		/// </summary>
		public static PathfindingCullingMask FromLayerNames(params string[] layers)
		{
			int m = 0;
			foreach (string layer in layers)
			{
				if (!string.IsNullOrEmpty(layer))
				{
					int index = NameToLayerIndex(layer);
					if (index >= 0)
					{
						m |= (1 << index);
					}
					else
					{
						Debug.LogWarning($"Pathfinding layer \"{layer}\" not found in project settings.");
					}
				}
			}
			return new PathfindingCullingMask(m);
		}

		private static int NameToLayerIndex(string layerName)
		{
			return PathfindingLayerData.Instance.GetCustomLayerIndex(layerName);
		}
		public static implicit operator PathfindingCullingMask(PathfindingLayer layer)
		{
			return new PathfindingCullingMask(layer);
		}

		public static bool operator ==(PathfindingCullingMask obj1, PathfindingCullingMask obj2)
		{
			return obj1.mask == obj2.mask;
		}

		public static bool operator !=(PathfindingCullingMask obj1, PathfindingCullingMask obj2)
		{
			return !(obj1.mask == obj2.mask);
		}

		// Fixed Equals method
		public override bool Equals(object obj)
		{
			return obj is PathfindingCullingMask other && other.mask == mask;
		}

		public override int GetHashCode()
		{
			return mask.GetHashCode();
		}
		public readonly bool Contains(PathfindingCullingMask other)
		{
			return (mask & other.mask) != 0;
		}
	}
}
