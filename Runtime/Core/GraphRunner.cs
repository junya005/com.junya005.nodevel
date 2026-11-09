using System;
using System.Threading;
using UnityEngine;
namespace Junya005.Nodevel
{
    /// <summary>
    /// ノードグラフのフロー制御およびスクリプト実行を司るランタイムエンジン。
    /// </summary>
    public class GraphRunner
    {
        private readonly VariableStore _variables;
        private readonly CommandRegistry _commands;
        private readonly INovelTextPresenter _textPresenter;
        private readonly INovelChoicePresenter _choicePresenter;

        public event Action<NodevelGraph, NodeBase> OnNodeEntered;
        public event Action<NodevelGraph, NodeBase> OnNodeExited;

        public GraphRunner(
            VariableStore variables,
            CommandRegistry commands,
            INovelTextPresenter textPresenter = null,
            INovelChoicePresenter choicePresenter = null)
        {
            _variables = variables ?? throw new ArgumentNullException(nameof(variables));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _textPresenter = textPresenter;
            _choicePresenter = choicePresenter;
        }

        public async Awaitable ExecuteAsync(NodevelGraph graph, CancellationToken ct)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var startNode = graph.GetStartNode();
            if (startNode == null)
            {
                throw new NovelScriptException($"グラフ '{graph.name}' に StartNode が存在しません。");
            }

            // Blackboard で定義された変数のうち、まだVariableStoreに存在しないものを初期値で登録
            if (graph.Variables != null)
            {
                for (var i = 0; i < graph.Variables.Count; i++)
                {
                    var variable = graph.Variables[i];
                    if (!string.IsNullOrEmpty(variable.Name) && !_variables.Has(variable.Name))
                    {
                        _variables.Set(variable.Name, variable.ToVariableValue());
                    }
                }
            }

            NodeBase currentNode = startNode;

            while (currentNode != null && !ct.IsCancellationRequested)
            {
                OnNodeEntered?.Invoke(graph, currentNode);

                var nextPort = 0;

                if (currentNode is StartNode)
                {
                    nextPort = 0;
                }
                else if (currentNode is ScriptNode scriptNode)
                {
                    await ExecuteScriptNodeAsync(scriptNode, ct);
                    nextPort = 0;
                }
                else if (currentNode is ConditionNode condNode)
                {
                    var isTrue = ConditionEvaluator.Evaluate(_variables, condNode.VariableName, condNode.Operator, condNode.RightValue);
                    nextPort = isTrue ? 0 : 1;
                }
                else if (currentNode is ChoiceNode choiceNode)
                {
                    if (_choicePresenter == null)
                    {
                        throw new NovelScriptException("ChoiceNode が実行されましたが、INovelChoicePresenter が登録されていません。");
                    }
                    var selectedIndex = await _choicePresenter.ShowChoicesAsync(choiceNode.Choices, ct);
                    nextPort = selectedIndex;
                }
                else if (currentNode is SetVariableNode setVarNode)
                {
                    ExecuteSetVariableNode(setVarNode);
                    nextPort = 0;
                }
                else if (currentNode is SubGraphNode subGraphNode)
                {
                    if (subGraphNode.SubGraph != null)
                    {
                        await ExecuteAsync(subGraphNode.SubGraph, ct);
                    }
                    nextPort = 0;
                }
                else if (currentNode is EndNode)
                {
                    OnNodeExited?.Invoke(graph, currentNode);
                    break;
                }

                OnNodeExited?.Invoke(graph, currentNode);
                currentNode = graph.GetNextNode(currentNode.Id, nextPort);
            }
        }

        private async Awaitable ExecuteScriptNodeAsync(ScriptNode node, CancellationToken ct)
        {
            var data = node.GetParsedData();
            if (data == null || data.Lines == null) return;

            foreach (var line in data.Lines)
            {
                if (ct.IsCancellationRequested) break;

                if (line is ScriptCommandLine cmdLine)
                {
                    await _commands.DispatchAsync(cmdLine.CommandName, cmdLine.PositionalArgs, cmdLine.NamedArgs, ct);
                }
                else if (line is ScriptDialogueLine dialogueLine)
                {
                    if (_textPresenter != null)
                    {
                        await _textPresenter.ShowTextAsync(dialogueLine, ct);
                    }
                }
            }
        }

        private void ExecuteSetVariableNode(SetVariableNode node)
        {
            switch (node.Operation)
            {
                case VariableOperation.Set:
                    if (bool.TryParse(node.Value, out var b))
                    {
                        _variables.Set(node.VariableName, b);
                    }
                    else if (int.TryParse(node.Value, out var i))
                    {
                        _variables.Set(node.VariableName, i);
                    }
                    else if (float.TryParse(node.Value, out var f))
                    {
                        _variables.Set(node.VariableName, f);
                    }
                    else
                    {
                        _variables.Set(node.VariableName, node.Value);
                    }
                    break;

                case VariableOperation.Add:
                    if (int.TryParse(node.Value, out var addInt))
                    {
                        _variables.Add(node.VariableName, addInt);
                    }
                    else if (float.TryParse(node.Value, out var addFloat))
                    {
                        _variables.Add(node.VariableName, addFloat);
                    }
                    break;

                case VariableOperation.Toggle:
                    _variables.Toggle(node.VariableName);
                    break;
            }
        }
    }
}

