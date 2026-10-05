using System.Collections;
using UnityEngine;

/// <summary>
/// Raket topu karşıladığında dikeyde ezilip yaylanarak toparlanır.
/// Ball squash ile aynı fikir, tek fark eksen sabit: raket daima yukarıdan darbe alır.
/// </summary>
public class PaddleSquashModule : JuiceModule
{
    public override string DisplayName => "Paddle Squash";

    [SerializeField] private Transform target;
    [SerializeField, Range(0f, 0.8f)] private float squashAmount = 0.35f;
    [SerializeField] private float squashInDuration = 0.04f;
    [SerializeField] private float recoverDuration = 0.32f;

    private Vector3 baseScale;
    private Coroutine running;

    private void Awake()
    {
        if (target == null) target = transform;
        baseScale = target.localScale;
    }

    private void OnEnable() => GameEvents.BallHitPaddle += OnHit;

    private void OnDisable()
    {
        GameEvents.BallHitPaddle -= OnHit;

        if (running != null) { StopCoroutine(running); running = null; }
        if (target != null) target.localScale = baseScale;
    }

    private void OnHit(ImpactInfo info)
    {
        if (!IsActive || target == null) return;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Routine(squashAmount * Amount * info.Strength));
    }

    private IEnumerator Routine(float amount)
    {
        float squashY = 1f - amount;
        float stretchX = 1f / Mathf.Max(squashY, 0.05f);   // hacmi koru

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(squashInDuration, 0.0001f);
            float k = Easing.OutQuad(Mathf.Clamp01(t));
            Apply(Mathf.Lerp(1f, stretchX, k), Mathf.Lerp(1f, squashY, k));
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(recoverDuration, 0.0001f);
            float k = Easing.OutElastic(Mathf.Clamp01(t));
            Apply(Mathf.Lerp(stretchX, 1f, k), Mathf.Lerp(squashY, 1f, k));
            yield return null;
        }

        target.localScale = baseScale;
        running = null;
    }

    private void Apply(float x, float y)
    {
        target.localScale = new Vector3(baseScale.x * x, baseScale.y * y, baseScale.z);
    }
}