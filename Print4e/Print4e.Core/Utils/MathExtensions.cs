// MathExtensions.cs
// 
// Copyright 2013 Arlo Belshee (http:/github.com/arlobelshee).
// All rights reserved. Usage as permitted by the LICENSE.txt file for this project.

using System;

namespace Print4e.Core.Utils
{
	public static class MathExtensions
	{
		public static bool ApxEquals(this double lhs, double rhs, double epsilon = 0.001)
		{
			return Math.Abs(lhs - rhs) < epsilon;
		}
	}
}
