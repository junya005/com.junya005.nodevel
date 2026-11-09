using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Tests
{
    [TestFixture]
    public class ReplayAndRollbackTests
    {
        private class TestTextPresenter : INovelTextPresenter
        {
            public List<string> Displayed = new List<string>();

            public Awaitable ShowTextAsync(ScriptDialogueLine line, CancellationToken ct)
            {
                Displayed.Add($"{line.Speaker}: {line.CleanText}");
                var acs = new AwaitableCompletionSource();
                acs.SetResult();
                return acs.Awaitable;
            }

            public Awaitable ClearAsync(CancellationToken ct)
            {
                var acs = new AwaitableCompletionSource();
                acs.SetResult();
                return acs.Awaitable;
            }
        }

        [Test]
        public void ReadStateManager_MarksAndChecksReadStatus()
        {
            var manager = new ReadStateManager();
            Assert.IsFalse(manager.IsRead("g1", "n1", 0));

            manager.MarkAsRead("g1", "n1", 0);
            Assert.IsTrue(manager.IsRead("g1", "n1", 0));
            Assert.IsFalse(manager.IsRead("g1", "n1", 1));

            var json = manager.ToJson();
            var newManager = new ReadStateManager();
            newManager.FromJson(json);
            Assert.IsTrue(newManager.IsRead("g1", "n1", 0));
        }

        [Test]
        public void BacklogManager_AppendsEntriesAndEnforcesLimit()
        {
            var backlog = new BacklogManager(maxEntries: 2);
            backlog.Add("アリス", "セリフ1", "id1");
            backlog.Add("ボブ", "セリフ2", "id2");
            backlog.Add("アリス", "セリフ3", "id3");

            var entries = backlog.GetEntries();
            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual("セリフ2", entries[0].Text);
            Assert.AreEqual("セリフ3", entries[1].Text);
        }

        [Test]
        public void AutoPlayController_CalculatesWaitDurationBasedOnTextLength()
        {
            var controller = new AutoPlayController
            {
                BaseWaitSeconds = 1.0f,
                PerCharacterSeconds = 0.05f
            };

            var duration = controller.CalculateWaitDuration("こんにちは"); // 5文字
            Assert.AreEqual(1.25f, duration, 0.0001f);
        }

        [Test]
        public async Task ReplayRunner_FastForward_SimulatesVariablesToTargetLine()
        {
            var graph = ScriptableObject.CreateInstance<NodevelGraph>();
            var start = graph.CreateNode<StartNode>("start");
            var script = graph.CreateNode<ScriptNode>("script1");
            script.ScriptText = @"
@var_add coins 10
@var_add coins 20
アリス: ここまで到達
@var_add coins 50
";
            var end = graph.CreateNode<EndNode>("end");
            graph.Connect(start.Id, 0, script.Id, 0);
            graph.Connect(script.Id, 0, end.Id, 0);

            var variables = new VariableStore();
            var commands = new CommandRegistry();
            commands.RegisterCommand("var_add", new DelegateCommandInvoker((pos, named, ct) =>
            {
                var name = pos[0];
                var val = int.Parse(pos[1]);
                variables.Add(name, val);
                var acs = new AwaitableCompletionSource();
                acs.SetResult();
                return acs.Awaitable;
            }));

            var presenter = new TestTextPresenter();
            var runner = new ReplayRunner(variables, commands, presenter);

            // 行2 (アリス: ここまで到達) までリプレイシミュレーション
            await runner.FastForwardToLineAsync(graph, script.Id, 2, CancellationToken.None);

            // 10 + 20 = 30 になっていること (50は加算されていない)
            Assert.AreEqual(30, variables.GetInt("coins"));
        }
    }
}

