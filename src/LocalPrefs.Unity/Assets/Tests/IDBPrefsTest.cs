#nullable enable

using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AndanteTribe.IO.Json;
using AndanteTribe.IO.MessagePack;
using AndanteTribe.IO.Tests;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace AndanteTribe.IO.Unity.Tests
{
    public class IDBPrefsTest
    {
        private sealed class IDBAccessor : FileAccessor
        {
            public static readonly IDBAccessor Shared = new(LocalPrefsTest.TestFilePath);

            private IDBAccessor(in string path) : base(path)
            {
            }

            public override byte[] ReadAllBytes() => IDBUtils.ReadAllBytesAsync(SavePath).GetAwaiter().GetResult();

            public override Stream GetWriteStream() => new IDBStream(SavePath);

            public override ValueTask DeleteAsync(CancellationToken cancellationToken = default) =>
                IDBUtils.DeleteAsync(SavePath, cancellationToken);
        }

        private static readonly Func<ILocalPrefs>[] s_factories =
        {
            () => new JsonLocalPrefs(IDBAccessor.Shared),
            () => new MessagePackLocalPrefs(IDBAccessor.Shared),
        };

        [SetUp]
        public void Setup()
        {
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await IDBUtils.DeleteAsync(LocalPrefsTest.TestFilePath);
            });
        }

        [UnityTest]
        public IEnumerator SaveAndLoad_Int([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.SaveAndLoad_Int(factory);
            });
        }

        [UnityTest]
        public IEnumerator SaveAndLoad_Int_OtherInstance([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.SaveAndLoad_Int_OtherInstance(factory);
            });
        }

        [UnityTest]
        public IEnumerator SaveAndLoad_String([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.SaveAndLoad_String(factory);
            });
        }

        [UnityTest]
        public IEnumerator SaveAndLoad_String_OtherInstance([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.SaveAndLoad_String_OtherInstance(factory);
            });
        }

        [UnityTest]
        public IEnumerator SaveAndLoad_CustomType([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.SaveAndLoad_CustomType(factory);
            });
        }

        [UnityTest]
        public IEnumerator SaveAndLoad_CustomType_OtherInstance([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.SaveAndLoad_CustomType_OtherInstance(factory);
            });
        }

        [UnityTest]
        public IEnumerator OverwriteValue([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.OverwriteValue(factory);
            });
        }

        [UnityTest]
        public IEnumerator OverwriteValue_OtherInstance([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.OverwriteValue_OtherInstance(factory);
            });
        }

        [UnityTest]
        public IEnumerator HasKey_Works([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.HasKey_Works(factory);
            });
        }

        [UnityTest]
        public IEnumerator HasKey_Works_OtherInstance([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.HasKey_Works_OtherInstance(factory);
            });
        }

        [UnityTest]
        public IEnumerator Delete_RemovesKey([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.Delete_RemovesKey(factory);
            });
        }

        [UnityTest]
        public IEnumerator Delete_RemovesKey_OtherInstance([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.Delete_RemovesKey_OtherInstance(factory);
            });
        }

        [UnityTest]
        public IEnumerator Delete_EmptyPrefs_Throws([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.Delete_EmptyPrefs_Throws(factory);
            });
        }

        [UnityTest]
        public IEnumerator DeleteAll_RemovesAll([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.DeleteAll_RemovesAll(factory);
            });
        }

        [UnityTest]
        public IEnumerator DeleteAll_RemovesAll_OtherInstance([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.DeleteAll_RemovesAll_OtherInstance(factory);
            });
        }

        [Test]
        public void Load_NonExistentKey_ReturnsDefault([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory) =>
            LocalPrefsTest.Load_NonExistentKey_ReturnsDefault(factory);

        [UnityTest]
        public IEnumerator Delete_NonExistentKey_Throws([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.Delete_NonExistentKey_Throws(factory);
            });
        }

        [UnityTest]
        public IEnumerator Delete_SecondElement([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.Delete_SecondElement(factory);
            });
        }

        [UnityTest]
        public IEnumerator AddAndRemoveMultipleTimes([ValueSource(nameof(s_factories))] Func<ILocalPrefs> factory)
        {
            yield return new ToCoroutineEnumerator(async () =>
            {
                await LocalPrefsTest.AddAndRemoveMultipleTimes(factory);
            });
        }
    }
}