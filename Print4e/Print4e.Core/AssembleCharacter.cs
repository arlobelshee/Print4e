// AssembleCharacter.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using JetBrains.Annotations;
using Print4e.Core.Api;
using Print4e.Core.DataAccess;

namespace Print4e.Core
{
	public static class AssembleCharacter
	{
		[NotNull]
		public static Character From([NotNull] CharacterFile mainFile)
		{
			var character = new Character {Name = mainFile.Name};
			return character;
		}
	}
}
