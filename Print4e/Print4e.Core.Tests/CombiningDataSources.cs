// CombiningDataSources.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using FluentAssertions;
using NUnit.Framework;
using Print4e.Core.Api;
using Print4e.Core.Tests.Data;

namespace Print4e.Core.Tests
{
	[TestFixture]
	public class CombiningDataSources
	{
		[Test]
		public void ShouldCopyNameFromCharacterFile()
		{
			AssembleCharacter.From(Kuri.MainFile)
				.ShouldBeEquivalentTo(Kuri.FinishedCharacter, options => options.Including(c => c.Name));
		}
	}
}
