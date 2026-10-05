using UnityEngine;

/// <summary>
/// Ses katmanı.
///
/// Tek bir AudioSource kullanmak yerine küçük bir "ses havuzu" tutuyoruz:
/// tek kaynakta yeni ses çalmak öncekini keser, hızlı çarpışmalarda
/// ses kopuk kopuk duyulur. Round-robin ile sesler üst üste binebilir.
///
/// Combo yükseldikçe pitch'i yarım ses adımlarıyla yukarı taşıyoruz.
/// 2^(n/12) müzikteki yarım ses oranıdır — lineer artış kulağa yanlış gelir,
/// üstel artış "doğru" gelir. Zelda'nın ot kesme sesi bu numarayı kullanır.
/// </summary>
public class AudioJuiceModule : JuiceModule
{
    public override string DisplayName => "Audio";

    [Header("Klipler")]
    [SerializeField] private AudioClip brickClip;
    [SerializeField] private AudioClip paddleClip;
    [SerializeField] private AudioClip wallClip;
    [SerializeField] private AudioClip ballLostClip;

    [Header("Ses havuzu")]
    [SerializeField] private int voiceCount = 8;
    [SerializeField, Range(0f, 1f)] private float volume = 0.55f;

    [Header("Pitch")]
    [SerializeField] private Vector2 pitchJitter = new Vector2(0.94f, 1.06f);
    [SerializeField] private float comboSemitones = 0.7f;
    [SerializeField] private int maxComboSteps = 14;

    [Header("Spam koruması")]
    [Tooltip("Aynı frame'de beş ses çalınca kulak tırmalar.")]
    [SerializeField] private float minInterval = 0.035f;

    private AudioSource[] voices;
    private int nextVoice;
    private float lastPlayTime;

    private void Awake()
    {
        voices = new AudioSource[Mathf.Max(1, voiceCount)];

        for (int i = 0; i < voices.Length; i++)
        {
            var go = new GameObject($"Voice_{i}");
            go.transform.SetParent(transform, false);

            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;   // 2D: mesafeye göre kısılmasın
            voices[i] = src;
        }
    }

    private void OnEnable()
    {
        GameEvents.BrickDestroyed += OnBrick;
        GameEvents.BallHitPaddle  += OnPaddle;
        GameEvents.BallHitWall    += OnWall;
        GameEvents.BallLost       += OnBallLost;
    }

    private void OnDisable()
    {
        GameEvents.BrickDestroyed -= OnBrick;
        GameEvents.BallHitPaddle  -= OnPaddle;
        GameEvents.BallHitWall    -= OnWall;
        GameEvents.BallLost       -= OnBallLost;
    }

    private void OnBrick(ImpactInfo info)  => Play(brickClip, info.Strength, info.Combo);
    private void OnPaddle(ImpactInfo info) => Play(paddleClip, info.Strength, 0);
    private void OnWall(ImpactInfo info)   => Play(wallClip, info.Strength * 0.7f, 0);
    private void OnBallLost()              => Play(ballLostClip, 1f, 0, ignoreInterval: true);

    private void Play(AudioClip clip, float strength, int combo, bool ignoreInterval = false)
    {
        if (!IsActive || clip == null) return;

        // Hit-stop sırasında timeScale düşer ama ses normal hızda akar,
        // o yüzden aralığı gerçek zamanla ölçüyoruz.
        if (!ignoreInterval && Time.unscaledTime - lastPlayTime < minInterval) return;
        lastPlayTime = Time.unscaledTime;

        AudioSource src = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;

        int step = Mathf.Min(combo, maxComboSteps);
        float comboPitch = Mathf.Pow(2f, comboSemitones * step / 12f);

        src.clip = clip;
        src.pitch = Random.Range(pitchJitter.x, pitchJitter.y) * comboPitch;
        src.volume = volume * Amount * Mathf.Clamp01(strength);
        src.Play();
    }
}