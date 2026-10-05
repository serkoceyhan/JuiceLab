using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Büyük olaylarda kısa bir renk sapması + vinyet darbesi.
/// Shake ile aynı "pulse²" mantığı: küçük olaylar bastırılır, büyükler öne çıkar.
/// 100 ms'yi geçmemeli — uzun tutulursa oyun bozuk görünür, efekt gibi değil.
/// </summary>
public class PostProcessPunchModule : JuiceModule
{
    public override string DisplayName => "Post FX Punch";

    [SerializeField] private Volume volume;

    [SerializeField] private float maxChroma = 0.7f;
    [SerializeField] private float maxVignette = 0.42f;
    [SerializeField] private float decayPerSecond = 3.5f;

    [SerializeField, Range(0f, 1f)] private float brickPulse = 0.45f;
    [SerializeField, Range(0f, 1f)] private float ballLostPulse = 1f;

    private ChromaticAberration chroma;
    private Vignette vignette;
    private float pulse;
    private float baseVignette;

    private void Awake()
    {
        if (volume == null) return;

        // volume.profile getter'ı çalışma anında profilin bir KOPYASINI üretir,
        // böylece diskteki asset'i bozmadan değer değiştirebiliriz.
        // volume.sharedProfile kullansaydık editörde asset kalıcı olarak değişirdi —
        // "oyunu kapattım ama ekran hâlâ bozuk" bug'ının klasik kaynağı.
        VolumeProfile profile = volume.profile;
        profile.TryGet(out chroma);
        profile.TryGet(out vignette);

        if (vignette != null) baseVignette = vignette.intensity.value;
    }

    private void OnEnable()
    {
        GameEvents.BrickDestroyed += OnBrick;
        GameEvents.BallLost       += OnBallLost;
    }

    private void OnDisable()
    {
        GameEvents.BrickDestroyed -= OnBrick;
        GameEvents.BallLost       -= OnBallLost;

        pulse = 0f;
        Apply();
    }

    private void OnBrick(ImpactInfo info) => AddPulse(brickPulse * info.Strength);
    private void OnBallLost()             => AddPulse(ballLostPulse);

    private void AddPulse(float amount)
    {
        if (!IsActive) return;
        pulse = Mathf.Clamp01(pulse + amount * Amount);
    }

    private void Update()
    {
        if (pulse <= 0f) return;

        pulse = Mathf.Max(0f, pulse - decayPerSecond * Time.unscaledDeltaTime);
        Apply();
    }

    private void Apply()
    {
        float k = pulse * pulse;

        if (chroma != null) chroma.intensity.value = maxChroma * k;
        if (vignette != null)
            vignette.intensity.value = baseVignette + (maxVignette - baseVignette) * k;
    }
}