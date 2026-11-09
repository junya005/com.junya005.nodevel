using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Editor
{
    /// <summary>
    /// GraphView 上でのノード表示および要約プレビューを司るコンパクトなビュークラス。
    /// 詳細プロパティの編集は Node Settings パネル側で行われます。
    /// </summary>
    public class NodevelNodeView : Node
    {
        public NodeBase TargetNode { get; }

        public Port InputPort { get; private set; }
        public List<Port> OutputPorts { get; } = new List<Port>();

        public event Action<NodevelNodeView> OnViewSelected;
        public event Action<NodevelNodeView> OnViewUnselected;

        private Label _summaryLabel;

        public NodevelNodeView(NodeBase node)
        {
            TargetNode = node ?? throw new ArgumentNullException(nameof(node));
            title = string.IsNullOrEmpty(node.Title) ? node.GetType().Name : node.Title;
            viewDataKey = node.Id;

            SetPosition(new Rect(node.Position, new Vector2(180, 80)));

            CreateInputPorts();
            CreateOutputPorts();
            CreateSummaryControl();

            RefreshExpandedState();
            RefreshPorts();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnViewSelected?.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            OnViewUnselected?.Invoke(this);
        }

        private void CreateInputPorts()
        {
            if (TargetNode is StartNode) return; // StartNodeは入力ポートなし

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);
        }

        private void CreateOutputPorts()
        {
            if (TargetNode is EndNode) return; // EndNodeは出力ポートなし

            if (TargetNode is ConditionNode)
            {
                var truePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                truePort.portName = "True";
                outputContainer.Add(truePort);
                OutputPorts.Add(truePort);

                var falsePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                falsePort.portName = "False";
                outputContainer.Add(falsePort);
                OutputPorts.Add(falsePort);
            }
            else if (TargetNode is ChoiceNode choiceNode)
            {
                RebuildChoicePorts(choiceNode);
            }
            else
            {
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                port.portName = "Out";
                outputContainer.Add(port);
                OutputPorts.Add(port);
            }
        }

        /// <summary>
        /// ChoiceNode の選択肢数・名前に応じて出力ポートを再構築します。
        /// </summary>
        public void RebuildChoicePorts(ChoiceNode choiceNode)
        {
            outputContainer.Clear();
            OutputPorts.Clear();

            var count = choiceNode.OutputPortCount;
            for (var i = 0; i < count; i++)
            {
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                var label = (choiceNode.Choices != null && i < choiceNode.Choices.Count)
                    ? choiceNode.Choices[i]
                    : $"Choice {i}";
                port.portName = label;
                outputContainer.Add(port);
                OutputPorts.Add(port);
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        private void CreateSummaryControl()
        {
            _summaryLabel = new Label();
            _summaryLabel.style.fontSize = 11;
            _summaryLabel.style.color = new StyleColor(new Color(0.75f, 0.75f, 0.75f));
            _summaryLabel.style.paddingLeft = 6;
            _summaryLabel.style.paddingRight = 6;
            _summaryLabel.style.paddingTop = 4;
            _summaryLabel.style.paddingBottom = 4;
            _summaryLabel.style.whiteSpace = WhiteSpace.Normal;
            _summaryLabel.style.unityFontStyleAndWeight = FontStyle.Italic;

            extensionContainer.Add(_summaryLabel);
            UpdateSummary();
        }

        /// <summary>
        /// ノードの現在の設定値に基づいて要約プレビューラベルを更新します。
        /// </summary>
        public void UpdateSummary()
        {
            if (_summaryLabel == null) return;

            title = string.IsNullOrEmpty(TargetNode.Title) ? TargetNode.GetType().Name : TargetNode.Title;

            if (TargetNode is ScriptNode scriptNode)
            {
                if (string.IsNullOrWhiteSpace(scriptNode.ScriptText))
                {
                    _summaryLabel.text = "(空のスクリプト)";
                }
                else
                {
                    var firstLine = scriptNode.ScriptText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var text = firstLine.Length > 0 ? firstLine[0].Trim() : "";
                    if (text.Length > 28)
                    {
                        text = text.Substring(0, 25) + "...";
                    }
                    _summaryLabel.text = text;
                }
                _summaryLabel.style.display = DisplayStyle.Flex;
            }
            else if (TargetNode is ConditionNode condNode)
            {
                var varName = string.IsNullOrEmpty(condNode.VariableName) ? "?" : condNode.VariableName;
                var op = string.IsNullOrEmpty(condNode.Operator) ? "==" : condNode.Operator;
                var val = string.IsNullOrEmpty(condNode.RightValue) ? "?" : condNode.RightValue;
                _summaryLabel.text = $"[{varName} {op} {val}]";
                _summaryLabel.style.display = DisplayStyle.Flex;
            }
            else if (TargetNode is SetVariableNode setVarNode)
            {
                var varName = string.IsNullOrEmpty(setVarNode.VariableName) ? "?" : setVarNode.VariableName;
                var val = string.IsNullOrEmpty(setVarNode.Value) ? "?" : setVarNode.Value;
                _summaryLabel.text = $"[{varName} = {val}]";
                _summaryLabel.style.display = DisplayStyle.Flex;
            }
            else if (TargetNode is ChoiceNode choiceNode)
            {
                var count = choiceNode.Choices != null ? choiceNode.Choices.Count : 0;
                _summaryLabel.text = $"{count} 件の選択肢";
                _summaryLabel.style.display = DisplayStyle.Flex;
            }
            else if (TargetNode is SubGraphNode subGraphNode)
            {
                _summaryLabel.text = subGraphNode.SubGraph != null ? $"Graph: {subGraphNode.SubGraph.name}" : "(グラフ未設定)";
                _summaryLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                _summaryLabel.text = string.Empty;
                _summaryLabel.style.display = DisplayStyle.None;
            }
        }

        public void SetHighlight(bool active)
        {
            if (active)
            {
                style.borderTopColor = new StyleColor(Color.yellow);
                style.borderBottomColor = new StyleColor(Color.yellow);
                style.borderLeftColor = new StyleColor(Color.yellow);
                style.borderRightColor = new StyleColor(Color.yellow);
                style.borderTopWidth = 3;
                style.borderBottomWidth = 3;
                style.borderLeftWidth = 3;
                style.borderRightWidth = 3;
            }
            else
            {
                style.borderTopWidth = 0;
                style.borderBottomWidth = 0;
                style.borderLeftWidth = 0;
                style.borderRightWidth = 0;
            }
        }
    }
}
