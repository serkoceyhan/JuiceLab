using UnityEngine;

/// <summary>
/// Trauma tabanlı ekran sarsıntısı.
///
/// Klasik yaklaşım "çarpınca kamerayı N kare rastgele oynat"tır ve titrek durur.
/// Burada bunun yerine 0-1 arası bir "trauma" birikimi tutuyoruz:
///   • Her olay trauma ekler, trauma zamanla sabit hızda azalır.
///   • Sarsıntı miktarı trauma DEĞİL trauma² ile ölçeklenir. Kare almak
///     küçük olayları bastırır, büyükleri öne çıkarır — his daha okunaklı olur.
///   • Offset rastgele değil Perlin noise'dan gelir. Random titrek,
///     Perlin akışkan hissettirir.
///
/// Zamanı unscaled okuyoruz: hit-stop efekti Time.timeScale'i sıfıra yaklaştıracak,
/// ama sarsıntının o sırada donmaması gerekiyor.
/// </summary>
public class CameraShakeModule : JuiceModule
{
    public override string DisplayName => "Camera Shake";

    [Header("Sarsıntı büyüklüğü")]
    [SerializeField] private float maxOffset = 0.55f;
    [SerializeField] private float maxAngle = 3.5f;
    [SerializeField] private float frequency = 22f;
    [SerializeField] private float decayPerSecond = 1.5f;

    [Header("Olay başına eklenen trauma")]
    [SerializeField, Range(0f, 1f)] private float brickTrauma = 0.40f;
    [SerializeField, Range(0f, 1f)] private float paddleTrauma = 0.18f;
    [SerializeField, Range(0f, 1f)] private float wallTrauma = 0.10f;
    [SerializeField, Range(0f, 1f)] private float ballLostTrauma = 0.75f;

    private float trauma;
    private Vector3 basePosition;
    private float seed;

    private void Awake()
    {
        basePosition = transform.localPosition;
        seed = Random.value * 100f;
    }

    private void OnEnable()
    {
        GameEvents.BrickDestroyed += OnBrickDestroyed;
        GameEvents.BallHitPaddle  += OnPaddleHit;
        GameEvents.BallHitWall    += OnWallHit;
        GameEvents.BallLost       += OnBallLost;
    }

    private void OnDisable()
    {
        GameEvents.BrickDestroyed -= OnBrickDestroyed;
        GameEvents.BallHitPaddle  -= OnPaddleHit;
        GameEvents.BallHitWall    -= OnWallHit;
        GameEvents.BallLost       -= OnBallLost;
    }

    private void OnBrickDestroyed(ImpactInfo info) => AddTrauma(brickTrauma * info.Strength);
    private void OnPaddleHit(ImpactInfo info)      => AddTrauma(paddleTrauma * info.Strength);
    private void OnWallHit(ImpactInfo info)        => AddTrauma(wallTrauma * info.Strength);
    private void OnBallLost()                      => AddTrauma(ballLostTrauma);

    private void AddTrauma(float amount)
    {
        if (!IsActive) return;
        trauma = Mathf.Clamp01(trauma + amount * Amount);
    }

    private void LateUpdate()
    {
        if (trauma <= 0f)
        {
            transform.localPosition = basePosition;
            transform.localRotation = Quaternion.identity;
            return;
        }

        float shake = trauma * trauma;
        float t = Time.unscaledTime * frequency;

        // Her eksen farklı seed kullanmalı; aynı seed verilirse
        // sarsıntı tek bir çapraz çizgi üzerinde gidip gelir.
        float offsetX = (Mathf.PerlinNoise(seed,       t) * 2f - 1f) * maxOffset * shake;
        float offsetY = (Mathf.PerlinNoise(seed + 10f, t) * 2f - 1f) * maxOffset * shake;
        float angle   = (Mathf.PerlinNoise(seed + 20f, t) * 2f - 1f) * maxAngle  * shake;

        transform.localPosition = basePosition + new Vector3(offsetX, offsetY, 0f);
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);

        trauma = Mathf.Max(0f, trauma - decayPerSecond * Time.unscaledDeltaTime);
    }
}