// Kuri.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using JetBrains.Annotations;
using Print4e.Core.Api;
using Print4e.Core.DataAccess;

namespace Print4e.Core.Tests.Data
{
	internal static class Kuri
	{
		[NotNull]
		public static Character FinishedCharacter { get { return new Character {Name = "Kuri"}; } }

		[NotNull]
		public static CharacterFile MainFile { get { return new CharacterFile {Name = "Kuri"}; } }
	}
}
