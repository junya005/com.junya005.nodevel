using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Samples
{
    /// <summary>
    /// Nodevel ノードグラフをシーン内で実行するエントリーポイントサンプル。
    /// </summary>
    public class BasicNovelGameRunner : MonoBehaviour
    {
        [Header("シナリオアセット")]
        [SerializeField] private NodevelGraph _startGraph;

        [Header("View プレゼンター")]
        [SerializeField] private BasicTextPresenter _textPresenter;
        [SerializeField] private BasicChoicePresenter _choicePresenter;

        [Header("演出ターゲット")]
        [SerializeField] private Image _backgroundImage;

        private VariableStore _variables;
        private CommandRegistry _commands;
        private GraphRunner _runner;
        private CancellationTokenSource _cts;

        public NodevelGraph StartGraph
        {
            get => _startGraph;
            set => _startGraph = value;
        }

        public BasicTextPresenter TextPresenter
        {
            get => _textPresenter;
            set => _textPresenter = value;
        }

        public BasicChoicePresenter ChoicePresenter
        {
            get => _choicePresenter;
            set => _choicePresenter = value;
        }

        public Image BackgroundImage
        {
            get => _backgroundImage;
            set => _backgroundImage = value;
        }

        private async void Start()
        {
            if (_startGraph == null)
            {
                Debug.LogWarning("[Nodevel] 開始グラフが設定されていません。");
                return;
            }

            _variables = new VariableStore();
            _commands = new CommandRegistry();

            // このMonoBehaviour自身内の [NovelCommand] を登録
            _commands.RegisterTarget(this);

            _runner = new GraphRunner(_variables, _commands, _textPresenter, _choicePresenter);
            _cts = new CancellationTokenSource();

            try
            {
                Debug.Log($"[Nodevel] シナリオ '{_startGraph.name}' の実行を開始します。");
                await _runner.ExecuteAsync(_startGraph, _cts.Token);
                Debug.Log($"[Nodevel] シナリオ '{_startGraph.name}' が完了しました。");
            }
            catch (global::System.Exception ex)
            {
                Debug.LogError($"[Nodevel] 実行時エラー: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        [NovelCommand("bg")]
        public async Awaitable OnBackground(string bgName, float fade = 0.5f, CancellationToken ct = default)
        {
            Debug.Log($"[Command: @bg] 背景を変更: {bgName} (fade={fade}s)");

            if (_backgroundImage != null)
            {
                Color targetColor;
                switch (bgName.ToLowerInvariant())
                {
                    case "classroom":
                        targetColor = new Color(0.2f, 0.35f, 0.55f, 1f); // 教室: 落ち着いたブルー
                        break;
                    case "library":
                        targetColor = new Color(0.25f, 0.45f, 0.3f, 1f); // 図書室: 深いグリーン
                        break;
                    case "rooftop":
                        targetColor = new Color(0.35f, 0.6f, 0.85f, 1f); // 屋上: 澄んだスカイブルー
                        break;
                    default:
                        targetColor = new Color(0.15f, 0.15f, 0.2f, 1f);
                        break;
                }

                _backgroundImage.color = targetColor;
            }

            if (fade > 0)
            {
                await Awaitable.WaitForSecondsAsync(fade, ct);
            }
        }

        [NovelCommand("se")]
        public void OnSoundEffect(string seName)
        {
            Debug.Log($"[Command: @se] 効果音を再生: {seName}");
        }
    }
}
