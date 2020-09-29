using System;
using System.IO;
using System.Linq;
using Ludiq.OdinSerializer;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UEditor = UnityEditor.Editor;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public class CreateGameObjectOption : FuzzyOption<LudiqGUI.PopupFunc>
	{
		private static UnityObject editorTarget;

		private static UEditor editor;

		public string primitivePath { get; private set; }

		public string primitiveFolder => PathUtility.NaiveParent(primitivePath);

		public AssetDatabaseEntry assetDatabaseEntry { get; private set; }

		public string assetFolder => PathUtility.NaiveNormalize(Path.GetDirectoryName(assetDatabaseEntry.path));

		public bool assetIsLoaded { get; private set; }

		private UnityObject _asset;

		public UnityObject asset
		{
			get
			{
				if (!assetIsLoaded)
				{
					_asset = UnityAPI.Await(LoadAsset);
					assetIsLoaded = true;
				}

				return _asset;
			}
		}

		public GameObject prefab => asset as GameObject;

		public Sprite sprite => asset as Sprite;

		public bool isAsset => assetDatabaseEntry != null;

		public bool isPrimitive => primitivePath != null;
		
		private CreateGameObjectOption() : base(FuzzyOptionMode.Leaf) { }
		
		public static CreateGameObjectOption Primitive(string primitivePath)
		{
			var option = new CreateGameObjectOption();
			option.primitivePath = primitivePath;
			option.label = primitivePath.PartAfterLast(EditorMainMenu.Separator).TrimStart("Create ");
			option.value = option.CreatePrimitive;
			option.getIcon = () => PeekPlugin.Icons.createGameObject;
			return option;
		}

		private static CreateGameObjectOption Asset(AssetDatabaseEntry assetDatabaseEntry)
		{
			var option = new CreateGameObjectOption();
			option.assetDatabaseEntry = assetDatabaseEntry;
			option.label = assetDatabaseEntry.name;
			option.getIcon = option.GetAssetIcon;
			return option;
		}

		public static CreateGameObjectOption Prefab(AssetDatabaseEntry assetDatabaseEntry)
		{
			var option = Asset(assetDatabaseEntry);
			option.value = option.InstantiatePrefab;
			return option;
		}

		public static CreateGameObjectOption Model(AssetDatabaseEntry assetDatabaseEntry)
		{
			// Models are still prefabs as far as Unity is concerned
			return Prefab(assetDatabaseEntry);
		}

		public static CreateGameObjectOption Sprite(AssetDatabaseEntry assetDatabaseEntry)
		{
			var option = Asset(assetDatabaseEntry);
			option.value = option.InstantiateSprite;
			return option;
		}

		private UnityObject LoadAsset()
		{
			if (assetDatabaseEntry == null)
			{
				throw new InvalidOperationException();
			}
			
			// Make sure the object is loaded into memory; not sure if this is guaranteed by instanceID existence
			AssetDatabase.LoadMainAssetAtPath(assetDatabaseEntry.path);
			AssetDatabase.LoadAllAssetRepresentationsAtPath(assetDatabaseEntry.path);
			
			var result = EditorUtility.InstanceIDToObject(assetDatabaseEntry.instanceID);

			if (result == null)
			{
				Debug.LogWarning($"Failed to lazy load asset for creator:\n{assetDatabaseEntry.path}::{assetDatabaseEntry.instanceID}");
			}

			return result;
		}

		private EditorTexture GetAssetIcon()
		{
			if (PeekPlugin.Configuration.enablePreviewIcons && 
			    PreviewUtility.TryGetPreview(asset, out var preview) && 
			    !AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
			{
				return EditorTexture.Single(preview);
			}
			else
			{
				return asset.Icon();
			}
		}

		private bool InstantiatePrefab(out object value)
		{
			var instance = PrefabUtility.InstantiatePrefab(prefab);
			Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
			value = instance;
			return true;
		}

		private bool InstantiateSprite(out object value)
		{
			var instance = new GameObject(sprite.name);
			var renderer = instance.AddComponent<SpriteRenderer>();
			renderer.sprite = sprite;
			Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
			value = instance;
			return true;
		}

		private bool CreatePrimitive(out object value)
		{
			var objectsBefore = UnityObject.FindObjectsOfType(typeof(GameObject));
			EditorApplication.ExecuteMenuItem(primitivePath);
			var objectsAfter = UnityObject.FindObjectsOfType(typeof(GameObject));
			var createdPrimitives = objectsAfter.Except(objectsBefore).Cast<GameObject>().ToArray();

			GameObject topmostPrimitive = null;

			foreach (var createPrimitive in createdPrimitives)
			{
				var isTopmost = true;

				foreach (var otherCreatedObject in createdPrimitives)
				{
					if (createPrimitive == otherCreatedObject)
					{
						continue;
					}

					if (createPrimitive.transform.IsChildOf(otherCreatedObject.transform))
					{
						isTopmost = false;
						break;
					}
				}

				if (isTopmost)
				{
					topmostPrimitive = createPrimitive;
				}
			}

			value = topmostPrimitive;

			return topmostPrimitive != null;
		}

		public override bool hasFooter => isAsset && prefab != null && prefab.GetComponentInChildren<Renderer>() != null;

		public override float GetFooterHeight(FuzzyOptionNode node, float width)
		{
			return 128;
		}

		public override void OnFooterGUI(FuzzyOptionNode node, Rect position)
		{
			if (editorTarget != prefab)
			{
				editorTarget = prefab;
				UEditor.CreateCachedEditor(editorTarget, null, ref editor);
			}

			if (editor != null)
			{
				editor.DrawPreview(position);
			}
		}

		public override string SearchResultLabel(string query)
		{
			var label = base.SearchResultLabel(query);

			if (isPrimitive)
			{
				label += LudiqGUIUtility.DimString($" (in {primitiveFolder})");
			}
			else if (isAsset)
			{
				label += LudiqGUIUtility.DimString($" (in {assetFolder})");
			}

			return label;
		}
	}
}