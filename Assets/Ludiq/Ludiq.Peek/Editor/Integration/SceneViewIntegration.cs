using Ludiq.Peek;
using Ludiq.PeekCore;
using UnityEditor;
using UnityObject = UnityEngine.Object;

[assembly: InitializeAfterPlugins(typeof(SceneViewIntegration))]

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public static class SceneViewIntegration
	{
		static SceneViewIntegration()
		{
#if UNITY_2019_1_OR_NEWER
			SceneView.duringSceneGui += OnSceneGUI;
#else
			SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
		}

		public static bool used { get; private set; }

		public static void Use()
		{
			used = true;
		}

		private static void OnSceneGUI(SceneView sceneView)
		{
			GuiCallback.Process();

			used = false;

			Tabs.OnSceneGUI(sceneView);

			SceneToolbars.OnSceneGUI(sceneView);
			
			Probe.OnSceneGUI(sceneView);

			Creator.OnSceneGUI(sceneView);
			
			SceneMaximizerIntegration.OnSceneGUI(sceneView);
			
			SceneDeselectIntegration.OnSceneGUI(sceneView);

			SceneHierarchyIntegration.OnSceneGUI(sceneView);
		}
	}
}