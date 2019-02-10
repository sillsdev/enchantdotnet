/* Copyright (c) 2007 Eric Scott Albright
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enchant.Tests
{
	[TestFixture]
	public class DictionaryTests
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			oldRegistryValue = (string) Registry.GetValue(@"HKEY_CURRENT_USER\Software\Enchant\Config", "Data_Dir", null);
			tempdir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			Registry.SetValue(@"HKEY_CURRENT_USER\Software\Enchant\Config", "Data_Dir", tempdir, RegistryValueKind.String);
			broker = new Broker();
			dictionary = broker.RequestDictionary("en_US");
		}

		[TearDown]
		public void Teardown()
		{
			if (oldRegistryValue == null)
			{
				Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Enchant").OpenSubKey("Config", true).DeleteValue(
					"Data_Dir");
			}
			else
			{
				Registry.SetValue(@"HKEY_CURRENT_USER\Software\Enchant\Config",
													"Data_Dir",
													oldRegistryValue,
													RegistryValueKind.String);
			}

			dictionary.Dispose();
			broker.Dispose();
			while (Directory.Exists(tempdir))
			{
				Directory.Delete(tempdir, true);
			}
		}

		#endregion

		private Broker broker;
		private Dictionary dictionary;
		private string tempdir;
		private string oldRegistryValue;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			TestSetupMethods.FixtureSetup();
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			TestSetupMethods.FixtureTearDown();
		}

		private static string RandomWord
		{
			get
			{
				var bldr = new StringBuilder();
				var random = new Random();
				for (int i = 0; i < 6; i++)
					bldr.Append(Convert.ToChar(random.Next(65, 90)));

				return bldr.ToString();
			}
		}

		[Test]
		public void Add()
		{
			var word = RandomWord;
			dictionary.Add(word);
			Assert.IsTrue(dictionary.Check(word));
		}

		[Test]
		public void AddToSession()
		{
			var word = RandomWord;
			dictionary.AddToSession(word);
			Assert.IsTrue(dictionary.Check(word));
		}

		[Test]
		public void Check()
		{
			Assert.IsTrue(dictionary.Check("hello"));
			Assert.IsFalse(dictionary.Check("helo"));
		}

		[Test]
		public void Information()
		{
			DictionaryInfo info = dictionary.Information;
			Console.WriteLine("Language:{0}\tName:{1}\tDescription:{2}\tFile:{3}",
												info.Language,
												info.Provider.Name,
												info.Provider.Description,
												info.Provider.File);
			Assert.AreEqual("en_US", info.Language);
			Assert.IsNotEmpty(info.Provider.Name);
			Assert.IsNotEmpty(info.Provider.Description);
			Assert.IsNotEmpty(info.Provider.File);
		}

		[Test]
		public void IsAdded()
		{
			var word = RandomWord;
			Assert.IsFalse(dictionary.IsAdded(word));
			dictionary.AddToSession(word);
			Assert.IsTrue(dictionary.IsAdded(word));
		}

		[Test]
		public void IsRemoved()
		{
			var word = RandomWord;
			dictionary.Add(word);
			Assert.IsFalse(dictionary.IsRemoved(word));
			dictionary.RemoveFromSession(word);
			Assert.IsTrue(dictionary.IsRemoved(word));
		}

		[Test]
		public void Remove()
		{
			var word = RandomWord;
			dictionary.Add(word);
			Assert.IsTrue(dictionary.Check(word));
			dictionary.Remove(word);
			Assert.IsFalse(dictionary.Check(word));
		}

		[Test]
		public void RemoveFromSession()
		{
			var word = RandomWord;
			dictionary.Add(word);
			Assert.IsTrue(dictionary.Check(word));
			dictionary.RemoveFromSession(word);
			Assert.IsFalse(dictionary.Check(word));
		}

		[Test]
		public void StoreReplacement()
		{
			Assert.That(() => dictionary.StoreReplacement("theirs", "their's"),
				Throws.Nothing);
		}

		[Test]
		public void Suggest()
		{
			List<string> suggestions = new List<string>(dictionary.Suggest("helo"));
			Assert.Contains("hello", suggestions);
		}

        [Test]
        public void Dispose_Called_SendsDisposedEvent()
        {
            bool disposedEventCalled = false;
            dictionary.Disposed += delegate
                                   { disposedEventCalled = true; };
            dictionary.Dispose();
            Assert.IsTrue(disposedEventCalled);
        }
	}
}