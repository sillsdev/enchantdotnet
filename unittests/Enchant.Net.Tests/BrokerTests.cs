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
using System.Threading;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enchant.Tests
{
	[TestFixture]
	public class BrokerTests
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			oldRegistryValue = (string)
												 Registry.GetValue(@"HKEY_CURRENT_USER\Software\Enchant\Config", "Data_Dir", null);
			tempdir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			Registry.SetValue(@"HKEY_CURRENT_USER\Software\Enchant\Config", "Data_Dir", tempdir, RegistryValueKind.String);
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
			while (Directory.Exists(tempdir))
			{
				Directory.Delete(tempdir, true);
			}
		}

		#endregion

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

		[Test]
		public void Construct()
		{
			Assert.IsNotNull(new Broker());
		}

		[Test]
		public void Dictionaries()
		{
			using (var broker = new Broker())
			{
				var dictionaries = broker.Dictionaries;
				Assert.IsNotNull(dictionaries);
				var count = 0;
				foreach (var info in dictionaries)
				{
					Console.WriteLine("Language:{0}\tName:{1}\tDescription:{2}\tFile:{3}",
														info.Language,
														info.Provider.Name,
														info.Provider.Description,
														info.Provider.File);
					Assert.IsNotEmpty(info.Language);
					Assert.IsNotEmpty(info.Provider.Name);
					Assert.IsNotEmpty(info.Provider.Description);
					Assert.IsNotEmpty(info.Provider.File);
					++count;
				}
				Assert.That(count, Is.AtLeast(1));
			}
		}

		[Test]
		public void DictionaryExists()
		{
			using (var broker = new Broker())
			{
				Assert.IsFalse(broker.DictionaryExists("qaa"));
				Assert.IsTrue(broker.DictionaryExists("en_US"));
			}
		}

		[Test]
		public void Providers()
		{
			using (var broker = new Broker())
			{
				var providers = broker.Providers;
				Assert.IsNotNull(providers);
				var count = 0;
				foreach (var info in providers)
				{
					Console.WriteLine("Name:{0}\tDescription:{1}\tFile:{2}",
														info.Name,
														info.Description,
														info.File);
					Assert.IsNotEmpty(info.Name);
					Assert.IsNotEmpty(info.Description);
					Assert.IsNotEmpty(info.File);
					++count;
				}
				Assert.That(count, Is.AtLeast(2));
			}
		}

		[Test]
		public void RequestDictionary()
		{
			using (var broker = new Broker())
			{
				var dictionary = broker.RequestDictionary("en_US");
				Assert.IsNotNull(dictionary);
			}
		}

		[Test]
		public void RequestDictionary_CachingEnabled_DictionaryReRequested_SameReference()
		{
			using (var broker = new Broker())
			{
				broker.CacheDictionaries = true;
				var dictionaryFirstRequest = broker.RequestDictionary("en_US");
				var dictionarySecondRequest = broker.RequestDictionary("en_US");

				Assert.AreSame(dictionaryFirstRequest, dictionarySecondRequest);
			}
		}

		[Test]
		public void RequestDictionary_CachingEnabled_DictionaryDisposedThenReRequested_DifferentReference()
		{
			using (var broker = new Broker())
			{
				broker.CacheDictionaries = true;
				Dictionary dictionaryFirstRequest;
				using (dictionaryFirstRequest = broker.RequestDictionary("en_US")) {}
				var dictionarySecondRequest = broker.RequestDictionary("en_US");

					Assert.AreNotSame(dictionaryFirstRequest, dictionarySecondRequest);
			}
		}

		[Test]
		public void RequestDictionary_CachingDisabled_DictionaryReRequested_DifferentReference()
		{
			using (var broker = new Broker())
			{
				broker.CacheDictionaries = false;
				var dictionaryFirstRequest = broker.RequestDictionary("en_US");
				var dictionarySecondRequest = broker.RequestDictionary("en_US");

				Assert.AreNotSame(dictionaryFirstRequest, dictionarySecondRequest);
			}
		}

		[Test]
		public void RequestDictionary_CachingDisabled_DictionaryDisposedThenReRequested_DifferentReference()
		{
			using (var broker = new Broker())
			{
				broker.CacheDictionaries = false;
				Dictionary dictionaryFirstRequest;
				using (dictionaryFirstRequest = broker.RequestDictionary("en_US"))
				{
				}
				var dictionarySecondRequest = broker.RequestDictionary("en_US");

				Assert.AreNotSame(dictionaryFirstRequest, dictionarySecondRequest);
			}
		}

		[Test]
		[ExpectedException(typeof(ObjectDisposedException))]
		public void Dispose_UseDictionaryAfterBrokerDisposed_Throws()
		{
			Dictionary dictionary;
			using (var broker = new Broker())
			{
				dictionary = broker.RequestDictionary("en_US");
			}
			var info = dictionary.Information;
		}

		[Test]
		[ExpectedException(typeof (ApplicationException))]
		public void RequestDictionary_DictionaryDoesNotExist_Throws()
		{
			using (var broker = new Broker())
			{
				broker.RequestDictionary("qaa");
			}
		}

		[Test]
		public void RequestPwlDictionary()
		{
			var filename = Path.GetTempFileName();
			using (var broker = new Broker())
			{
				using (var dictionary = broker.RequestPwlDictionary(filename))
				{
					Assert.IsNotNull(dictionary);
					File.Delete(filename);
				}
			}
		}

		[Test]
		public void SetOrdering()
		{
			using (var broker = new Broker())
			{
				broker.SetOrdering("en_US", "aspell, myspell, ispell");
			}
		}

		[Test]
		public void DictionaryKeepsBrokerAlive()
		{
			WeakReference brokerReference;
			var dictionary = GetDictionaryAllowingBrokerToGoOutOfScope(out brokerReference);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Assert.IsTrue(brokerReference.IsAlive);
			GC.KeepAlive(dictionary);
		}

		private static Dictionary GetDictionaryAllowingBrokerToGoOutOfScope(out WeakReference brokerReference)
		{
			var broker = new Broker();
			brokerReference = new WeakReference(broker);
			return broker.RequestDictionary("en_US");
		}

		[Test]
		public void Finalize_DictionaryGoesOutOfScope_Finalized()
		{
			using (var broker = new Broker())
			{
				broker.CacheDictionaries = true;
				var dictionaryReference = GetDictionaryReference(broker);
				Thread.Sleep(10);
				GC.Collect();
				GC.WaitForPendingFinalizers();
				Assert.IsFalse(dictionaryReference.IsAlive);
			}
		}

		//this will allow the dictionary object to go out of scope
		private static WeakReference GetDictionaryReference(Broker broker)
		{
			var dictionary = broker.RequestDictionary("en_US");
			return new WeakReference(dictionary);
		}

		[Test]
		public void Default_ReturnsNonNull()
		{
			Assert.IsNotNull(Broker.Default);
		}

		[Test]
		public void Default_CalledTwice_ReturnsSame()
		{
			Assert.AreSame(Broker.Default, Broker.Default);
		}

		[Test]
		public void Default_Disposed_ReturnsNonNull()
		{
			Broker.Default.Dispose();
			Assert.IsNotNull(Broker.Default);
		}

		[Test]
		public void Default_Disposed_ReturnsNewObject()
		{
			var originalBroker = Broker.Default;
			originalBroker.Dispose();

			Assert.AreNotSame(originalBroker, Broker.Default);
		}

	}
}
