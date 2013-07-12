// ParseCharacterFile.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using FluentAssertions;
using NUnit.Framework;
using Print4e.Core.DataAccess;

namespace Print4e.Core.Tests.Data
{
	[TestFixture]
	public class ParseCharacterFile
	{
		[Test]
		public void ShouldFindTheName()
		{
			var result = CharacterFile.FromXml(TestData.Kuri);
			result.Name.Should()
				.Be("Kuri");
		}
	}
}
