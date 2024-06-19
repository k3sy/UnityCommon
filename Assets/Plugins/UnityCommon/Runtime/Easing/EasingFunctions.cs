using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// イージング関数
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public delegate float EasingFunction(float x);

    /// <summary>
    /// イージング関数の定義
    /// </summary>
    public static class EasingFunctions
    {
        public static readonly EasingFunction Linear = x => x;

        public static readonly EasingFunction SineIn = x => 1 - Mathf.Cos(x * Mathf.PI / 2);

        public static readonly EasingFunction SineOut = x => Mathf.Sin(x * Mathf.PI / 2);

        public static readonly EasingFunction SineInOut = x => 0.5f - Mathf.Cos(Mathf.PI * x) / 2;

        public static readonly EasingFunction QuadIn = x => x * x;

        public static readonly EasingFunction QuadOut = x => 1 - --x * x;

        public static readonly EasingFunction QuadInOut = x => x < 0.5f ? 2 * x * x : 1 - 2 * --x * x;

        public static readonly EasingFunction CubicIn = x => x * x * x;

        public static readonly EasingFunction CubicOut = x => 1 + --x * x * x;

        public static readonly EasingFunction CubicInOut = x => x < 0.5f ? 4 * x * x * x : 1 + 4 * --x * x * x;

        public static readonly EasingFunction QuartIn = x => x * x * x * x;

        public static readonly EasingFunction QuartOut = x => 1 - --x * x * x * x;

        public static readonly EasingFunction QuartInOut = x => x < 0.5f ? 8 * x * x * x * x : 1 - 8 * --x * x * x * x;

        public static readonly EasingFunction QuintIn = x => x * x * x * x * x;

        public static readonly EasingFunction QuintOut = x => 1 + --x * x * x * x * x;

        public static readonly EasingFunction QuintInOut = x => x < 0.5f ? 16 * x * x * x * x * x : 1 + 16 * --x * x * x * x * x;

        public static readonly EasingFunction ExpoIn = x => x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);

        public static readonly EasingFunction ExpoOut = x => x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);

        public static readonly EasingFunction ExpoInOut = x => x == 0 ? 0 : x == 1 ? 1
            : x < 0.5f ? Mathf.Pow(2, 20 * x - 10) / 2 : (2 - Mathf.Pow(2, -20 * x + 10)) / 2;

        public static readonly EasingFunction CircIn = x => 1 - Mathf.Sqrt(1 - x * x);

        public static readonly EasingFunction CircOut = x => Mathf.Sqrt(1 - --x * x);

        public static readonly EasingFunction CircInOut = x =>
            x < 0.5f ? (1 - Mathf.Sqrt(1 - 4 * x * x)) / 2 : (1 + Mathf.Sqrt(1 - 4 * --x * x)) / 2;

        public static readonly EasingFunction BackIn = x => {
            float c = 1.70158f;
            return x * x * ((c + 1) * x - c);
        };

        public static readonly EasingFunction BackOut = x => {
            float c = 1.70158f;
            return 1 + --x * x * ((c + 1) * x + c);
        };

        public static readonly EasingFunction BackInOut = x => {
            float c = 1.70158f * 1.525f;
            return x < 0.5f ? 4 * x * x * ((c + 1) * 2 * x - c) / 2 : (2 + 4 * --x * x * ((c + 1) * 2 * x + c)) / 2;
        };

        public static readonly EasingFunction ElasticIn = x => x == 0 ? 0 : x == 1 ? 1
            : -Mathf.Pow(2, 10 * x - 10) * Mathf.Sin((x * 10 - 10.75f) * (2 * Mathf.PI / 3));

        public static readonly EasingFunction ElasticOut = x => x == 0 ? 0 : x == 1 ? 1
            : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * (2 * Mathf.PI / 3)) + 1;

        public static readonly EasingFunction ElasticInOut = x => x == 0 ? 0 : x == 1 ? 1
            : x < 0.5f ? -Mathf.Pow(2, 20 * x - 10) * Mathf.Sin((20 * x - 11.125f) * (2 * Mathf.PI / 4.5f)) / 2
            : Mathf.Pow(2, -20 * x + 10) * Mathf.Sin((20 * x - 11.125f) * (2 * Mathf.PI / 4.5f)) / 2 + 1;

        public static readonly EasingFunction BounceIn = x => 1 - BounceOut(1 - x);

        public static readonly EasingFunction BounceOut = x => x switch {
            < 1 / 2.75f => 7.5625f * x * x,
            < 2 / 2.75f => 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f,
            < 2.5f / 2.75f => 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f,
            _ => 7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f
        };

        public static readonly EasingFunction BounceInOut = x =>
            x < 0.5f ? (1 - BounceOut(1 - 2 * x)) / 2 : (1 + BounceOut(2 * x - 1)) / 2;
    }
}
