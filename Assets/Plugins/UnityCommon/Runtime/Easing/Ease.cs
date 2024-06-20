namespace UnityCommon
{
    /// <summary>
    /// イージングの種類
    /// </summary>
    public enum Ease
    {
        Linear,
        InSine,
        OutSine,
        InOutSine,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,
        InExpo,
        OutExpo,
        ExpoInOut,
        InCirc,
        OutCirc,
        InOutCirc,
        InBack,
        OutBack,
        InOutBack,
        InElastic,
        OutElastic,
        InOutElastic,
        InBounce,
        OutBounce,
        InOutBounce,
    }

    public static class EaseExtensions
    {
        /// <summary>
        /// 指定した種類のイージング関数を取得する
        /// </summary>
        /// <param name="ease"></param>
        /// <returns></returns>
        public static EasingFunction GetEasingFunction(this Ease ease)
        {
            return ease switch {
                Ease.Linear => EasingFunctions.EaseLinear,
                Ease.InSine => EasingFunctions.EaseInSine,
                Ease.OutSine => EasingFunctions.EaseOutSine,
                Ease.InOutSine => EasingFunctions.EaseInOutSine,
                Ease.InQuad => EasingFunctions.EaseInQuad,
                Ease.OutQuad => EasingFunctions.EaseOutQuad,
                Ease.InOutQuad => EasingFunctions.EaseInOutQuad,
                Ease.InCubic => EasingFunctions.EaseInCubic,
                Ease.OutCubic => EasingFunctions.EaseOutCubic,
                Ease.InOutCubic => EasingFunctions.EaseInOutCubic,
                Ease.InQuart => EasingFunctions.EaseInQuart,
                Ease.OutQuart => EasingFunctions.EaseOutQuart,
                Ease.InOutQuart => EasingFunctions.EaseInOutQuart,
                Ease.InQuint => EasingFunctions.EaseInQuint,
                Ease.OutQuint => EasingFunctions.EaseOutQuint,
                Ease.InOutQuint => EasingFunctions.EaseInOutQuint,
                Ease.InExpo => EasingFunctions.EaseInExpo,
                Ease.OutExpo => EasingFunctions.EaseOutExpo,
                Ease.ExpoInOut => EasingFunctions.EaseInOutExpo,
                Ease.InCirc => EasingFunctions.EaseInCirc,
                Ease.OutCirc => EasingFunctions.EaseOutCirc,
                Ease.InOutCirc => EasingFunctions.EaseInOutCirc,
                Ease.InBack => EasingFunctions.EaseInBack,
                Ease.OutBack => EasingFunctions.EaseOutBack,
                Ease.InOutBack => EasingFunctions.EaseInOutBack,
                Ease.InElastic => EasingFunctions.EaseInElastic,
                Ease.OutElastic => EasingFunctions.EaseOutElastic,
                Ease.InOutElastic => EasingFunctions.EaseInOutElastic,
                Ease.InBounce => EasingFunctions.EaseInBounce,
                Ease.OutBounce => EasingFunctions.EaseOutBounce,
                Ease.InOutBounce => EasingFunctions.EaseInOutBounce,
                _ => EasingFunctions.EaseLinear
            };
        }
    }
}
