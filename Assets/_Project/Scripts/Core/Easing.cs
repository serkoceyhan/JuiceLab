using UnityEngine;

/// <summary>
/// Yumuşatma eğrileri. Hazır tween kütüphanesi yerine elle yazıldı —
/// bu projede eğrinin kendisi konunun ta kendisi.
/// Hepsi t ∈ [0,1] alır. Back ve Elastic bilerek [0,1] dışına taşar.
/// </summary>
public static class Easing
{
    /// <summary>Hızlı başlar, yavaş biter. Darbe girişleri için.</summary>
    public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);

    /// <summary>Yavaş başlar, hızlanır. Kaybolma için.</summary>
    public static float InQuad(float t) => t * t;

    /// <summary>Hedefi aşıp geri döner. "Pop" hissi.</summary>
    public static float OutBack(float t, float overshoot = 1.70158f)
    {
        float c = overshoot + 1f;
        float p = t - 1f;
        return 1f + c * p * p * p + overshoot * p * p;
    }

    /// <summary>Yaylanarak oturur. Squash & stretch toparlanması için.</summary>
    public static float OutElastic(float t, float amplitude = 1f, float period = 0.35f)
    {
        if (t <= 0f) return 0f;
        if (t >= 1f) return 1f;

        float s = period / (2f * Mathf.PI) * Mathf.Asin(1f / Mathf.Max(amplitude, 1f));
        return amplitude * Mathf.Pow(2f, -10f * t)
                         * Mathf.Sin((t - s) * (2f * Mathf.PI) / period) + 1f;
    }
}