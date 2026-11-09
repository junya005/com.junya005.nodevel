using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;
using Junya005.Nodevel;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Junya005.Nodevel.Samples
{
    /// <summary>
    /// TextMeshPro を用いたシンプルな文字送り・クリック待ちの実装サンプル。
    /// </summary>
    public class BasicTextPresenter : MonoBehaviour, INovelTextPresenter
    {
        [SerializeField] private TextMeshProUGUI _speakerText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private float _charactersPerSecond = 30f;

        public TextMeshProUGUI SpeakerText
        {
            get => _speakerText;
            set => _speakerText = value;
        }

        public TextMeshProUGUI DialogueText
        {
            get => _dialogueText;
            set => _dialogueText = value;
        }

        private bool _isClicked;

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (keyboard == null || keyboard.spaceKey.wasPressedThisFrame || mouse.leftButton.wasPressedThisFrame)
            {
                _isClicked = true;
            }
#else
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                _isClicked = true;
            }
#endif
        }

        public async Awaitable ShowTextAsync(ScriptDialogueLine line, CancellationToken ct)
        {
            if (_speakerText != null)
            {
                _speakerText.text = line.Speaker ?? string.Empty;
                _speakerText.gameObject.SetActive(!string.IsNullOrEmpty(line.Speaker));
            }

            if (_dialogueText == null) return;

            var cleanText = line.CleanText;
            var sb = new StringBuilder();
            var delay = _charactersPerSecond > 0 ? 1f / _charactersPerSecond : 0f;

            _isClicked = false;

            // タイプライター表示
            for (var i = 0; i < cleanText.Length; i++)
            {
                if (ct.IsCancellationRequested) break;

                // クリックで一括表示
                if (_isClicked)
                {
                    _dialogueText.text = cleanText;
                    _isClicked = false;
                    break;
                }

                sb.Append(cleanText[i]);
                _dialogueText.text = sb.ToString();

                if (delay > 0)
                {
                    await Awaitable.WaitForSecondsAsync(delay, ct);
                }
            }

            _dialogueText.text = cleanText;

            // プレイヤーの次へのクリック待ち
            _isClicked = false;
            while (!_isClicked && !ct.IsCancellationRequested)
            {
                await Awaitable.NextFrameAsync(ct);
            }
            _isClicked = false;
        }

        public async Awaitable ClearAsync(CancellationToken ct)
        {
            if (_speakerText != null) _speakerText.text = string.Empty;
            if (_dialogueText != null) _dialogueText.text = string.Empty;
            await Awaitable.NextFrameAsync(ct);
        }
    }
}

