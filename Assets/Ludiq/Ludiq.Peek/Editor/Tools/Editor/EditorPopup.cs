using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ludiq.PeekCore.ReflectionMagic;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;
	using Editor = Editor;

	public sealed class EditorPopup : LudiqEditorWindow, IFollowingPopupWindow
	{
		public Rect activatorPosition { get; set; }

		private float autoHeight;
		
		private UnityObject[] initializationTargets;

		public Editor editor { get; private set; }

		private Editor importerTargetEditor;

		public bool isPopup { get; private set; }

		public bool isPinned => !isPopup;

		private bool shouldClose => editor == null || editor.targets.All(t => t == null);

		public static EditorPopup Open(UnityObject target, Rect activator)
		{
			return Open(target.Yield().ToArray(), activator);
		}

		public static EditorPopup Open(UnityObject[] targets, Rect activator)
		{
			Ensure.That(nameof(targets)).IsNotNull(targets);
			Ensure.That(nameof(targets)).HasNoNullItem(targets);

			if (!ArrayTypeUtility.TryGetCommonType(targets, out var commonType))
			{
				throw new InvalidOperationException("Cannot create editor for objects of mismatching types.");
			}

			var popup = CreateInstance<EditorPopup>();
			popup.isPopup = true;

			try
			{
				popup.Initialize(targets);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				popup.Close();
				return null;
			}

			popup.activatorPosition = activator;
			popup.position = Rect.zero;
			popup.isPopup = true;
			popup.ShowAsDropDown(popup.activatorPosition, new Vector2(PeekPlugin.Configuration.editorPopupWidth, 1));
			popup.minSize = new Vector2(200, 16);
			popup.maxSize = new Vector2(4000, 4000);

			popup.Focus();

			return popup;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			minSize = new Vector2(200, 16);
			maxSize = new Vector2(4000, 4000);
			autoRepaintOnSceneChange = true;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (editor != null)
			{
				DestroyImmediate(editor);
				editor = null;
			}

			if (importerTargetEditor != null)
			{
				DestroyImmediate(importerTargetEditor);
				importerTargetEditor = null;
			}
		}

		private void Initialize(UnityObject[] targets)
		{
			initializationTargets = targets;

			editor = Editor.CreateEditor(targets);

			AssetImporter importer = null;

			foreach (var target in targets)
			{
				var path = AssetDatabase.GetAssetPath(target);

				if (path == null || AssetDatabase.IsSubAsset(target))
				{
					continue;
				}

				importer = AssetImporter.GetAtPath(path);

				if (importer != null)
				{
					break;
				}
			}

			if (importer != null)
			{
				var importerEditor = Editor.CreateEditor(importer);

				if (importerEditor is AssetImporterEditor)
				{
					var importerEditorDynamic = importerEditor.AsDynamic();
					importerEditorDynamic.InternalSetAssetImporterTargetEditor(editor);
					importerTargetEditor = editor;
					editor = importerEditor;
				}
			}

			var dynamicEditor = editor.AsDynamic();
			dynamicEditor.firstInspectedEditor = true;

			if (targets.Length == 1)
			{
				var target = targets[0];
				titleContent.text = $"{target.name} ({target.GetType().DisplayName()})";
			}
			else
			{
				titleContent.text = "(Multiple)";
			}
		}

		protected override void Update()
		{
			base.Update();

			if (shouldClose)
			{
				Close();
				return;
			}

			if (isPopup)
			{
				// Here, we're trying to avoid having the popup show on the side of the activator (because that's annoying for toolbars)
				// The trick is to stretch the activator position to calculate the desired Y
				// But to keep the original activator position to calculate the desired X
				var activatorPositionBar = new Rect(activatorPosition);
				activatorPositionBar.xMin = 0;
				activatorPositionBar.xMax = 4000;
				var positionX = this.GetDropdownPositionCropped(activatorPosition, new Vector2(position.width, 1));
				var positionY =  this.GetDropdownPositionCropped(activatorPositionBar, new Vector2(position.width, autoHeight));

				position = new Rect
				(
					positionX.xMin,
					positionY.yMin,
					positionX.width,
					positionY.height
				);
			}
		}

		private void Pin()
		{
			var instance = CreateInstance<EditorPopup>();
			instance.Initialize(initializationTargets);
			instance.ShowUtility();
			instance.position = position;
			Close();
		}
		
		protected override void OnGUI()
		{
			base.OnGUI();

			if (shouldClose)
			{
				Close();
				return;
			}

			if (e.type == EventType.KeyDown &&
			    e.modifiers == EventModifiers.None &&
			    e.keyCode == KeyCode.Escape)
			{
				Close();
				return;
			}

			var position = this.position;
			position.x = position.y = 0;
			var height = 0f;
			var calculateHeight = isPopup;
			var applyHeight = calculateHeight && e.type == EventType.Repaint;

			var editable = editor.targets.None(t => t.HasHideFlag(HideFlags.NotEditable));

			EditorGUI.BeginChangeCheck();

			GUILayout.BeginVertical(Styles.background, GUILayout.ExpandHeight(true));

			var largeHeader = editor.targets.Any(t => !(t is Component));

			EditorGUIUtility.wideMode = position.width >= 330;

			EditorGUIUtility.hierarchyMode = false;

			GUILayout.BeginHorizontal();

			EditorGUI.BeginDisabledGroup(!editable);

			if (largeHeader)
			{
				GUILayout.BeginVertical();
				GUILayout.Space(0); // Fix to stick to top
				editor.DrawHeader();
				GUILayout.EndVertical();
			}
			else
			{
				GUILayout.BeginVertical(Styles.smallHeader);
				GUILayout.Space(EditorGUIUtility.isProSkin && !LudiqGUIUtility.isFlatSkin ? -3 : -4);
				GUILayout.BeginHorizontal();
				GUILayout.Space(-12);
				EditorGUILayout.InspectorTitlebar(true, editor.targets, false);
				GUILayout.EndHorizontal();

				if (LudiqGUIUtility.isFlatSkin)
				{
					GUILayout.Space(-5);
				}

				GUILayout.EndVertical();
			}
			
			GUILayout.BeginVertical(Styles.smallHeaderExtra, GUILayout.ExpandHeight(true));

			if (largeHeader)
			{
				GUILayout.Space(6);
			}
			else
			{
				if (LudiqGUIUtility.isFlatSkin)
				{
					GUILayout.Space(5);
				}
				else
				{
					if (EditorGUIUtility.isProSkin)
					{
						var topBorderRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(16), GUILayout.Height(1));
						topBorderRect.y += 1;
						EditorGUI.DrawRect(topBorderRect, Color.Lerp(ColorPalette.unityBackgroundLighter, ColorPalette.unityForegroundDim, 0.25f));
						GUILayout.Space(2);
					}
					else
					{
						GUILayout.Space(2);
					}
				}
			}
			
			EditorGUI.EndDisabledGroup();

			var wantsPin = GUILayout.Toggle(isPinned, GUIContent.none, PeekStyles.pinButton);
			
			EditorGUI.BeginDisabledGroup(!editable);

			if (isPinned != wantsPin)
			{
				if (wantsPin)
				{
					Pin();
				}
				else
				{
					Close();
				}

				GUIUtility.ExitGUI();
			}

			GUILayout.EndVertical();

			if (largeHeader)
			{
				GUILayout.Space(2);
			}

			GUILayout.EndHorizontal();

			if (editor.targets.Length == 1 && editor.targets[0] is GameObject go)
			{
				GUILayout.Space(-1);

				GUILayout.BeginHorizontal(Styles.prefabTools);

				if (PrefabUtility.IsPartOfAnyPrefab(go))
				{
					GUILayout.Label("Unpack", GUILayout.ExpandWidth(false));

					if (GUILayout.Button("Root", EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true)))
					{
						PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
					}

					if (GUILayout.Button("Completely", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true)))
					{
						PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.UserAction);
					}

					GUILayout.Space(2);
					GUILayout.Label("Create", GUILayout.ExpandWidth(false));
					
					if (GUILayout.Button("Prefab", EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(true)))
					{
						GameObjectOperations.CreateOriginalPrefab(go);
					}

					if (GUILayout.Button("Variant", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(true)))
					{
						GameObjectOperations.CreatePrefabVariant(go);
					}
				}
				else
				{
					GUILayout.Space(LudiqGUIUtility.isFlatSkin ? 4 : 9);
					GUILayout.Label("Prefab", GUILayout.ExpandWidth(false));

					if (GUILayout.Button("Create Prefab", EditorStyles.miniButton))
					{
						GameObjectOperations.CreatePrefab(go);
					}
				}

				GUILayout.Space(14);

				GUILayout.EndHorizontal();

				if (LudiqGUIUtility.isFlatSkin)
				{
					GUILayout.Space(-1);
				}
			}

			if (calculateHeight)
			{
				height += GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(0)).yMin;
			}
			
			EditorGUI.EndDisabledGroup();

			scroll = EditorGUILayout.BeginScrollView(scroll);

			if (!largeHeader)
			{
				GUILayout.Space(3);
			}

			GUILayout.BeginVertical(Styles.inspectorBackground);
			EditorGUIUtility.hierarchyMode = true;
			EditorGUI.BeginDisabledGroup(!editable);
			editor.OnInspectorGUI();
			EditorGUI.EndDisabledGroup();
			GUILayout.EndVertical();


			if (isPinned)
			{
				GUILayout.FlexibleSpace();
			}

			if (HasPreview())
			{
				var previewArea = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(120));
				editor.DrawPreview(previewArea);
			}
			else
			{
				GUILayout.Space(2);
			}

			if (calculateHeight)
			{
				height += GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(0)).yMin;
			}

			GUILayout.EndVertical();

			GUILayout.EndScrollView();

			if (applyHeight)
			{
				autoHeight = height;
			}

			if (EditorGUI.EndChangeCheck()) { }

			if (e.type == EventType.KeyDown &&
			    e.keyCode == KeyCode.Space)
			{
				if (isPopup)
				{
					Pin();
				}
				else
				{
					Close();
				}

				e.Use();
				return;
			}

			if (e.type == EventType.Repaint)
			{
				LudiqGUI.DrawEmptyRect(new Rect(Vector2.zero, this.position.size), ColorPalette.unityBackgroundVeryDark);
			}

			if (editor.RequiresConstantRepaint())
			{
				Repaint();
			}
		}

		private bool HasPreview()
		{
			return editor.HasPreviewGUI() || PreviewUtility.HasPreview(editor.target);
		}

		private static class Styles
		{
			static Styles()
			{
				background = ColorPalette.unityBackgroundMid.CreateBackground();
				background.padding = new RectOffset(0, 0, 0, 0);
				background.margin = new RectOffset(0, 0, 0, 0);

				inspectorBackground = new GUIStyle();
				inspectorBackground.padding = new RectOffset(0, 0, 0, 0);
				inspectorBackground.margin = new RectOffset(16, 0, 0, 0);
				inspectorBackground.fixedHeight = 0;

				smallHeader = new GUIStyle(LudiqStyles.headerBackground);
				smallHeader.padding = new RectOffset(0, 0, 4, 4);
				smallHeader.margin = new RectOffset(0, 0, 0, 0);

				smallHeaderExtra = new GUIStyle(smallHeader);
				smallHeaderExtra.padding = new RectOffset(0, 0, 0, 0);
				smallHeaderExtra.margin = new RectOffset(0, 0, 0, 0);

				prefabTools = new GUIStyle(LudiqStyles.headerBackground);
				prefabTools.padding = new RectOffset(8, 8, 4, 4);
				prefabTools.margin = new RectOffset(0, 0, 0, 0);
			}

			public static readonly GUIStyle background;

			public static readonly GUIStyle inspectorBackground;

			public static readonly GUIStyle smallHeader;

			public static readonly GUIStyle smallHeaderExtra;

			public static readonly GUIStyle prefabTools;
		}
	}
}