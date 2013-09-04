// LayoutUpdateMonitor.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Print4e.Common
{
	public class LayoutUpdateMonitor
	{
		[NotNull] private readonly List<Control> _layoutAwareControls = new List<Control>();
		[NotNull] private readonly LayoutAwarePage _page;
		[NotNull] private readonly Window _windowToWatch;

		public LayoutUpdateMonitor([NotNull] LayoutAwarePage page)
		{
			_page = page;
			Debug.Assert(Window.Current != null, "Window.Current != null");
			_windowToWatch = Window.Current;
		}

		/// <summary>
		///    Invoked as an event handler, typically on the <see cref="FrameworkElement.Loaded" />
		///    event of a <see cref="Control" /> within the page, to indicate that the control should
		///    start receiving visual state management changes that correspond to application view
		///    state changes.
		/// </summary>
		/// <param name="control">
		///    Instance of <see cref="Control" /> that supports visual state
		///    management corresponding to view states.
		/// </param>
		/// <remarks>
		///    The current view state will immediately be used to set the corresponding
		///    visual state when layout updates are requested.  A corresponding
		///    <see cref="FrameworkElement.Unloaded" /> event handler connected to
		///    <see cref="StopLayoutUpdates" /> is strongly encouraged.  Instances of
		///    <see cref="LayoutAwarePage" /> automatically invoke these handlers in their Loaded and
		///    Unloaded events.
		/// </remarks>
		/// <seealso cref="InvalidateVisualState" />
		public void StartLayoutUpdates([CanBeNull] Control control)
		{
			if (control == null) return;
			if (_layoutAwareControls.Count == 0)
			{
				_windowToWatch.SizeChanged += WindowSizeChanged;
			}

			_layoutAwareControls.Add(control);
			VisualStateManager.GoToState(control, _page.DetermineVisualState(ApplicationView.Value), false);
		}

		/// <summary>
		///    Invoked as an event handler, typically on the <see cref="FrameworkElement.Unloaded" />
		///    event of a <see cref="Control" />, to indicate that the control should stop receiving
		///    visual state management changes that correspond to application view state changes.
		/// </summary>
		/// <param name="control">
		///    Instance of <see cref="Control" /> that supports visual state
		///    management corresponding to view states.
		/// </param>
		/// <seealso cref="StartLayoutUpdates" />
		public void StopLayoutUpdates([CanBeNull] Control control)
		{
			if (control == null || _layoutAwareControls.Count == 0) return;
			_layoutAwareControls.Remove(control);
			if (_layoutAwareControls.Count == 0)
			{
				_windowToWatch.SizeChanged -= WindowSizeChanged;
			}
		}

		private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
		{
			InvalidateVisualState();
		}

		/// <summary>
		///    Updates all controls that are listening for visual state changes with the correct
		///    visual state.
		/// </summary>
		/// <remarks>
		///    Typically used in conjunction with overriding <see cref="LayoutAwarePage.DetermineVisualState" /> to
		///    signal that a different value may be returned even though the view state has not
		///    changed.
		/// </remarks>
		public void InvalidateVisualState()
		{
			var visualState = _page.DetermineVisualState(ApplicationView.Value);
			foreach (var layoutAwareControl in _layoutAwareControls)
			{
				VisualStateManager.GoToState(layoutAwareControl, visualState, false);
			}
		}
	}
}
