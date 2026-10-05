using UnityEngine;

/// <summary>
/// Çarpma noktasında, çarpma normali yönünde partikül püskürtür.
///
/// Her çarpmada yeni bir ParticleSystem yaratmak (Instantiate) hem çöp üretir
/// hem havuzlama gerektirir. Bunun yerine tek bir sistem kullanıp
/// Emit(EmitParams) ile parçacıkları tek tek besliyoruz: ayırma yok.
///
/// Rastgele yöne 50 parçacık atmak yerine doğru yöne 14 parçacık atmak
/// hem daha iyi görünür hem daha ucuzdur.
/// </summary>
public class ImpactParticlesModule : JuiceModule
{
    public override string DisplayName => "Impact Particles";

    [SerializeField] private ParticleSystem particles;

    [Header("Parçacık sayısı")]
    [SerializeField] private int brickCount = 14;
    [SerializeField] private int paddleCount = 5;
    [SerializeField] private int wallCount = 3;

    [Header("Hareket")]
    [SerializeField] private float speedMin = 3f;
    [SerializeField] private float speedMax = 9f;
    [SerializeField] private float spreadAngle = 70f;
    [SerializeField] private float lifetime = 0.45f;
    [SerializeField] private float sizeMin = 0.08f;
    [SerializeField] private float sizeMax = 0.22f;

    private void OnEnable()
    {
        GameEvents.BrickDestroyed += OnBrick;
        GameEvents.BallHitPaddle  += OnPaddle;
        GameEvents.BallHitWall    += OnWall;
    }

    private void OnDisable()
    {
        GameEvents.BrickDestroyed -= OnBrick;
        GameEvents.BallHitPaddle  -= OnPaddle;
        GameEvents.BallHitWall    -= OnWall;
    }

    private void OnBrick(ImpactInfo info)  => Burst(info, brickCount);
    private void OnPaddle(ImpactInfo info) => Burst(info, paddleCount);
    private void OnWall(ImpactInfo info)   => Burst(info, wallCount);

    private void Burst(ImpactInfo info, int baseCount)
    {
        if (!IsActive || particles == null) return;

        int count = Mathf.RoundToInt(baseCount * Amount * info.Strength);
        if (count <= 0) return;

        float baseAngle = Mathf.Atan2(info.Normal.y, info.Normal.x) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + Random.Range(-spreadAngle, spreadAngle) * 0.5f;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

            var emit = new ParticleSystem.EmitParams
            {
                position      = info.Point,
                velocity      = dir * Random.Range(speedMin, speedMax),
                startColor    = info.Color,
                startSize     = Random.Range(sizeMin, sizeMax),
                startLifetime = lifetime * Random.Range(0.7f, 1.2f)
            };

            particles.Emit(emit, 1);
        }
    }
}