namespace UnityCommon
{
    /// <summary>
    /// イージングの種類
    /// </summary>
    public enum Ease
    {
        Linear,
        SineIn,
        SineOut,
        SineInOut,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        CircIn,
        CircOut,
        CircInOut,
        BackIn,
        BackOut,
        BackInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
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
                Ease.Linear => EasingFunctions.Linear,
                Ease.SineIn => EasingFunctions.SineIn,
                Ease.SineOut => EasingFunctions.SineOut,
                Ease.SineInOut => EasingFunctions.SineInOut,
                Ease.QuadIn => EasingFunctions.QuadIn,
                Ease.QuadOut => EasingFunctions.QuadOut,
                Ease.QuadInOut => EasingFunctions.QuadInOut,
                Ease.CubicIn => EasingFunctions.CubicIn,
                Ease.CubicOut => EasingFunctions.CubicOut,
                Ease.CubicInOut => EasingFunctions.CubicInOut,
                Ease.QuartIn => EasingFunctions.QuartIn,
                Ease.QuartOut => EasingFunctions.QuartOut,
                Ease.QuartInOut => EasingFunctions.QuartInOut,
                Ease.QuintIn => EasingFunctions.QuintIn,
                Ease.QuintOut => EasingFunctions.QuintOut,
                Ease.QuintInOut => EasingFunctions.QuintInOut,
                Ease.ExpoIn => EasingFunctions.ExpoIn,
                Ease.ExpoOut => EasingFunctions.ExpoOut,
                Ease.ExpoInOut => EasingFunctions.ExpoInOut,
                Ease.CircIn => EasingFunctions.CircIn,
                Ease.CircOut => EasingFunctions.CircOut,
                Ease.CircInOut => EasingFunctions.CircInOut,
                Ease.BackIn => EasingFunctions.BackIn,
                Ease.BackOut => EasingFunctions.BackOut,
                Ease.BackInOut => EasingFunctions.BackInOut,
                Ease.ElasticIn => EasingFunctions.ElasticIn,
                Ease.ElasticOut => EasingFunctions.ElasticOut,
                Ease.ElasticInOut => EasingFunctions.ElasticInOut,
                Ease.BounceIn => EasingFunctions.BounceIn,
                Ease.BounceOut => EasingFunctions.BounceOut,
                Ease.BounceInOut => EasingFunctions.BounceInOut,
                _ => EasingFunctions.Linear
            };
        }
    }
}
