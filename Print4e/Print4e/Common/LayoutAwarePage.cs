// LayoutAwarePage.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Print4e.Core.Utils;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Print4e.Common
{
	/// <summary>
	///    Typical implementation of Page that provides several important conveniences:
	///    <list type="bullet">
	///       <item>
	///          <description>Application view state to visual state mapping</description>
	///       </item>
	///       <item>
	///          <description>GoBack, GoForward, and GoHome event handlers</description>
	///       </item>
	///       <item>
	///          <description>Mouse and keyboard shortcuts for navigation</description>
	///       </item>
	///       <item>
	///          <description>State management for navigation and process lifetime management</description>
	///       </item>
	///       <item>
	///          <description>A default view model</description>
	///       </item>
	///    </list>
	/// </summary>
	[WebHostHidden]
	public class LayoutAwarePage : Page
	{
		/// <summary>
		///    Identifies the <see cref="DefaultViewModel" /> dependency property.
		/// </summary>
		[NotNull] public static readonly DependencyProperty DefaultViewModelProperty =
			DependencyProperty.Register("DefaultViewModel",
				typeof (IObservableMap<String, Object>),
				typeof (LayoutAwarePage),
				null);

		/// <summary>
		///    Initializes a new instance of the <see cref="LayoutAwarePage" /> class.
		/// </summary>
		public LayoutAwarePage()
		{
			_layoutUpdateMonitor = new LayoutUpdateMonitor(this);
			if (DesignMode.DesignModeEnabled) return;

			DefaultViewModel = new ObservableDictionary<String, Object>();

			// When this page is part of the visual tree make two changes:
			// 1) Map application view state to visual state for the page
			// 2) Handle keyboard and mouse navigation requests
			Loaded += (sender, e) => {
				StartLayoutUpdates(sender as Control);

				// Keyboard and mouse navigation only apply when occupying the entire window
				Debug.Assert(Window.Current != null, "Window.Current != null");
				if (ActualHeight.ApxEquals(Window.Current.Bounds.Height) && ActualWidth.ApxEquals(Window.Current.Bounds.Width))
				{
					// Listen to the window directly so focus isn't required
					Debug.Assert(Window.Current.CoreWindow != null, "Window.Current.CoreWindow != null");
					Debug.Assert(Window.Current.CoreWindow.Dispatcher != null, "Window.Current.CoreWindow.Dispatcher != null");
					Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcher_AcceleratorKeyActivated;
					Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;
				}
			};

			// Undo the same changes when the page is no longer visible
			Unloaded += (sender, e) => {
				StopLayoutUpdates(sender as Control);
				Debug.Assert(Window.Current != null, "Window.Current != null");
				Debug.Assert(Window.Current.CoreWindow != null, "Window.Current.CoreWindow != null");
				Debug.Assert(Window.Current.CoreWindow.Dispatcher != null, "Window.Current.CoreWindow.Dispatcher != null");
				Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= CoreDispatcher_AcceleratorKeyActivated;
				Window.Current.CoreWindow.PointerPressed -= CoreWindow_PointerPressed;
			};
		}

		/// <summary>
		///    An implementation of <see cref="IObservableMap&lt;String, Object&gt;" /> designed to be
		///    used as a trivial view model.
		/// </summary>
		[NotNull]
		protected IObservableMap<String, Object> DefaultViewModel
		{
			get
			{
				// ReSharper disable AssignNullToNotNullAttribute
				return (IObservableMap<string, object>) GetValue(DefaultViewModelProperty);
				// ReSharper restore AssignNullToNotNullAttribute
			}
			set { SetValue(DefaultViewModelProperty, value); }
		}

		public LayoutUpdateMonitor LayoutUpdateMonitor { get { return _layoutUpdateMonitor; } }

		#region Navigation support
		/// <summary>
		///    Invoked as an event handler to navigate backward in the page's associated
		///    <see cref="Frame" /> until it reaches the top of the navigation stack.
		/// </summary>
		/// <param name="sender">Instance that triggered the event.</param>
		/// <param name="e">Event data describing the conditions that led to the event.</param>
		protected virtual void GoHome(object sender, RoutedEventArgs e)
		{
			// Use the navigation frame to return to the topmost page
			if (Frame != null)
			{
				while (Frame.CanGoBack)
				{
					Frame.GoBack();
				}
			}
		}

		/// <summary>
		///    Invoked as an event handler to navigate backward in the navigation stack
		///    associated with this page's <see cref="Frame" />.
		/// </summary>
		/// <param name="sender">Instance that triggered the event.</param>
		/// <param name="e">
		///    Event data describing the conditions that led to the
		///    event.
		/// </param>
		protected virtual void GoBack(object sender, RoutedEventArgs e)
		{
			// Use the navigation frame to return to the previous page
			if (Frame != null && Frame.CanGoBack) Frame.GoBack();
		}

		/// <summary>
		///    Invoked as an event handler to navigate forward in the navigation stack
		///    associated with this page's <see cref="Frame" />.
		/// </summary>
		/// <param name="sender">Instance that triggered the event.</param>
		/// <param name="e">
		///    Event data describing the conditions that led to the
		///    event.
		/// </param>
		protected virtual void GoForward(object sender, RoutedEventArgs e)
		{
			// Use the navigation frame to move to the next page
			if (Frame != null && Frame.CanGoForward) Frame.GoForward();
		}

		/// <summary>
		///    Invoked on every keystroke, including system keys such as Alt key combinations, when
		///    this page is active and occupies the entire window.  Used to detect keyboard navigation
		///    between pages even when the page itself doesn't have focus.
		/// </summary>
		/// <param name="sender">Instance that triggered the event.</param>
		/// <param name="args">Event data describing the conditions that led to the event.</param>
		private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
		{
			if (args == null) return;
			if (args.EventType != CoreAcceleratorKeyEventType.SystemKeyDown
				&& args.EventType != CoreAcceleratorKeyEventType.KeyDown) return;

			args.Handled = HandleNavigationKeyCommands(args.VirtualKey);
		}

		/// <summary>
		///    Invoked on every mouse click, touch screen tap, or equivalent interaction when this
		///    page is active and occupies the entire window.  Used to detect browser-style next and
		///    previous mouse button clicks to navigate between pages.
		/// </summary>
		/// <param name="sender">Instance that triggered the event.</param>
		/// <param name="args">Event data describing the conditions that led to the event.</param>
		private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
		{
			if (args == null || args.CurrentPoint == null) return;
			var properties = args.CurrentPoint.Properties;
			if (properties == null) return;
			args.Handled = HandleMouseNavigationButtonCommands(properties);
		}

		private bool HandleNavigationKeyCommands(VirtualKey virtualKey)
		{
			var keyCode = (int) virtualKey;
			const int previousButton = 166;
			const int nextButton = 167;

			if (virtualKey != VirtualKey.Left && virtualKey != VirtualKey.Right && keyCode != previousButton
				&& keyCode != nextButton) return false;

			var modifiers = ModifierKeys.Current();

			if ((keyCode == previousButton && modifiers.None) || (virtualKey == VirtualKey.Left && modifiers.OnlyAlt))
			{
				GoBack(this, new RoutedEventArgs());
				return true;
			}
			if ((keyCode == nextButton && modifiers.None) || (virtualKey == VirtualKey.Right && modifiers.OnlyAlt))
			{
				GoForward(this, new RoutedEventArgs());
				return true;
			}
			return false;
		}

		private bool HandleMouseNavigationButtonCommands([NotNull] PointerPointProperties properties)
		{
			if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed || properties.IsMiddleButtonPressed) return false;

			var backPressed = properties.IsXButton1Pressed;
			var forwardPressed = properties.IsXButton2Pressed;
			if (backPressed && !forwardPressed)
			{
				GoBack(this, new RoutedEventArgs());
				return true;
			}
			if (forwardPressed && !backPressed)
			{
				GoForward(this, new RoutedEventArgs());
				return true;
			}
			return false;
		}
		#endregion

		#region Visual state switching
		[NotNull] private readonly LayoutUpdateMonitor _layoutUpdateMonitor;

		/// <summary>
		///    Invoked as an event handler, typically on the <see cref="FrameworkElement.Loaded" />
		///    event of a <see cref="Control" /> within the page, to indicate that the sender should
		///    start receiving visual state management changes that correspond to application view
		///    state changes.
		/// </summary>
		/// <param name="control"></param>
		/// <remarks>
		///    The current view state will immediately be used to set the corresponding
		///    visual state when layout updates are requested.  A corresponding
		///    <see cref="FrameworkElement.Unloaded" /> event handler connected to
		///    <see cref="StopLayoutUpdates" /> is strongly encouraged.  Instances of
		///    <see cref="LayoutAwarePage" /> automatically invoke these handlers in their Loaded and
		///    Unloaded events.
		/// </remarks>
		public void StartLayoutUpdates(Control control)
		{
			_layoutUpdateMonitor.StartLayoutUpdates(control);
		}

		/// <summary>
		///    Invoked as an event handler, typically on the <see cref="FrameworkElement.Unloaded" />
		///    event of a <see cref="Control" />, to indicate that the sender should stop receiving
		///    visual state management changes that correspond to application view state changes.
		/// </summary>
		/// <param name="control"></param>
		/// <seealso cref="StartLayoutUpdates" />
		public void StopLayoutUpdates([CanBeNull] Control control)
		{
			_layoutUpdateMonitor.StopLayoutUpdates(control);
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
			_layoutUpdateMonitor.InvalidateVisualState();
		}

		/// <summary>
		///    Translates <see cref="ApplicationViewState" /> values into strings for visual state
		///    management within the page.  The default implementation uses the names of enum values.
		///    Subclasses may override this method to control the mapping scheme used.
		/// </summary>
		/// <param name="viewState">View state for which a visual state is desired.</param>
		/// <returns>
		///    Visual state name used to drive the
		///    <see cref="VisualStateManager" />
		/// </returns>
		[NotNull]
		public virtual string DetermineVisualState(ApplicationViewState viewState)
		{
			return viewState.ToString() ?? "";
		}
		#endregion

		#region Process lifetime management
		private String _pageKey;

		/// <summary>
		///    Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">
		///    Event data that describes how this page was reached.  The Parameter
		///    property provides the group to be displayed.
		/// </param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			// Returning to a cached page through navigation shouldn't trigger state loading
			if (_pageKey != null) return;
			Debug.Assert(e != null, "e != null");

			Debug.Assert(Frame != null, "Frame != null");
			var frameState = SuspensionManager.SessionStateForFrame(Frame);
			_pageKey = "Page-" + Frame.BackStackDepth;

			if (e.NavigationMode == NavigationMode.New)
			{
				// Adding a new page to the navigation stack
				RemoveForwardNavigationCachedPages(frameState, Frame.BackStackDepth);
				LoadState(e.Parameter, null);
			}
			else
			{
				// Pass the navigation parameter and preserved page state to the page, using
				// the same strategy for loading suspended state and recreating pages discarded
				// from cache
				object pageState = null;
				if (frameState != null) frameState.TryGetValue(_pageKey, out pageState);
				LoadState(e.Parameter, (Dictionary<string, object>) pageState);
			}
		}

		private void RemoveForwardNavigationCachedPages([CanBeNull] IDictionary<string, object> frameState, int currentPageIndex)
		{
			if (frameState == null) return;
			var nextPageKey = _pageKey;
			var nextPageIndex = currentPageIndex;
			while (frameState.Remove(nextPageKey))
			{
				nextPageIndex++;
				nextPageKey = "Page-" + nextPageIndex;
			}
		}

		/// <summary>
		///    Invoked when this page will no longer be displayed in a Frame.
		/// </summary>
		/// <param name="e">
		///    Event data that describes how this page was reached.  The Parameter
		///    property provides the group to be displayed.
		/// </param>
		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			var frameState = SuspensionManager.SessionStateForFrame(Frame);
			var pageState = new Dictionary<String, Object>();
			SaveState(pageState);
			frameState[_pageKey] = pageState;
		}

		/// <summary>
		///    Populates the page with content passed during navigation.  Any saved state is also
		///    provided when recreating a page from a prior session.
		/// </summary>
		/// <param name="navigationParameter">
		///    The parameter value passed to
		///    <see cref="Frame.Navigate(Type, Object)" /> when this page was initially requested.
		/// </param>
		/// <param name="pageState">
		///    A dictionary of state preserved by this page during an earlier
		///    session.  This will be null the first time a page is visited.
		/// </param>
		protected virtual void LoadState(Object navigationParameter, Dictionary<String, Object> pageState) {}

		/// <summary>
		///    Preserves state associated with this page in case the application is suspended or the
		///    page is discarded from the navigation cache.  Values must conform to the serialization
		///    requirements of <see cref="SuspensionManager.SessionState" />.
		/// </summary>
		/// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
		protected virtual void SaveState(Dictionary<String, Object> pageState) {}
		#endregion
	}
}
