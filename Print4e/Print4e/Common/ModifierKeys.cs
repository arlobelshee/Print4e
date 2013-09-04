// ModifierKeys.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System.Diagnostics;
using JetBrains.Annotations;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Print4e.Common
{
	public class ModifierKeys
	{
		public ModifierKeys(bool menuKey, bool controlKey, bool shiftKey)
		{
			Alt = menuKey;
			Control = controlKey;
			Shift = shiftKey;
		}

		public bool Alt { get; private set; }
		public bool Control { get; private set; }
		public bool Shift { get; private set; }
		public bool OnlyAlt { get { return Alt && !Control && !Shift; } }
		public bool OnlyControl { get { return !Alt && Control && !Shift; } }
		public bool OnlyShift { get { return !Alt && !Control && Shift; } }
		public bool None { get { return !Alt && !Control && !Shift; } }

		[NotNull]
		public static ModifierKeys Current()
		{
			Debug.Assert(Window.Current != null, "Window.Current != null");
			Debug.Assert(Window.Current.CoreWindow != null, "Window.Current.CoreWindow != null");
			var coreWindow = Window.Current.CoreWindow;

			var menuKey = KeyIsDown(coreWindow, VirtualKey.Menu);
			var controlKey = KeyIsDown(coreWindow, VirtualKey.Control);
			var shiftKey = KeyIsDown(coreWindow, VirtualKey.Shift);
			return new ModifierKeys(menuKey, controlKey, shiftKey);
		}

		private static bool KeyIsDown([NotNull] CoreWindow coreWindow, VirtualKey key)
		{
			return (coreWindow.GetKeyState(key) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
		}
	}
}
