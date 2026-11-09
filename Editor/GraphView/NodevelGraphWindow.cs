using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Editor
{
    /// <summary>
    /// Nodevel のノードグラフエディタウィンドウ。
    /// </summary>
    public class NodevelGraphWindow : EditorWindow
    {
        private const string PrefsInspectorVisible = "Nodevel_Inspector_Visible";

        private NodevelGraphView _graphView;
        private ObjectField _graphField;
        private GraphInspectorPanel _inspectorPanel;
        private ToolbarToggle _inspectorToggle;

        [MenuItem("Window/Nodevel/Graph Editor")]
        public static void Open()
        {
            var window = GetWindow<NodevelGraphWindow>("Nodevel Graph");
            window.minSize = new Vector2(700, 450);
            window.Show();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
#pragma warning disable CS0618
            var asset = EditorUtility.InstanceIDToObject(instanceId) as NodevelGraph;
#pragma warning restore CS0618
            if (asset != null)
            {
                var window = GetWindow<NodevelGraphWindow>("Nodevel Graph");
                window.minSize = new Vector2(700, 450);
                window.LoadGraph(asset);
                window.Show();
                return true;
            }
            return false;
        }

        private void OnEnable()
        {
            ConstructView();
        }

        private void OnDisable()
        {
            _inspectorPanel?.SavePosition();
            rootVisualElement.Clear();
        }

        private void ConstructView()
        {
            rootVisualElement.Clear();

            // ツールバー
            var toolbar = new Toolbar();
            _graphField = new ObjectField("Graph Asset")
            {
                objectType = typeof(NodevelGraph),
                allowSceneObjects = false
            };
            _graphField.RegisterValueChangedCallback(evt =>
            {
                LoadGraph(evt.newValue as NodevelGraph);
            });
            toolbar.Add(_graphField);

            var saveButton = new Button(() =>
            {
                AssetDatabase.SaveAssets();
            })
            {
                text = "Save Assets"
            };
            toolbar.Add(saveButton);

            var isVisible = EditorPrefs.GetBool(PrefsInspectorVisible, true);
            _inspectorToggle = new ToolbarToggle { text = "Inspector", value = isVisible };
            _inspectorToggle.RegisterValueChangedCallback(evt =>
            {
                if (_inspectorPanel != null)
                {
                    _inspectorPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
                    EditorPrefs.SetBool(PrefsInspectorVisible, evt.newValue);
                }
            });
            toolbar.Add(_inspectorToggle);

            rootVisualElement.Add(toolbar);

            // GraphView
            _graphView = new NodevelGraphView();
            rootVisualElement.Add(_graphView);

            // フローティングプロパティパネル
            _inspectorPanel = new GraphInspectorPanel(_graphView.CurrentGraph);
            _inspectorPanel.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            rootVisualElement.Add(_inspectorPanel);

            // ノード選択同期
            _graphView.OnNodeSelected += nodeView =>
            {
                _inspectorPanel.SetSelectedNode(nodeView);
            };
        }

        public void LoadGraph(NodevelGraph graph)
        {
            if (_graphField != null && _graphField.value != graph)
            {
                _graphField.value = graph;
            }
            _graphView?.PopulateView(graph);
            _inspectorPanel?.SetGraph(graph);
        }

        private void OnSelectionChange()
        {
            var selected = Selection.activeObject as NodevelGraph;
            if (selected != null && (_graphView == null || _graphView.CurrentGraph != selected))
            {
                LoadGraph(selected);
            }
        }
    }
}
