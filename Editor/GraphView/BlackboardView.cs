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
    /// グラフ全体で使用される変数を一覧管理・定義する Blackboard UI コンポーネント。
    /// </summary>
    public class BlackboardView : VisualElement
    {
        private NodevelGraph _targetGraph;
        private readonly Action _onVariableChanged;
        private readonly ScrollView _variablesScrollView;

        public BlackboardView(NodevelGraph graph, Action onVariableChanged = null)
        {
            _targetGraph = graph;
            _onVariableChanged = onVariableChanged;

            style.flexGrow = 1;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 8;
            style.paddingBottom = 8;

            // ヘッダーバー
            var headerBar = new VisualElement();
            headerBar.style.flexDirection = FlexDirection.Row;
            headerBar.style.justifyContent = Justify.SpaceBetween;
            headerBar.style.alignItems = Align.Center;
            headerBar.style.marginBottom = 6;
            headerBar.style.paddingBottom = 4;
            headerBar.style.borderBottomWidth = 1;
            headerBar.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

            var titleLabel = new Label("Blackboard (Variables)");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 12;
            headerBar.Add(titleLabel);

            var addButton = new Button(OnAddVariableClicked)
            {
                text = "+ Add Variable"
            };
            addButton.style.fontSize = 11;
            headerBar.Add(addButton);

            Add(headerBar);

            // リスト表示領域
            _variablesScrollView = new ScrollView(ScrollViewMode.Vertical);
            _variablesScrollView.style.flexGrow = 1;
            Add(_variablesScrollView);

            RefreshList();
        }

        public void SetGraph(NodevelGraph graph)
        {
            _targetGraph = graph;
            RefreshList();
        }

        private void OnAddVariableClicked()
        {
            if (_targetGraph == null) return;

            Undo.RecordObject(_targetGraph, "Add Blackboard Variable");

            // 重複しないユニークな変数名を生成
            var baseName = "newVariable";
            var candidate = baseName;
            var counter = 1;
            while (_targetGraph.GetVariable(candidate) != null)
            {
                candidate = $"{baseName}_{counter++}";
            }

            var newVar = new GraphVariable(candidate, VariableType.Int, "0", "");
            _targetGraph.AddVariable(newVar);
            EditorUtility.SetDirty(_targetGraph);

            RefreshList();
            _onVariableChanged?.Invoke();
        }

        public void RefreshList()
        {
            _variablesScrollView.Clear();

            if (_targetGraph == null)
            {
                var emptyLabel = new Label("グラフが選択されていません。");
                emptyLabel.style.color = new StyleColor(Color.gray);
                emptyLabel.style.marginTop = 12;
                _variablesScrollView.Add(emptyLabel);
                return;
            }

            var variables = _targetGraph.Variables;
            if (variables == null || variables.Count == 0)
            {
                var noVarLabel = new Label("変数が定義されていません。\n'+ Add Variable' ボタンから追加できます。");
                noVarLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                noVarLabel.style.marginTop = 12;
                _variablesScrollView.Add(noVarLabel);
                return;
            }

            for (var i = 0; i < variables.Count; i++)
            {
                var v = variables[i];
                var card = CreateVariableCard(v);
                _variablesScrollView.Add(card);
            }
        }

        private VisualElement CreateVariableCard(GraphVariable variable)
        {
            var card = new VisualElement();
            card.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f));
            card.style.borderTopLeftRadius = 4;
            card.style.borderTopRightRadius = 4;
            card.style.borderBottomLeftRadius = 4;
            card.style.borderBottomRightRadius = 4;
            card.style.paddingLeft = 6;
            card.style.paddingRight = 6;
            card.style.paddingTop = 6;
            card.style.paddingBottom = 6;
            card.style.marginBottom = 6;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            card.style.borderBottomColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            card.style.borderLeftColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            card.style.borderRightColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));

            // 上段: 変数名・型・削除ボタン
            var topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.alignItems = Align.Center;
            topRow.style.marginBottom = 4;

            var nameField = new TextField { value = variable.Name };
            nameField.style.flexGrow = 1;
            nameField.style.marginRight = 4;

            // 型選択ドロップダウン
            var typeOptions = new List<string> { "Int", "Float", "String", "Bool" };
            var typeIndex = (int)variable.Type;
            if (typeIndex < 0 || typeIndex >= typeOptions.Count) typeIndex = 0;

            var typeField = new PopupField<string>(typeOptions, typeIndex);
            typeField.style.width = 65;
            typeField.style.marginRight = 4;
            typeField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_targetGraph, "Change Variable Type");
                var newType = (VariableType)typeOptions.IndexOf(evt.newValue);
                variable.Type = newType;
                // 型に応じたデフォルト値へ
                switch (newType)
                {
                    case VariableType.Bool: variable.InitialValue = "false"; break;
                    case VariableType.Int: variable.InitialValue = "0"; break;
                    case VariableType.Float: variable.InitialValue = "0.0"; break;
                    case VariableType.String: variable.InitialValue = ""; break;
                }
                EditorUtility.SetDirty(_targetGraph);
                RefreshList();
                _onVariableChanged?.Invoke();
            });

            var deleteButton = new Button(() =>
            {
                Undo.RecordObject(_targetGraph, "Delete Blackboard Variable");
                _targetGraph.RemoveVariable(variable.Name);
                EditorUtility.SetDirty(_targetGraph);
                RefreshList();
                _onVariableChanged?.Invoke();
            })
            {
                text = "×"
            };
            deleteButton.style.width = 24;
            deleteButton.style.height = 20;

            topRow.Add(nameField);
            topRow.Add(typeField);
            topRow.Add(deleteButton);
            card.Add(topRow);

            // 変数名バリデーション＆更新
            nameField.RegisterValueChangedCallback(evt =>
            {
                var newName = evt.newValue?.Trim();
                if (string.IsNullOrEmpty(newName))
                {
                    nameField.style.backgroundColor = new StyleColor(new Color(0.4f, 0.15f, 0.15f));
                    return;
                }

                // 重複チェック（自分以外に同名があるか）
                var existing = _targetGraph.GetVariable(newName);
                if (existing != null && existing != variable)
                {
                    nameField.style.backgroundColor = new StyleColor(new Color(0.4f, 0.15f, 0.15f));
                    return;
                }

                nameField.style.backgroundColor = StyleKeyword.Null;
                Undo.RecordObject(_targetGraph, "Rename Blackboard Variable");
                variable.Name = newName;
                EditorUtility.SetDirty(_targetGraph);
                _onVariableChanged?.Invoke();
            });

            // 下段: 初期値 & 説明
            var bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.alignItems = Align.Center;

            var valField = new TextField("Init:") { value = variable.InitialValue ?? "" };
            valField.style.flexGrow = 1;
            valField.style.marginRight = 4;
            valField.labelElement.style.minWidth = 30;
            valField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_targetGraph, "Change Variable Initial Value");
                variable.InitialValue = evt.newValue;
                EditorUtility.SetDirty(_targetGraph);
            });
            bottomRow.Add(valField);

            var descField = new TextField("Desc:") { value = variable.Description ?? "" };
            descField.style.flexGrow = 1;
            descField.labelElement.style.minWidth = 35;
            descField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_targetGraph, "Change Variable Description");
                variable.Description = evt.newValue;
                EditorUtility.SetDirty(_targetGraph);
            });
            bottomRow.Add(descField);

            card.Add(bottomRow);

            return card;
        }
    }
}
