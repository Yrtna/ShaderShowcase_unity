using System;
using Ludiq.Peek;
using Ludiq.PeekCore;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityObject = UnityEngine.Object;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public static class Creator
	{
		private static Event e => Event.current;

		internal static void OnSceneGUI(SceneView sceneView)
		{
			if (!PeekPlugin.Configuration.enableCreator.Display(sceneView.maximized))
			{
				return;
			}

			if (SceneViewIntegration.used)
			{
				return;
			}

			Profiler.BeginSample("Peek." + nameof(Creator));

			try
			{
				var position = sceneView.GetInnerGuiPosition(); 

				Handles.BeginGUI();
				
				if (position.Contains(e.mousePosition) && e.CtrlOrCmd() && !e.alt && e.shift)
				{
					var filter = ProbeFilter.@default;
					filter.proBuilder = false; // Too slow and useless here anyway
					var hit = Probe.Pick(filter, sceneView, e.mousePosition, out var point);

					var createIndicatorStyle = LudiqStyles.CommandButton(true, true);
					var createIndicatorContent = LudiqGUIUtility.TempContent(PeekPlugin.Icons.createGameObjectOptions?[IconSize.Small]);
					var createIndicatorSize = createIndicatorStyle.CalcSize(createIndicatorContent);

					var createIndicatorPosition = new Rect
					(
						e.mousePosition.x - (createIndicatorSize.x / 2),
						e.mousePosition.y + Styles.indicatorMargin,
						createIndicatorSize.x,
						createIndicatorSize.y
					);
					
					GUI.Label
					(
						createIndicatorPosition,
						createIndicatorContent,
						createIndicatorStyle
					);

					if (e.type == EventType.MouseDown && e.button == (int)MouseButton.Left)
					{
						var activatorPosition = new Rect(e.mousePosition, Vector2.zero);
						activatorPosition.width = 220;
						activatorPosition = LudiqGUIUtility.GUIToScreenRect(activatorPosition);

						// Delay closure allocations
						var _hit = hit;
						var _point = point;
						var _sceneView = sceneView;

						LudiqGUI.FuzzyDropdown
						(
							activatorPosition,
							new CreateGameObjectOptionTree(),
							null,
							(_instance) =>
							{
								var instance = (GameObject)_instance;

								var is2D = instance.GetComponent<RectTransform>() != null ||
								           instance.GetComponent<SpriteRenderer>() != null;
								
								if (_hit != null)
								{
									instance.transform.SetParent(_hit.Value.transform.parent, true);
								}

								instance.transform.position = _point;

								if (!is2D && PeekPlugin.Configuration.createOnBounds && instance.CalculateBounds(out var bounds, Space.World, true, false, false, false, false))
								{
									var difference = _point.y - bounds.min.y;

									instance.transform.position += difference * Vector3.up;
								}

								Selection.activeGameObject = instance;

								if (_hit == null && !_sceneView.in2DMode)
								{
									_sceneView.FrameSelected();
								}
							}
						);

						FuzzyWindow.instance.Focus();

						e.Use();
					}

					#if LUDIQ_PEEK_INTEROP_PROBUILDER
					UnityEditor.ProBuilder.EditorUtility.SynchronizeWithMeshFilter(null);
					#endif

					Handles.EndGUI();

					// Scale handles take depth into account for handle size, so they're more expressive than position handles

					if (sceneView.in2DMode)
					{
						Handles.PositionHandle(point, Quaternion.identity);
					}
					else
					{
						Handles.ScaleHandle(Vector3.one, point, Quaternion.identity, PeekPlugin.Configuration.creatorUnitSize);
					}

					Handles.BeginGUI();
				}

				if (position.Contains(e.mousePosition))
				{
					sceneView.Repaint();
				}

				Handles.EndGUI();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}

			Profiler.EndSample();
		}

		private static class Styles
		{
			static Styles() { }
			
			public static readonly int indicatorMargin = 20;
		}
	}
}