using System.Collections;
using UnityEngine;

/// <summary>
/// Top çarptığında çarpma normaline dik ezilir, sonra yaylanarak toparlanır.
/// Hacim korunur (ezilen eksenin tersi aynı oranda uzar): şişip küçülmüş
/// gibi değil, gerçekten ezilmiş gibi durur.
///
/// Yalnızca görsel çocuk objeye dokunur; Rigidbody2D'ye hiç karışmaz.
/// </summary>
public class BallSquashModule : JuiceModule
{
    public override string DisplayName => "Ball Squash & Stretch";

    [SerializeField] private Transform target;
    [SerializeField, Range(0f, 0.8f)] private float squashAmount = 0.45f;
    [SerializeField] private float squashInDuration = 0.05f;
    [SerializeField] private float recoverDuration = 0.3f;

    private Vector3 baseScale;
    private Coroutine running;

    private void Awake()
    {
        if (target == null) target = transform;
        baseScale = target.localScale;
    }

    private void OnEnable()
    {
        GameEvents.BallHitPaddle  += OnImpact;
        GameEvents.BallHitWall    += OnImpact;
        GameEvents.BrickDestroyed += OnImpact;
    }

    private void OnDisable()
    {
        GameEvents.BallHitPaddle  -= OnImpact;
        GameEvents.BallHitWall    -= OnImpact;
        GameEvents.BrickDestroyed -= OnImpact;

        if (running != null) { StopCoroutine(running); running = null; }
        if (target != null)
        {
            target.localScale = baseScale;
            target.localRotation = Quaternion.identity;
        }
    }

    private void OnImpact(ImpactInfo info)
    {
        if (!IsActive || target == null) return;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(SquashRoutine(info.Normal, squashAmount * Amount * info.Strength));
    }

    private IEnumerator SquashRoutine(Vector2 normal, float amount)
    {
        // Ezilme ekseni çarpma normali. Objeyi o yöne döndürüp X'te eziyoruz.
        float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;

        float squash = 1f - amount;
        float stretch = 1f / Mathf.Max(squash, 0.05f);   // hacmi koru

        // 1) Hızlıca ez
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(squashInDuration, 0.0001f);
            float k = Easing.OutQuad(Mathf.Clamp01(t));
            Apply(angle, Mathf.Lerp(1f, squash, k), Mathf.Lerp(1f, stretch, k));
            yield return null;
        }

        // 2) Yaylanarak geri dön
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(recoverDuration, 0.0001f);
            float k = Easing.OutElastic(Mathf.Clamp01(t));
            Apply(angle, Mathf.Lerp(squash, 1f, k), Mathf.Lerp(stretch, 1f, k));
            yield return null;
        }

        target.localScale = baseScale;
        target.localRotation = Quaternion.identity;
        running = null;
    }

    private void Apply(float angleDeg, float alongNormal, float acrossNormal)
    {
        target.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
        target.localScale = new Vector3(baseScale.x * alongNormal,
                                        baseScale.y * acrossNormal,
                                        baseScale.z);
    }
}