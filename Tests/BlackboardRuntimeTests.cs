using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Tests
{
    [TestFixture]
    public class BlackboardRuntimeTests
    {
        [Test]
        public void GraphVariable_ToVariableValue_ConvertsCorrectly()
        {
            var intVar = new GraphVariable("score", VariableType.Int, "100", "プレイヤーのスコア");
            var floatVar = new GraphVariable("speed", VariableType.Float, "3.14", "移動速度");
            var stringVar = new GraphVariable("name", VariableType.String, "Alice", "キャラクター名");
            var boolVar = new GraphVariable("hasKey", VariableType.Bool, "true", "鍵所持フラグ");

            var intVal = intVar.ToVariableValue();
            var floatVal = floatVar.ToVariableValue();
            var strVal = stringVar.ToVariableValue();
            var boolVal = boolVar.ToVariableValue();

            Assert.AreEqual(VariableType.Int, intVal.Type);
            Assert.AreEqual(100, intVal.AsInt());

            Assert.AreEqual(VariableType.Float, floatVal.Type);
            Assert.AreEqual(3.14f, floatVal.AsFloat(), 0.001f);

            Assert.AreEqual(VariableType.String, strVal.Type);
            Assert.AreEqual("Alice", strVal.AsString());

            Assert.AreEqual(VariableType.Bool, boolVal.Type);
            Assert.IsTrue(boolVal.AsBool());
        }

        [Test]
        public void NodevelGraph_Variables_AddAndRemove_WorksCorrectly()
        {
            var graph = ScriptableObject.CreateInstance<NodevelGraph>();

            var v1 = new GraphVariable("gold", VariableType.Int, "500");
            var v2 = new GraphVariable("hero", VariableType.String, "Bob");

            var added1 = graph.AddVariable(v1);
            var added2 = graph.AddVariable(v2);

            Assert.IsTrue(added1);
            Assert.IsTrue(added2);
            Assert.AreEqual(2, graph.Variables.Count);

            // 重複名は追加拒否されること
            var vDuplicate = new GraphVariable("gold", VariableType.Int, "999");
            var addedDup = graph.AddVariable(vDuplicate);
            Assert.IsFalse(addedDup);
            Assert.AreEqual(2, graph.Variables.Count);

            // 取得
            var found = graph.GetVariable("hero");
            Assert.IsNotNull(found);
            Assert.AreEqual("Bob", found.InitialValue);

            // 削除
            var removed = graph.RemoveVariable("gold");
            Assert.IsTrue(removed);
            Assert.AreEqual(1, graph.Variables.Count);
            Assert.IsNull(graph.GetVariable("gold"));
        }

        [Test]
        public async Task GraphRunner_InitializeBlackboardVariables_RegistersToStore()
        {
            var graph = ScriptableObject.CreateInstance<NodevelGraph>();
            var start = graph.CreateNode<StartNode>();
            var end = graph.CreateNode<EndNode>();
            graph.Connect(start.Id, 0, end.Id, 0);

            // Blackboard 変数を定義
            graph.AddVariable(new GraphVariable("hp", VariableType.Int, "100"));
            graph.AddVariable(new GraphVariable("title", VariableType.String, "Adventurer"));

            var varStore = new VariableStore();
            // すでにhpが存在している場合（セーブデータからの復旧など）
            varStore.Set("hp", VariableValue.Create(50));

            var registry = new CommandRegistry();
            var runner = new GraphRunner(varStore, registry);

            await runner.ExecuteAsync(graph, CancellationToken.None);

            // 既存の変数は上書きされない
            Assert.AreEqual(50, varStore.Get("hp").AsInt());

            // 未登録の変数は初期値が投入される
            Assert.IsTrue(varStore.Has("title"));
            Assert.AreEqual("Adventurer", varStore.Get("title").AsString());
        }
    }
}

