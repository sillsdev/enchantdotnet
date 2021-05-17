using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using System.Reflection;

namespace Enchant.Tests
{
	public static class TestSetupMethods
	{
		private static string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		public static void FixtureSetup()
		{
			var providerDir = Path.Combine(currentDir, "lib",
				"enchant");
			if (!Directory.Exists(providerDir))
			{
				Directory.CreateDirectory(providerDir);
			}

			// If we find the ispell or myspell dll, we copy it to the provider directory.
			// Otherwise we rely on the OS to find it (as it the case on Linux)
			if (File.Exists("libenchant_ispell.dll"))
				File.Copy("libenchant_ispell.dll",
					Path.Combine(providerDir, "libenchant_ispell.dll"), true);
			if (File.Exists("libenchant_myspell.dll"))
				File.Copy("libenchant_myspell.dll",
					Path.Combine(providerDir, "libenchant_myspell.dll"), true);
			InstallDictionary("myspell", new string[] { "en_US.aff", "en_US.dic" });
		}

		private static void InstallDictionary(string provider, IEnumerable<string> files)
		{
			var dictionarySourceDir = currentDir;

			var dictionaryDestDir = Path.Combine(
					currentDir, "share", "enchant",
				provider);

			if (!Directory.Exists(dictionaryDestDir))
			{
				Directory.CreateDirectory(dictionaryDestDir);
			}

			foreach (var file in files)
			{
				File.Copy(Path.Combine(dictionarySourceDir, file),
					Path.Combine(dictionaryDestDir, file), true);
			}
		}

		public static void FixtureTearDown()
		{
			Directory.Delete(Path.Combine(currentDir, "lib"), true);
			Directory.Delete(Path.Combine(currentDir, "share"), true);
		}
	}
}