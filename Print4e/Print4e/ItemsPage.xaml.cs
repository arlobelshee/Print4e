// ItemsPage.xaml.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System;
using System.Collections.Generic;
using Print4e.Common;
using Print4e.Data;
using Windows.UI.Xaml.Controls;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Print4e
{
	/// <summary>
	///    A page that displays a collection of item previews.  In the Split Application this page
	///    is used to display and select one of the available groups.
	/// </summary>
	public sealed partial class ItemsPage
	{
		public ItemsPage()
		{
			InitializeComponent();
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
		protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
		{
			// TODO: Create an appropriate data model for your problem domain to replace the sample data
			var sampleDataGroups = SampleDataSource.GetGroups((String) navigationParameter);
			DefaultViewModel["Items"] = sampleDataGroups;
		}

		/// <summary>
		///    Invoked when an item is clicked.
		/// </summary>
		/// <param name="sender">
		///    The GridView (or ListView when the application is snapped)
		///    displaying the item clicked.
		/// </param>
		/// <param name="e">Event data that describes the item clicked.</param>
		private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
		{
			// Navigate to the appropriate destination page, configuring the new page
			// by passing required information as a navigation parameter
			var groupId = ((SampleDataGroup) e.ClickedItem).UniqueId;
			Frame.Navigate(typeof (SplitPage), groupId);
		}
	}
}
