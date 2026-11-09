using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Editor
{
    /// <summary>
    /// GraphView 上にフロート表示される、Node Settings と Blackboard を内包したプロパティインスペクタパネル。
    /// ドラッグによる移動、タブ切り替え、開閉トグルに対応します。
    /// </summary>
    public class GraphInspectorPanel : VisualElement
    {
        private const string PrefsPrefix = "Nodevel_Inspector_";

        private readonly VisualElement _header;
        private readonly VisualElement _contentContainer;
        private readonly Button _tabNodeBtn;
        private readonly Button _tabBlackboardBtn;
        private readonly Button _collapseBtn;

        private readonly NodeSettingsView _nodeSettingsView;
        private readonly BlackboardView _blackboardView;

        private bool _isCollapsed;
        private bool _isDragging;
        private Vector2 _dragStartMousePos;
        private Vector2 _dragStartPanelPos;

        public NodeSettingsView NodeSettings => _nodeSettingsView;
        public BlackboardView Blackboard => _blackboardView;

        public GraphInspectorPanel(NodevelGraph graph, Action<NodevelNodeView> onNodeModified = null)
        {
            style.position = Position.Absolute;
            style.backgroundColor = new StyleColor(new Color(0.14f, 0.14f, 0.14f, 0.95f));
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftColor = new StyleColor(new Color(0.28f, 0.28f, 0.28f));
            style.borderRightColor = new StyleColor(new Color(0.28f, 0.28f, 0.28f));
            style.borderTopColor = new StyleColor(new Color(0.28f, 0.28f, 0.28f));
            style.borderBottomColor = new StyleColor(new Color(0.28f, 0.28f, 0.28f));
            style.borderTopLeftRadius = 6;
            style.borderTopRightRadius = 6;
            style.borderBottomLeftRadius = 6;
            style.borderBottomRightRadius = 6;
            style.minWidth = 280;
            style.minHeight = 220;
            style.width = 340;
            style.height = 420;

            // ヘッダー（ドラッグバー & タブ）
            _header = new VisualElement();
            _header.style.flexDirection = FlexDirection.Row;
            _header.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            _header.style.paddingLeft = 6;
            _header.style.paddingRight = 6;
            _header.style.paddingTop = 4;
            _header.style.paddingBottom = 4;
            _header.style.alignItems = Align.Center;
            _header.style.justifyContent = Justify.SpaceBetween;
            _header.style.borderTopLeftRadius = 5;
            _header.style.borderTopRightRadius = 5;

            var tabRow = new VisualElement();
            tabRow.style.flexDirection = FlexDirection.Row;

            _tabNodeBtn = new Button(() => SelectTab(0)) { text = "Node Settings" };
            _tabNodeBtn.style.fontSize = 11;
            _tabNodeBtn.style.marginRight = 2;

            _tabBlackboardBtn = new Button(() => SelectTab(1)) { text = "Blackboard" };
            _tabBlackboardBtn.style.fontSize = 11;

            tabRow.Add(_tabNodeBtn);
            tabRow.Add(_tabBlackboardBtn);
            _header.Add(tabRow);

            _collapseBtn = new Button(ToggleCollapse) { text = "−" };
            _collapseBtn.style.width = 24;
            _collapseBtn.style.height = 20;
            _header.Add(_collapseBtn);

            Add(_header);

            // コンテンツコンテナ
            _contentContainer = new VisualElement();
            _contentContainer.style.flexGrow = 1;

            _nodeSettingsView = new NodeSettingsView(graph, onNodeModified);
            _blackboardView = new BlackboardView(graph, () => _nodeSettingsView.Refresh());

            _contentContainer.Add(_nodeSettingsView);
            _contentContainer.Add(_blackboardView);
            Add(_contentContainer);

            // ドラッグ操作イベントの登録
            _header.RegisterCallback<MouseDownEvent>(OnHeaderMouseDown);
            _header.RegisterCallback<MouseMoveEvent>(OnHeaderMouseMove);
            _header.RegisterCallback<MouseUpEvent>(OnHeaderMouseUp);

            SelectTab(0);
            RestorePosition();
        }

        public void SetGraph(NodevelGraph graph)
        {
            _nodeSettingsView.SetGraph(graph);
            _blackboardView.SetGraph(graph);
        }

        public void SetSelectedNode(NodevelNodeView nodeView)
        {
            _nodeSettingsView.SetSelectedNode(nodeView);
            if (nodeView != null)
            {
                SelectTab(0); // ノード選択時は自動的に Node Settings タブを表示
            }
        }

        private void SelectTab(int index)
        {
            if (index == 0)
            {
                _nodeSettingsView.style.display = DisplayStyle.Flex;
                _blackboardView.style.display = DisplayStyle.None;
                _tabNodeBtn.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
                _tabBlackboardBtn.style.backgroundColor = StyleKeyword.Null;
            }
            else
            {
                _nodeSettingsView.style.display = DisplayStyle.None;
                _blackboardView.style.display = DisplayStyle.Flex;
                _blackboardView.RefreshList();
                _tabNodeBtn.style.backgroundColor = StyleKeyword.Null;
                _tabBlackboardBtn.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            }
        }

        private void ToggleCollapse()
        {
            _isCollapsed = !_isCollapsed;
            if (_isCollapsed)
            {
                _contentContainer.style.display = DisplayStyle.None;
                style.height = 30;
                _collapseBtn.text = "+";
            }
            else
            {
                _contentContainer.style.display = DisplayStyle.Flex;
                style.height = 420;
                _collapseBtn.text = "−";
            }
        }

        private void OnHeaderMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;
            _isDragging = true;
            _dragStartMousePos = evt.mousePosition;
            _dragStartPanelPos = new Vector2(resolvedStyle.left, resolvedStyle.top);
            _header.CaptureMouse();
            evt.StopPropagation();
        }

        private void OnHeaderMouseMove(MouseMoveEvent evt)
        {
            if (!_isDragging) return;
            var delta = evt.mousePosition - _dragStartMousePos;
            var newLeft = Mathf.Max(10, _dragStartPanelPos.x + delta.x);
            var newTop = Mathf.Max(30, _dragStartPanelPos.y + delta.y);

            style.left = newLeft;
            style.top = newTop;
            style.right = StyleKeyword.Auto;
            evt.StopPropagation();
        }

        private void OnHeaderMouseUp(MouseUpEvent evt)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _header.ReleaseMouse();
            SavePosition();
            evt.StopPropagation();
        }

        public void SavePosition()
        {
            EditorPrefs.SetFloat(PrefsPrefix + "Left", resolvedStyle.left);
            EditorPrefs.SetFloat(PrefsPrefix + "Top", resolvedStyle.top);
        }

        public void RestorePosition()
        {
            if (EditorPrefs.HasKey(PrefsPrefix + "Left"))
            {
                var left = EditorPrefs.GetFloat(PrefsPrefix + "Left", 100);
                var top = EditorPrefs.GetFloat(PrefsPrefix + "Top", 50);
                style.left = left;
                style.top = top;
                style.right = StyleKeyword.Auto;
            }
            else
            {
                style.right = 20;
                style.top = 40;
            }
        }
    }
}
