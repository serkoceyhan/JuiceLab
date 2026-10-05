using System.Collections;
using UnityEngine;

/// <summary>
/// Kameraya kısa bir zoom darbesi.
///
/// Shake ile aynı kanalı kullanmıyor: shake pozisyonu, punch orthographicSize'ı
/// değiştirir. Bu ayrım sayesinde ikisi aynı anda çalışırken birbirini ezmez —
/// aynı özelliği iki modülün yazması juice sistemlerinde en sık görülen hatadır.
/// </summary>
public class CameraPunchModule : JuiceModule
{
    public override string DisplayName => "Camera Punch";

    [SerializeField] private Camera targetCamera;
    [SerializeField] private float brickPunch = 0.18f;
    [SerializeField] private float ballLostPunch = 0.5f;
    [SerializeField] private float inDuration = 0.04f;
    [SerializeField] private float outDuration = 0.22f;

    private float baseSize;
    private Coroutine running;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = GetComponent<Camera>();
        if (targetCamera != null) baseSize = targetCamera.orthographicSize;
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

        if (running != null) { StopCoroutine(running); running = null; }
        if (targetCamera != null) targetCamera.orthographicSize = baseSize;
    }

    private void OnBrick(ImpactInfo info) => Punch(brickPunch * info.Strength);
    private void OnBallLost()             => Punch(ballLostPunch);

    private void Punch(float strength)
    {
        if (!IsActive || targetCamera == null) return;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(PunchRoutine(strength * Amount));
    }

    private IEnumerator PunchRoutine(float strength)
    {
        float target = baseSize - strength;    // size küçülmesi = yakınlaşma
        float start = targetCamera.orthographicSize;

        // Hit-stop sırasında donmasın diye unscaled zaman.
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / inDuration;
            targetCamera.orthographicSize =
                Mathf.Lerp(start, target, Easing.OutQuad(Mathf.Clamp01(t)));
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / outDuration;
            // OutBack 1'i aştığı için geri dönerken hafif bir "esneme" oluşur.
            targetCamera.orthographicSize =
                Mathf.Lerp(target, baseSize, Easing.OutBack(Mathf.Clamp01(t)));
            yield return null;
        }

        targetCamera.orthographicSize = baseSize;
        running = null;
    }
}