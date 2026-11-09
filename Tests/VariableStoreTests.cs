using NUnit.Framework;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Tests
{
    [TestFixture]
    public class VariableStoreTests
    {
        private VariableStore _store;

        [SetUp]
        public void SetUp()
        {
            _store = new VariableStore();
        }

        [Test]
        public void SetAndGet_BasicTypes_ReturnsExactValues()
        {
            _store.Set("flagA", true);
            _store.Set("count", 10);
            _store.Set("affinity", 2.5f);
            _store.Set("playerName", "アリス");

            Assert.AreEqual(true, _store.GetBool("flagA"));
            Assert.AreEqual(10, _store.GetInt("count"));
            Assert.AreEqual(2.5f, _store.GetFloat("affinity"), 0.0001f);
            Assert.AreEqual("アリス", _store.GetString("playerName"));
        }

        [Test]
        public void GetUndefinedVariable_ThrowsNovelScriptException()
        {
            Assert.Throws<NovelScriptException>(() => _store.GetInt("undefined_var"));
        }

        [Test]
        public void TryGet_UndefinedVariable_ReturnsFalse()
        {
            var success = _store.TryGet("undefined_var", out int val);
            Assert.IsFalse(success);
            Assert.AreEqual(0, val);
        }

        [Test]
        public void Add_NumericVariables_UpdatesValuesCorrectly()
        {
            _store.Set("score", 100);
            _store.Add("score", 50);
            Assert.AreEqual(150, _store.GetInt("score"));

            _store.Set("speed", 1.5f);
            _store.Add("speed", 0.5f);
            Assert.AreEqual(2.0f, _store.GetFloat("speed"), 0.0001f);
        }

        [Test]
        public void Toggle_BoolVariable_FlipsValue()
        {
            _store.Set("flag", false);
            _store.Toggle("flag");
            Assert.IsTrue(_store.GetBool("flag"));
            _store.Toggle("flag");
            Assert.IsFalse(_store.GetBool("flag"));
        }

        [Test]
        public void ConditionEvaluator_NumberComparisons_EvaluatesCorrectly()
        {
            _store.Set("affinity", 10);

            Assert.IsTrue(ConditionEvaluator.Evaluate(_store, "affinity", "==", "10"));
            Assert.IsTrue(ConditionEvaluator.Evaluate(_store, "affinity", ">=", "10"));
            Assert.IsTrue(ConditionEvaluator.Evaluate(_store, "affinity", "<=", "10"));
            Assert.IsTrue(ConditionEvaluator.Evaluate(_store, "affinity", ">", "5"));
            Assert.IsTrue(ConditionEvaluator.Evaluate(_store, "affinity", "<", "15"));
            Assert.IsFalse(ConditionEvaluator.Evaluate(_store, "affinity", "!=", "10"));
        }

        [Test]
        public void ConditionEvaluator_StringAndBoolComparisons_EvaluatesCorrectly()
        {
            _store.Set("flag", true);
            _store.Set("route", "TrueEnd");

            Assert.IsTrue(ConditionEvaluator.Evaluate(_store, "flag", "==", "true"));
            Assert.IsFalse(ConditionEvaluator.Evaluate(_store, "flag", "==", "false"));
            Assert.IsTrue(ConditionEvaluator.Evaluate(_store, "route", "==", "TrueEnd"));
            Assert.IsTrue(ConditionEvaluator.Evaluate(_store, "route", "!=", "BadEnd"));
        }

        [Test]
        public void SerializeAndDeserialize_RestoresAllVariables()
        {
            _store.Set("intVal", 42);
            _store.Set("boolVal", true);
            _store.Set("floatVal", 3.14f);
            _store.Set("strVal", "Hello");

            var json = _store.ToJson();

            var newStore = new VariableStore();
            newStore.FromJson(json);

            Assert.AreEqual(42, newStore.GetInt("intVal"));
            Assert.AreEqual(true, newStore.GetBool("boolVal"));
            Assert.AreEqual(3.14f, newStore.GetFloat("floatVal"), 0.0001f);
            Assert.AreEqual("Hello", newStore.GetString("strVal"));
        }
    }
}

