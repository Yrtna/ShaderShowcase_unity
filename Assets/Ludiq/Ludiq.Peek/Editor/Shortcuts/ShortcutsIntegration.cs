#define PEEK_PRIMARY_SHORTCUTS // Alt+[0-9]
#define PEEK_SECONDARY_SHORTCUTS // Alt+Shift+[0-9]

using UnityEditor;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public static class ShortcutsIntegration
	{
		public static ToolbarControl primaryToolbar { get; set; }

		public static ToolbarControl secondaryToolbar { get; set; }



		#if PEEK_PRIMARY_SHORTCUTS

		[MenuItem("Tools/Peek/Shortcuts/Primary 1 &1")]
		private static void PrimaryShortcut1()
		{
			primaryToolbar?.TriggerShortcut(1);
		}

		[MenuItem("Tools/Peek/Shortcuts/Primary 2 &2")]
		private static void PrimaryShortcut2()
		{
			primaryToolbar?.TriggerShortcut(2);
		}

		[MenuItem("Tools/Peek/Shortcuts/Primary 3 &3")]
		private static void PrimaryShortcut3()
		{
			primaryToolbar?.TriggerShortcut(3);
		}

		[MenuItem("Tools/Peek/Shortcuts/Primary 4 &4")]
		private static void PrimaryShortcut4()
		{
			primaryToolbar?.TriggerShortcut(4);
		}

		[MenuItem("Tools/Peek/Shortcuts/Primary 5 &5")]
		private static void PrimaryShortcut5()
		{
			primaryToolbar?.TriggerShortcut(5);
		}

		[MenuItem("Tools/Peek/Shortcuts/Primary 6 &6")]
		private static void PrimaryShortcut6()
		{
			primaryToolbar?.TriggerShortcut(6);
		}

		[MenuItem("Tools/Peek/Shortcuts/Primary 7 &7")]
		private static void PrimaryShortcut7()
		{
			primaryToolbar?.TriggerShortcut(7);
		}

		[MenuItem("Tools/Peek/Shortcuts/Primary 8 &8")]
		private static void PrimaryShortcut8()
		{
			primaryToolbar?.TriggerShortcut(8);
		}

		[MenuItem("Tools/Peek/Shortcuts/Primary 9 &9")]
		private static void PrimaryShortcut9()
		{
			primaryToolbar?.TriggerShortcut(9);
		}

		[MenuItem("Tools/Peek/Shortcuts/Primary 0 &0")]
		private static void PrimaryShortcut0()
		{
			primaryToolbar?.TriggerShortcut(0);
		}

		#endif



		#if PEEK_SECONDARY_SHORTCUTS

		[MenuItem("Tools/Peek/Shortcuts/Secondary 1 &#1")]
		private static void SecondaryShortcut1()
		{
			secondaryToolbar?.TriggerShortcut(1);
		}

		[MenuItem("Tools/Peek/Shortcuts/Secondary 2 &#2")]
		private static void SecondaryShortcut2()
		{
			secondaryToolbar?.TriggerShortcut(2);
		}

		[MenuItem("Tools/Peek/Shortcuts/Secondary 3 &#3")]
		private static void SecondaryShortcut3()
		{
			secondaryToolbar?.TriggerShortcut(3);
		}

		[MenuItem("Tools/Peek/Shortcuts/Secondary 4 &#4")]
		private static void SecondaryShortcut4()
		{
			secondaryToolbar?.TriggerShortcut(4);
		}

		[MenuItem("Tools/Peek/Shortcuts/Secondary 5 &#5")]
		private static void SecondaryShortcut5()
		{
			secondaryToolbar?.TriggerShortcut(5);
		}

		[MenuItem("Tools/Peek/Shortcuts/Secondary 6 &#6")]
		private static void SecondaryShortcut6()
		{
			secondaryToolbar?.TriggerShortcut(6);
		}

		[MenuItem("Tools/Peek/Shortcuts/Secondary 7 &#7")]
		private static void SecondaryShortcut7()
		{
			secondaryToolbar?.TriggerShortcut(7);
		}

		[MenuItem("Tools/Peek/Shortcuts/Secondary 8 &#8")]
		private static void SecondaryShortcut8()
		{
			secondaryToolbar?.TriggerShortcut(8);
		}

		[MenuItem("Tools/Peek/Shortcuts/Secondary 9 &#9")]
		private static void SecondaryShortcut9()
		{
			secondaryToolbar?.TriggerShortcut(9);
		}

		[MenuItem("Tools/Peek/Shortcuts/Secondary 0 &#0")]
		private static void SecondaryShortcut0()
		{
			secondaryToolbar?.TriggerShortcut(0);
		}

		#endif
	}
}