using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public static class Replacer
	{
		public static FuzzyWindow Open(GameObject[] targets, Rect activatorPosition)
		{
			if (!TransformOperations.WarnRestructurable(targets.Select(go => go.transform).ToArray()))
			{
				return null;
			}

			// GameObject menu creators change the selection, so we need to cache it
			var selectionSnapshot = Selection.objects;

			LudiqGUI.FuzzyDropdown
			(
				activatorPosition,
				new CreateGameObjectOptionTree("Replace with..."),
				null,
				(_instance) =>
				{
					var template = (GameObject)_instance;

					var allSelected = new HashSet<GameObject>();

					foreach (var target in targets)
					{
						var selected = selectionSnapshot.Contains(target);
						var position = target.transform.position;
						var rotation = target.transform.rotation;
						var scale = target.transform.localScale;
						var parent = target.transform.parent;

						Undo.DestroyObjectImmediate(target);
						var replacement = DuplicateGameObject(template);

						replacement.transform.position = position;
						replacement.transform.rotation = rotation;
						//replacement.transform.localScale = scale;
						replacement.transform.SetParent(parent, true);

						if (selected)
						{
							allSelected.Add(replacement);
						}
					}

					Selection.objects = allSelected.ToArray();

					UnityObject.DestroyImmediate(template);
				}
			);

			return FuzzyWindow.instance;
		}

		private static GameObject DuplicateGameObject(GameObject go)
		{
			UnityObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(go);

			GameObject result;

			if (prefabRoot != null)
			{
				result = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
			}
			else
			{
				result = (GameObject)UnityObject.Instantiate(go);
				result.name = go.name;
			}

			Undo.RegisterCreatedObjectUndo(result, "Duplicate " + result.name);

			return result;
		}
	}
}