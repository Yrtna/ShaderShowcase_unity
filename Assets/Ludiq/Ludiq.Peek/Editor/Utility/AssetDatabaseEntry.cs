using UnityObject = UnityEngine.Object;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public sealed class AssetDatabaseEntry
	{
		public int instanceID;

		public string guid;

		public string name;

		public string path;

		public bool isMainAsset;

		public bool isFolder;

		public bool isSubAsset => !isMainAsset;
	}
}