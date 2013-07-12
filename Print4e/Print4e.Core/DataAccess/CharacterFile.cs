// CharacterFile.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Print4e.Core.DataAccess
{
	public class CharacterFile
	{
		[NotNull]
		private static string GetChildValue([NotNull] XElement details, [NotNull] string elementName)
		{
			return GetSingleChild(details, elementName)
				.Value.Trim();
		}

		[NotNull]
		private static XElement GetSingleChild([NotNull] XElement data, [NotNull] string elementName)
		{
			// ReSharper disable AssignNullToNotNullAttribute
			return data.Descendants(elementName)
				.Single();
			// ReSharper restore AssignNullToNotNullAttribute
		}

		[NotNull]
		public string Name { get; set; }

		[NotNull]
		public static CharacterFile FromXml([NotNull] string fileContents)
		{
			var data = XElement.Parse(fileContents);
			var result = new CharacterFile();
			var charSheet = GetSingleChild(data, "CharacterSheet");
			var details = GetSingleChild(charSheet, "Details");
			result.Name = GetChildValue(details, "name");
			return result;
		}
	}
}
