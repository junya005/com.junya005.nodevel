using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Tests
{
    [TestFixture]
    public class SaveDataStorageTests
    {
        private string _testDir;
        private LocalFileSaveDataStorage _storage;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Application.temporaryCachePath, "NodevelSaveTests");
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
            Directory.CreateDirectory(_testDir);
            _storage = new LocalFileSaveDataStorage(_testDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [Test]
        public void SaveDataSnapshot_SerializeAndDeserialize_RestoresAllFields()
        {
            var original = new SaveDataSnapshot
            {
                Version = 1,
                Timestamp = 123456789,
                GraphGuid = "test_graph_guid",
                NodeId = "script_node_1",
                LineIndex = 3,
                VariablesJson = "{\"score\": 100}",
                ReadStateJson = "[\"key1\"]"
            };

            var json = original.ToJson();
            var restored = SaveDataSnapshot.FromJson(json);

            Assert.AreEqual(original.Version, restored.Version);
            Assert.AreEqual(original.Timestamp, restored.Timestamp);
            Assert.AreEqual(original.GraphGuid, restored.GraphGuid);
            Assert.AreEqual(original.NodeId, restored.NodeId);
            Assert.AreEqual(original.LineIndex, restored.LineIndex);
            Assert.AreEqual(original.VariablesJson, restored.VariablesJson);
            Assert.AreEqual(original.ReadStateJson, restored.ReadStateJson);
        }

        [Test]
        public async Task LocalFileSaveDataStorage_SaveAndLoad_Succeeds()
        {
            var key = "slot1";
            var text = "Hello, Save Data!";
            var data = Encoding.UTF8.GetBytes(text);

            Assert.IsFalse(_storage.Exists(key));

            await _storage.SaveAsync(key, data, CancellationToken.None);

            Assert.IsTrue(_storage.Exists(key));

            var loaded = await _storage.LoadAsync(key, CancellationToken.None);
            Assert.IsNotNull(loaded);
            var loadedText = Encoding.UTF8.GetString(loaded);
            Assert.AreEqual(text, loadedText);

            _storage.Delete(key);
            Assert.IsFalse(_storage.Exists(key));
        }

        [Test]
        public async Task LocalFileSaveDataStorage_AtomicWrite_OverwritesSafely()
        {
            var key = "slot_overwrite";
            var data1 = Encoding.UTF8.GetBytes("First Version");
            var data2 = Encoding.UTF8.GetBytes("Second Version (Updated)");

            await _storage.SaveAsync(key, data1, CancellationToken.None);
            await _storage.SaveAsync(key, data2, CancellationToken.None);

            var loaded = await _storage.LoadAsync(key, CancellationToken.None);
            var loadedText = Encoding.UTF8.GetString(loaded);
            Assert.AreEqual("Second Version (Updated)", loadedText);
        }
    }
}

