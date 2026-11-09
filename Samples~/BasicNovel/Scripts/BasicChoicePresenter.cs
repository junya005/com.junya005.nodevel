using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Samples
{
    /// <summary>
    /// uGUI Button を動的に配置して選択肢を受け付ける実装サンプル。
    /// </summary>
    public class BasicChoicePresenter : MonoBehaviour, INovelChoicePresenter
    {
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private Button _buttonPrefab;

        public Transform ButtonContainer
        {
            get => _buttonContainer;
            set => _buttonContainer = value;
        }

        public Button ButtonPrefab
        {
            get => _buttonPrefab;
            set => _buttonPrefab = value;
        }

        private int _selectedChoiceIndex = -1;

        public async Awaitable<int> ShowChoicesAsync(IReadOnlyList<string> choiceTexts, CancellationToken ct)
        {
            _selectedChoiceIndex = -1;

            if (_buttonContainer != null)
            {
                _buttonContainer.gameObject.SetActive(true);
            }

            var spawnedButtons = new List<Button>();

            for (var i = 0; i < choiceTexts.Count; i++)
            {
                var index = i;
                var btn = Instantiate(_buttonPrefab, _buttonContainer);
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = choiceTexts[i];
                }

                btn.onClick.AddListener(() =>
                {
                    _selectedChoiceIndex = index;
                });

                spawnedButtons.Add(btn);
            }

            // プレイヤーの選択を待機
            while (_selectedChoiceIndex < 0 && !ct.IsCancellationRequested)
            {
                await Awaitable.NextFrameAsync(ct);
            }

            // ボタンの破棄
            foreach (var btn in spawnedButtons)
            {
                if (btn != null) Destroy(btn.gameObject);
            }

            if (_buttonContainer != null)
            {
                _buttonContainer.gameObject.SetActive(false);
            }

            return _selectedChoiceIndex;
        }
    }
}

