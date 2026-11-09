using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Tests
{
    [TestFixture]
    public class GraphRunnerTests
    {
        private class MockTextPresenter : INovelTextPresenter
        {
            public List<string> DisplayedLines = new List<string>();

            public Awaitable ShowTextAsync(ScriptDialogueLine line, CancellationToken ct)
            {
                DisplayedLines.Add($"{line.Speaker}: {line.CleanText}");
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

        private class MockChoicePresenter : INovelChoicePresenter
        {
            public int ChoiceToSelect = 0;
            public List<string> LastChoices;

            public Awaitable<int> ShowChoicesAsync(IReadOnlyList<string> choiceTexts, CancellationToken ct)
            {
                LastChoices = new List<string>(choiceTexts);
                var acs = new AwaitableCompletionSource<int>();
                acs.SetResult(ChoiceToSelect);
                return acs.Awaitable;
            }
        }

        private VariableStore _variables;
        private CommandRegistry _commands;
        private MockTextPresenter _textPresenter;
        private MockChoicePresenter _choicePresenter;
        private GraphRunner _runner;

        [SetUp]
        public void SetUp()
        {
            _variables = new VariableStore();
            _commands = new CommandRegistry();
            _textPresenter = new MockTextPresenter();
            _choicePresenter = new MockChoicePresenter();
            _runner = new GraphRunner(_variables, _commands, _textPresenter, _choicePresenter);
        }

        [Test]
        public async Task ExecuteAsync_LinearGraph_ExecutesNodesInSequence()
        {
            var graph = ScriptableObject.CreateInstance<NodevelGraph>();

            var start = graph.CreateNode<StartNode>("start");
            var script = graph.CreateNode<ScriptNode>("script1");
            script.ScriptText = "アリス: こんにちは！\nナレーション文章。";
            var end = graph.CreateNode<EndNode>("end");

            graph.Connect(start.Id, 0, script.Id, 0);
            graph.Connect(script.Id, 0, end.Id, 0);

            await _runner.ExecuteAsync(graph, CancellationToken.None);

            Assert.AreEqual(2, _textPresenter.DisplayedLines.Count);
            Assert.AreEqual("アリス: こんにちは！", _textPresenter.DisplayedLines[0]);
            Assert.AreEqual(": ナレーション文章。", _textPresenter.DisplayedLines[1]);
        }

        [Test]
        public async Task ExecuteAsync_ConditionNode_BranchesCorrectly()
        {
            var graph = ScriptableObject.CreateInstance<NodevelGraph>();

            var start = graph.CreateNode<StartNode>("start");
            var cond = graph.CreateNode<ConditionNode>("cond");
            cond.VariableName = "score";
            cond.Operator = ">=";
            cond.RightValue = "50";

            var scriptTrue = graph.CreateNode<ScriptNode>("script_true");
            scriptTrue.ScriptText = "合格！";
            var scriptFalse = graph.CreateNode<ScriptNode>("script_false");
            scriptFalse.ScriptText = "不合格…";

            var end = graph.CreateNode<EndNode>("end");

            graph.Connect(start.Id, 0, cond.Id, 0);
            graph.Connect(cond.Id, 0, scriptTrue.Id, 0);   // Port 0: True
            graph.Connect(cond.Id, 1, scriptFalse.Id, 0);  // Port 1: False
            graph.Connect(scriptTrue.Id, 0, end.Id, 0);
            graph.Connect(scriptFalse.Id, 0, end.Id, 0);

            // Case 1: score = 80 -> True
            _variables.Set("score", 80);
            await _runner.ExecuteAsync(graph, CancellationToken.None);
            Assert.AreEqual(1, _textPresenter.DisplayedLines.Count);
            Assert.AreEqual(": 合格！", _textPresenter.DisplayedLines[0]);

            // Case 2: score = 30 -> False
            _textPresenter.DisplayedLines.Clear();
            _variables.Set("score", 30);
            await _runner.ExecuteAsync(graph, CancellationToken.None);
            Assert.AreEqual(1, _textPresenter.DisplayedLines.Count);
            Assert.AreEqual(": 不合格…", _textPresenter.DisplayedLines[0]);
        }

        [Test]
        public async Task ExecuteAsync_ChoiceNode_BranchesToSelectedPort()
        {
            var graph = ScriptableObject.CreateInstance<NodevelGraph>();

            var start = graph.CreateNode<StartNode>("start");
            var choice = graph.CreateNode<ChoiceNode>("choice");
            choice.Choices.Add("右へ行く");
            choice.Choices.Add("左へ行く");

            var scriptRight = graph.CreateNode<ScriptNode>("script_right");
            scriptRight.ScriptText = "宝箱を見つけた！";
            var scriptLeft = graph.CreateNode<ScriptNode>("script_left");
            scriptLeft.ScriptText = "罠にかかった！";

            var end = graph.CreateNode<EndNode>("end");

            graph.Connect(start.Id, 0, choice.Id, 0);
            graph.Connect(choice.Id, 0, scriptRight.Id, 0); // Port 0: 右
            graph.Connect(choice.Id, 1, scriptLeft.Id, 0);  // Port 1: 左
            graph.Connect(scriptRight.Id, 0, end.Id, 0);
            graph.Connect(scriptLeft.Id, 0, end.Id, 0);

            // プレイヤーが「左へ行く (Index 1)」を選択
            _choicePresenter.ChoiceToSelect = 1;
            await _runner.ExecuteAsync(graph, CancellationToken.None);

            Assert.AreEqual(1, _textPresenter.DisplayedLines.Count);
            Assert.AreEqual(": 罠にかかった！", _textPresenter.DisplayedLines[0]);
        }

        [Test]
        public async Task ExecuteAsync_SetVariableNode_ModifiesVariables()
        {
            var graph = ScriptableObject.CreateInstance<NodevelGraph>();

            var start = graph.CreateNode<StartNode>("start");
            var setVar = graph.CreateNode<SetVariableNode>("set_var");
            setVar.VariableName = "coins";
            setVar.Operation = VariableOperation.Add;
            setVar.Value = "100";
            var end = graph.CreateNode<EndNode>("end");

            graph.Connect(start.Id, 0, setVar.Id, 0);
            graph.Connect(setVar.Id, 0, end.Id, 0);

            _variables.Set("coins", 50);
            await _runner.ExecuteAsync(graph, CancellationToken.None);

            Assert.AreEqual(150, _variables.GetInt("coins"));
        }
    }
}
