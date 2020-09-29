#if PROBUILDER_4_OR_NEWER
using System;
using System.Reflection;
using Ludiq.PeekCore.ReflectionMagic;
using UnityEditor.ProBuilder;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public static class UnityEditorProBuilderDynamic
	{
		public static readonly Assembly UnityEditorProBuilderAssembly;

		public static readonly dynamic IconUtility;
		
		public static readonly Type IconSkinType;

		public static readonly dynamic EditorMeshHandles;

		static UnityEditorProBuilderDynamic()
		{
			UnityEditorProBuilderAssembly = typeof(ProBuilderEditor).Assembly;

			IconUtility = UnityEditorProBuilderAssembly.GetType("UnityEditor.ProBuilder.IconUtility", true).AsDynamicType();
			IconSkinType = UnityEditorProBuilderAssembly.GetType("UnityEditor.ProBuilder.IconSkin", true);
			EditorMeshHandles = UnityEditorProBuilderAssembly.GetType("UnityEditor.ProBuilder.EditorMeshHandles", true).AsDynamicType();
		}
	}
}
#endif