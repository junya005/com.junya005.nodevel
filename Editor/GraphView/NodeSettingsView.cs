using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Editor
{
    /// <summary>
    /// 選択中ノードの詳細プロパティ編集およびグラフ設定を表示する UI コンポーネント。
    /// </summary>
    public class NodeSettingsView : VisualElement
    {
        private NodevelGraph _graph;
        private NodevelNodeView _selectedNodeView;
        private readonly Action<NodevelNodeView> _onNodeModified;
        private readonly ScrollView _contentScrollView;

        public NodeSettingsView(NodevelGraph graph, Action<NodevelNodeView> onNodeModified = null)
        {
            _graph = graph;
            _onNodeModified = onNodeModified;

            style.flexGrow = 1;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;
            style.paddingBottom = 8;

            _contentScrollView = new ScrollView(ScrollViewMode.Vertical);
            _contentScrollView.style.flexGrow = 1;
            Add(_contentScrollView);

            Refresh();
        }

        public void SetGraph(NodevelGraph graph)
        {
            _graph = graph;
            _selectedNodeView = null;
            Refresh();
        }

        public void SetSelectedNode(NodevelNodeView nodeView)
        {
            _selectedNodeView = nodeView;
            Refresh();
        }

        public void Refresh()
        {
            _contentScrollView.Clear();

            if (_selectedNodeView == null || _selectedNodeView.TargetNode == null)
            {
                RenderGraphSettings();
            }
            else
            {
                RenderNodeProperties(_selectedNodeView);
            }
        }

        private void RenderGraphSettings()
        {
            var header = new Label("Graph Settings");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.fontSize = 13;
            header.style.marginBottom = 8;
            _contentScrollView.Add(header);

            if (_graph == null)
            {
                var noGraph = new Label("グラフアセットが読み込まれていません。");
                noGraph.style.color = new StyleColor(Color.gray);
                _contentScrollView.Add(noGraph);
                return;
            }

            var nameLabel = new Label($"アセット名: {_graph.name}");
            nameLabel.style.marginBottom = 4;
            _contentScrollView.Add(nameLabel);

            var nodeCount = new Label($"ノード数: {_graph.Nodes.Count}");
            nodeCount.style.marginBottom = 4;
            _contentScrollView.Add(nodeCount);

            var edgeCount = new Label($"接続 (エッジ) 数: {_graph.Edges.Count}");
            edgeCount.style.marginBottom = 4;
            _contentScrollView.Add(edgeCount);

            var varCount = new Label($"Blackboard 変数数: {_graph.Variables.Count}");
            varCount.style.marginBottom = 12;
            _contentScrollView.Add(varCount);

            var tipBox = new VisualElement();
            tipBox.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f));
            tipBox.style.borderTopLeftRadius = 4;
            tipBox.style.borderTopRightRadius = 4;
            tipBox.style.borderBottomLeftRadius = 4;
            tipBox.style.borderBottomRightRadius = 4;
            tipBox.style.paddingLeft = 8;
            tipBox.style.paddingRight = 8;
            tipBox.style.paddingTop = 8;
            tipBox.style.paddingBottom = 8;

            var tipText = new Label("💡 ヒント:\nキャンバス上のノードをクリックすると、そのノードの専用プロパティ（スクリプト、条件式、選択肢など）をここで編集できます。");
            tipText.style.fontSize = 11;
            tipText.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            tipText.style.whiteSpace = WhiteSpace.Normal;
            tipBox.Add(tipText);

            _contentScrollView.Add(tipBox);
        }

        private void RenderNodeProperties(NodevelNodeView nodeView)
        {
            var node = nodeView.TargetNode;

            // ノードタイトル
            var titleHeader = new Label($"Node: {node.GetType().Name}");
            titleHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleHeader.style.fontSize = 13;
            titleHeader.style.marginBottom = 6;
            _contentScrollView.Add(titleHeader);

            var titleField = new TextField("Title") { value = node.Title ?? "" };
            titleField.style.marginBottom = 8;
            titleField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(node, "Change Node Title");
                node.Title = evt.newValue;
                EditorUtility.SetDirty(node);
                nodeView.UpdateSummary();
                _onNodeModified?.Invoke(nodeView);
            });
            _contentScrollView.Add(titleField);

            var idLabel = new Label($"GUID: {node.Id}");
            idLabel.style.fontSize = 10;
            idLabel.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f));
            idLabel.style.marginBottom = 12;
            _contentScrollView.Add(idLabel);

            // 区切り線
            var divider = new VisualElement();
            divider.style.height = 1;
            divider.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            divider.style.marginBottom = 10;
            _contentScrollView.Add(divider);

            // 種別ごとのインスペクタ
            if (node is ScriptNode scriptNode)
            {
                RenderScriptNodeInspector(scriptNode, nodeView);
            }
            else if (node is ChoiceNode choiceNode)
            {
                RenderChoiceNodeInspector(choiceNode, nodeView);
            }
            else if (node is ConditionNode condNode)
            {
                RenderConditionNodeInspector(condNode, nodeView);
            }
            else if (node is SetVariableNode setVarNode)
            {
                RenderSetVariableNodeInspector(setVarNode, nodeView);
            }
            else if (node is SubGraphNode subGraphNode)
            {
                RenderSubGraphNodeInspector(subGraphNode, nodeView);
            }
        }

        private void RenderScriptNodeInspector(ScriptNode scriptNode, NodevelNodeView nodeView)
        {
            var label = new Label("Script DSL (会話・地の文・演出コマンド):");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 4;
            _contentScrollView.Add(label);

            var errorLabel = new Label();
            errorLabel.style.color = new StyleColor(new Color(1f, 0.35f, 0.35f));
            errorLabel.style.fontSize = 11;
            errorLabel.style.whiteSpace = WhiteSpace.Normal;
            errorLabel.style.marginBottom = 6;
            errorLabel.style.display = DisplayStyle.None;
            _contentScrollView.Add(errorLabel);

            var scriptField = new TextField
            {
                multiline = true,
                value = scriptNode.ScriptText ?? ""
            };
            scriptField.style.minHeight = 160;
            scriptField.style.unityFont = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Courier New" }, 12);
            scriptField.style.fontSize = 12;

            void ValidateScript(string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    errorLabel.style.display = DisplayStyle.None;
                    return;
                }

                try
                {
                    ScriptParser.Parse(text);
                    errorLabel.style.display = DisplayStyle.None;
                }
                catch (NovelScriptException ex)
                {
                    errorLabel.text = $"⚠️ 行 {ex.LineNumber}: {ex.Message}";
                    errorLabel.style.display = DisplayStyle.Flex;
                }
                catch (Exception ex)
                {
                    errorLabel.text = $"⚠️ 構文エラー: {ex.Message}";
                    errorLabel.style.display = DisplayStyle.Flex;
                }
            }

            ValidateScript(scriptField.value);

            scriptField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(scriptNode, "Edit Script Text");
                scriptNode.ScriptText = evt.newValue;
                EditorUtility.SetDirty(scriptNode);
                ValidateScript(evt.newValue);
                nodeView.UpdateSummary();
                _onNodeModified?.Invoke(nodeView);
            });

            _contentScrollView.Add(scriptField);
        }

        private void RenderChoiceNodeInspector(ChoiceNode choiceNode, NodevelNodeView nodeView)
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 6;

            var label = new Label("Choices (選択肢リスト):");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(label);

            var addBtn = new Button(() =>
            {
                Undo.RecordObject(choiceNode, "Add Choice");
                if (choiceNode.Choices == null) return;
                var index = choiceNode.Choices.Count + 1;
                choiceNode.Choices.Add($"選択肢 {index}");
                EditorUtility.SetDirty(choiceNode);
                nodeView.RebuildChoicePorts(choiceNode);
                nodeView.UpdateSummary();
                Refresh();
                _onNodeModified?.Invoke(nodeView);
            })
            {
                text = "+ Add Choice"
            };
            header.Add(addBtn);
            _contentScrollView.Add(header);

            var choices = choiceNode.Choices;
            if (choices == null || choices.Count == 0)
            {
                var emptyLabel = new Label("選択肢がありません。'+ Add Choice' で追加してください。");
                emptyLabel.style.color = new StyleColor(Color.gray);
                _contentScrollView.Add(emptyLabel);
                return;
            }

            for (var i = 0; i < choices.Count; i++)
            {
                var index = i;
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;

                var numLabel = new Label($"#{index + 1}");
                numLabel.style.minWidth = 24;
                row.Add(numLabel);

                var tf = new TextField { value = choices[index] };
                tf.style.flexGrow = 1;
                tf.style.marginRight = 4;
                tf.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(choiceNode, "Edit Choice Text");
                    choices[index] = evt.newValue;
                    EditorUtility.SetDirty(choiceNode);
                    nodeView.RebuildChoicePorts(choiceNode);
                    nodeView.UpdateSummary();
                    _onNodeModified?.Invoke(nodeView);
                });
                row.Add(tf);

                var delBtn = new Button(() =>
                {
                    Undo.RecordObject(choiceNode, "Delete Choice");
                    choices.RemoveAt(index);
                    EditorUtility.SetDirty(choiceNode);
                    nodeView.RebuildChoicePorts(choiceNode);
                    nodeView.UpdateSummary();
                    Refresh();
                    _onNodeModified?.Invoke(nodeView);
                })
                {
                    text = "×"
                };
                delBtn.style.width = 24;
                delBtn.style.height = 20;
                row.Add(delBtn);

                _contentScrollView.Add(row);
            }
        }

        private void RenderConditionNodeInspector(ConditionNode condNode, NodevelNodeView nodeView)
        {
            var label = new Label("Condition (条件式):");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 6;
            _contentScrollView.Add(label);

            // Blackboard変数からのドロップダウン選択 + 自由入力
            var varNames = GetBlackboardVariableNames();
            if (varNames.Count > 0 && !varNames.Contains("(直接入力)"))
            {
                varNames.Add("(直接入力)");
            }

            var selectedVar = condNode.VariableName ?? "";
            var varIndex = varNames.IndexOf(selectedVar);

            var varDropdown = new PopupField<string>("Variable", varNames, varIndex >= 0 ? varIndex : varNames.Count - 1);
            var manualField = new TextField("Variable Name") { value = condNode.VariableName ?? "" };
            manualField.style.display = varIndex >= 0 && varIndex < varNames.Count - 1 ? DisplayStyle.None : DisplayStyle.Flex;

            varDropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == "(直接入力)")
                {
                    manualField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    manualField.style.display = DisplayStyle.None;
                    manualField.value = evt.newValue;
                    Undo.RecordObject(condNode, "Change Condition Variable");
                    condNode.VariableName = evt.newValue;
                    EditorUtility.SetDirty(condNode);
                    nodeView.UpdateSummary();
                    _onNodeModified?.Invoke(nodeView);
                }
            });
            _contentScrollView.Add(varDropdown);

            manualField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(condNode, "Change Condition Variable");
                condNode.VariableName = evt.newValue;
                EditorUtility.SetDirty(condNode);
                nodeView.UpdateSummary();
                _onNodeModified?.Invoke(nodeView);
            });
            _contentScrollView.Add(manualField);

            // 演算子ドロップダウン
            var operators = new List<string> { "==", "!=", ">", ">=", "<", "<=" };
            var opIndex = operators.IndexOf(condNode.Operator ?? "==");
            if (opIndex < 0) opIndex = 0;

            var opDropdown = new PopupField<string>("Operator", operators, opIndex);
            opDropdown.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(condNode, "Change Condition Operator");
                condNode.Operator = evt.newValue;
                EditorUtility.SetDirty(condNode);
                nodeView.UpdateSummary();
                _onNodeModified?.Invoke(nodeView);
            });
            _contentScrollView.Add(opDropdown);

            // 比較値
            var valField = new TextField("Target Value") { value = condNode.RightValue ?? "" };
            valField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(condNode, "Change Condition Target Value");
                condNode.RightValue = evt.newValue;
                EditorUtility.SetDirty(condNode);
                nodeView.UpdateSummary();
                _onNodeModified?.Invoke(nodeView);
            });
            _contentScrollView.Add(valField);
        }

        private void RenderSetVariableNodeInspector(SetVariableNode setVarNode, NodevelNodeView nodeView)
        {
            var label = new Label("Set Variable (変数代入・更新):");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 6;
            _contentScrollView.Add(label);

            var varNames = GetBlackboardVariableNames();
            if (varNames.Count > 0 && !varNames.Contains("(直接入力)"))
            {
                varNames.Add("(直接入力)");
            }

            var selectedVar = setVarNode.VariableName ?? "";
            var varIndex = varNames.IndexOf(selectedVar);

            var varDropdown = new PopupField<string>("Variable", varNames, varIndex >= 0 ? varIndex : varNames.Count - 1);
            var manualField = new TextField("Variable Name") { value = setVarNode.VariableName ?? "" };
            manualField.style.display = varIndex >= 0 && varIndex < varNames.Count - 1 ? DisplayStyle.None : DisplayStyle.Flex;

            varDropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == "(直接入力)")
                {
                    manualField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    manualField.style.display = DisplayStyle.None;
                    manualField.value = evt.newValue;
                    Undo.RecordObject(setVarNode, "Change SetVariable Name");
                    setVarNode.VariableName = evt.newValue;
                    EditorUtility.SetDirty(setVarNode);
                    nodeView.UpdateSummary();
                    _onNodeModified?.Invoke(nodeView);
                }
            });
            _contentScrollView.Add(varDropdown);

            manualField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(setVarNode, "Change SetVariable Name");
                setVarNode.VariableName = evt.newValue;
                EditorUtility.SetDirty(setVarNode);
                nodeView.UpdateSummary();
                _onNodeModified?.Invoke(nodeView);
            });
            _contentScrollView.Add(manualField);

            var valField = new TextField("Value") { value = setVarNode.Value ?? "" };
            valField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(setVarNode, "Change SetVariable Value");
                setVarNode.Value = evt.newValue;
                EditorUtility.SetDirty(setVarNode);
                nodeView.UpdateSummary();
                _onNodeModified?.Invoke(nodeView);
            });
            _contentScrollView.Add(valField);
        }

        private void RenderSubGraphNodeInspector(SubGraphNode subGraphNode, NodevelNodeView nodeView)
        {
            var label = new Label("SubGraph (サブグラフ呼び出し):");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 6;
            _contentScrollView.Add(label);

            var graphField = new ObjectField("Graph Asset")
            {
                objectType = typeof(NodevelGraph),
                allowSceneObjects = false,
                value = subGraphNode.SubGraph
            };
            graphField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(subGraphNode, "Change SubGraph Asset");
                subGraphNode.SubGraph = evt.newValue as NodevelGraph;
                EditorUtility.SetDirty(subGraphNode);
                nodeView.UpdateSummary();
                _onNodeModified?.Invoke(nodeView);
            });
            _contentScrollView.Add(graphField);
        }

        private List<string> GetBlackboardVariableNames()
        {
            var list = new List<string>();
            if (_graph != null && _graph.Variables != null)
            {
                for (var i = 0; i < _graph.Variables.Count; i++)
                {
                    var name = _graph.Variables[i].Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        list.Add(name);
                    }
                }
            }
            return list;
        }
    }
}
