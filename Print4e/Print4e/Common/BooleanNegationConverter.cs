// BooleanNegationConverter.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System;
using Windows.UI.Xaml.Data;

namespace Print4e.Common
{
	/// <summary>
	///    Value converter that translates true to false and vice versa.
	/// </summary>
	public sealed class BooleanNegationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return !(value is bool && (bool) value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return !(value is bool && (bool) value);
		}
	}
}
