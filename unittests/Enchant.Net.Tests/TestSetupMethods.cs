using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Enchant.Tests
{
	public static class TestSetupMethods
	{
		public static void FixtureSetup()
		{
			var providerDir = Path.Combine(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "lib"),
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
			string dictionarySourceDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

			var dictionaryDestDir = Path.Combine(Path.Combine(Path.Combine(
					Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "share"), "enchant"),
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
			Directory.Delete(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "lib"), true);
			Directory.Delete(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "share"), true);
		}
	}
}