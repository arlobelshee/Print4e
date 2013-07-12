// ParseCharacterFile.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Print4e.Core.Tests.Data
{
	[TestFixture]
	public class ParseCharacterFile
	{
		[Test]
		public void ShouldFindTheName()
		{
			CharacterFile.FromXml(TestData.Kuri);
		}
	}

	public class TestData
	{
		private static string _kuri;

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

	public class CharacterFile
	{
		public static CharacterFile FromXml(string fileContents)
		{
			return new CharacterFile();
		}
	}
}
