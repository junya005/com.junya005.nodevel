using System;

namespace Junya005.Nodevel
{
    /// <summary>
    /// オート進行時の待機時間を計算・制御するコントローラー。
    /// </summary>
    public class AutoPlayController
    {
        public bool IsAutoPlayEnabled { get; set; } = false;

        public float BaseWaitSeconds { get; set; } = 1.0f;
        public float PerCharacterSeconds { get; set; } = 0.05f;
        public float MinWaitSeconds { get; set; } = 0.5f;

        public float CalculateWaitDuration(string cleanText)
        {
            var len = string.IsNullOrEmpty(cleanText) ? 0 : cleanText.Length;
            var duration = BaseWaitSeconds + (len * PerCharacterSeconds);
            return Math.Max(MinWaitSeconds, duration);
        }
    }
}

