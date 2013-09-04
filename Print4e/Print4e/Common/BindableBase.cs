// BindableBase.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Foundation.Metadata;

namespace Print4e.Common
{
	/// <summary>
	///    Implementation of <see cref="INotifyPropertyChanged" /> to simplify models.
	/// </summary>
	[WebHostHidden]
	public abstract class BindableBase : INotifyPropertyChanged
	{
		/// <summary>
		///    Multicast event for property change notifications.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///    Checks if a property already matches a desired value.  Sets the property and
		///    notifies listeners only when necessary.
		/// </summary>
		/// <typeparam name="T">Type of the property.</typeparam>
		/// <param name="storage">Reference to a property with both getter and setter.</param>
		/// <param name="value">Desired value for the property.</param>
		/// <param name="propertyName">
		///    Name of the property used to notify listeners.  This
		///    value is optional and can be provided automatically when invoked from compilers that
		///    support CallerMemberName.
		/// </param>
		/// <returns>
		///    True if the value was changed, false if the existing value matched the
		///    desired value.
		/// </returns>
		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
		{
			Debug.Assert(propertyName != null, "propertyName != null");
			if (Equals(storage, value)) return false;

			storage = value;
			// ReSharper disable ExplicitCallerInfoArgument
			OnPropertyChanged(propertyName);
			// ReSharper restore ExplicitCallerInfoArgument
			return true;
		}

		/// <summary>
		///    Notifies listeners that a property value has changed.
		/// </summary>
		/// <param name="propertyName">
		///    Name of the property used to notify listeners.
		/// </param>
		protected void OnPropertyChanged(string propertyName)
		{
			var eventHandler = PropertyChanged;
			if (eventHandler != null) eventHandler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
