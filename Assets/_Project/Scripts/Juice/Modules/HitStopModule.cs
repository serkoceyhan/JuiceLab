using System.Collections;
using UnityEngine;

/// <summary>
/// Çarpma anında zamanı çok kısa süre neredeyse durdurur (freeze frame).
/// Dövüş oyunlarının darbe ağırlığı bu tek numaradan gelir.
///
/// Kritik ayrıntı: Time.timeScale düşünce normal coroutine bekleme de yavaşlar.
/// Süreyi Time.unscaledDeltaTime ile saymazsan efekt kendi kendini dondurur
/// ve oyun bir daha asla hızlanmaz.
/// </summary>
public class HitStopModule : JuiceModule
{
    public override string DisplayName => "Hit Stop";

    [Header("Donma süresi (gerçek saniye)")]
    [SerializeField] private float brickDuration = 0.07f;
    [SerializeField] private float paddleDuration = 0.025f;
    [SerializeField] private float ballLostDuration = 0.16f;

    [Tooltip("0 = tam donma. Tam sıfır yerine çok küçük bir değer daha organik durur.")]
    [SerializeField, Range(0f, 0.3f)] private float frozenTimeScale = 0.03f;

    private Coroutine running;

    private void OnEnable()
    {
        GameEvents.BrickDestroyed += OnBrick;
        GameEvents.BallHitPaddle  += OnPaddle;
        GameEvents.BallLost       += OnBallLost;
    }

    private void OnDisable()
    {
        GameEvents.BrickDestroyed -= OnBrick;
        GameEvents.BallHitPaddle  -= OnPaddle;
        GameEvents.BallLost       -= OnBallLost;

        // Modül kapanırken zamanı normale döndür, yoksa oyun ağır çekimde kalır.
        if (running != null) { StopCoroutine(running); running = null; }
        Time.timeScale = 1f;
    }

    private void OnBrick(ImpactInfo info)  => Freeze(brickDuration * info.Strength);
    private void OnPaddle(ImpactInfo info) => Freeze(paddleDuration * info.Strength);
    private void OnBallLost()              => Freeze(ballLostDuration);

    private void Freeze(float duration)
    {
        if (!IsActive) return;

        float scaled = duration * Amount;
        if (scaled <= 0.001f) return;

        // Üst üste binen donmalarda sonuncusu kazansın, süreler toplanmasın.
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FreezeRoutine(scaled));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        Time.timeScale = frozenTimeScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            // WaitForSecondsRealtime her çağrıda nesne ayırır; bu döngü ayırmaz.
            yield return null;
        }

        Time.timeScale = 1f;
        running = null;
    }
}