// TestData.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace Print4e.Core.Tests.Data
{
	public class TestData
	{
		[CanBeNull] private static string _kuri;

		[NotNull]
		public static string Kuri
		{
			get
			{
				if (_kuri == null)
				{
					var data = Assembly.GetExecutingAssembly()
						.GetManifestResourceStream("Print4e.Core.Tests.Data.Kuri.dnd4e");
					_kuri = data == null ? string.Empty : new StreamReader(data).ReadToEnd();
				}
				return _kuri;
			}
		}
	}
}
