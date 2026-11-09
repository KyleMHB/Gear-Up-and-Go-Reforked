using System;
using System.Collections.Generic;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class StaticConstructorOnStartupAttribute : Attribute
	{
	}

	public sealed class Pawn
	{
	}

	public sealed class ModContentPack
	{
		public string PackageIdPlayerFacing { get; set; }
	}

	public static class ModsConfig
	{
		public static List<ModContentPack> ActiveModsInLoadOrder { get; } = new List<ModContentPack>();
	}

	public static class Log
	{
		public static void Error(string message)
		{
		}

		public static void Warning(string message)
		{
		}
	}
}
