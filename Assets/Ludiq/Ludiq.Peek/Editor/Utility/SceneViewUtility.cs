using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityObject = UnityEngine.Object;

namespace Ludiq.Peek
{
    using System.Collections.Generic;
    // ReSharper disable once RedundantUsingDirective
    using PeekCore;

	public static class SceneViewUtility
	{
		private static Vector3 WorldToGUIPoint(this SceneView sceneView, Vector3 world)
		{
			// Does the same as this, but reimplemented to also work outside Handles.GUI scope
			// return HandleUtility.WorldToGUIPoint(worldPoint);

			world = Handles.matrix.MultiplyPoint(world);
			var screen = sceneView.camera.WorldToScreenPoint(world);
			var points = EditorGUIUtility.PixelsToPoints(screen);
			points.y = sceneView.GetInnerGuiPosition().height - points.y;
			return new Vector3(points.x, points.y, screen.z);
		}

		public static Rect? BoundsToGUIRect(this SceneView sceneView, Bounds worldBounds)
		{
			var worldPoints = ArrayPool<Vector3>.New(8);
			worldBounds.GetPointsNoAlloc(worldPoints);

			Rect? guiRect = null;

			foreach (var worldPoint in worldPoints)
			{
				var guiPoint = sceneView.WorldToGUIPoint(worldPoint);

				if (guiPoint.z < 0)
				{
					continue;
				}

				guiRect = guiRect.Encompass(guiPoint);
			}

			worldPoints.Free();

			return guiRect;
		}


		public static void CalculateGuiBounds(this SceneView sceneView, GameObject[] targets, out Rect? guiBounds, out Vector2? guiCenter)
		{
			guiBounds = null;
			guiCenter = null;

			foreach (var target in targets)
			{
				if (target.CalculateBounds(out var worldBounds, Space.World, true, true, true, true, false))
				{
					var targetGuiBounds = sceneView.BoundsToGUIRect(worldBounds);
					guiBounds = guiBounds.Encompass(targetGuiBounds);
				}
			}

			var worldCenter = Vector3.zero;

			foreach (var target in targets)
			{
				worldCenter += target.transform.position;
			}

			worldCenter /= targets.Length;
			
			var _guiCenter = sceneView.WorldToGUIPoint(worldCenter);

			if (_guiCenter.z > 0)
			{
				guiCenter = _guiCenter;
			}
		}
		
		public static Rect GetInnerGuiPosition(this SceneView sceneView)
		{
			var position = sceneView.position;
			position.x = position.y = 0;
			position.height -= EditorStyles.toolbar.fixedHeight;
			return position;
		}

		public static bool? IsWithinView(this SceneView sceneView, Rect? guiBounds, Vector2? guiCenter)
		{
			var innerPosition = GetInnerGuiPosition(sceneView);

			if (guiBounds != null)
			{
				return innerPosition.Overlaps(guiBounds.Value);
			}
			else if (guiCenter != null)
			{
				return innerPosition.Contains(guiCenter.Value);
			}

			return null;
		}

		public static void FrameSelectedIfOutOfView(this SceneView sceneView)
		{
			CalculateGuiBounds(sceneView, Selection.transforms.Select(t => t.gameObject).ToArray(), out var guiBounds, out var guiCenter);
			
			if (!(IsWithinView(sceneView, guiBounds, guiCenter) ?? true))
			{
				sceneView.FrameSelected();
			}
		}
	}
}