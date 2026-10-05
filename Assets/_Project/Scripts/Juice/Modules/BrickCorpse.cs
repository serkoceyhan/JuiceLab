using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Kırılan tuğlanın yerine geçen, tamamen görsel "ceset".
///
/// Gameplay tarafı tuğlayı anında yok eder — ölüm animasyonu oyun mantığını
/// bir kare bile geciktirmez. Animasyon tamamen juice katmanında yaşar.
/// Bu ayrım projenin ana fikri: his katmanı kaldırılınca oyun aynen çalışır.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BrickCorpse : MonoBehaviour
{
    [SerializeField] private float popDuration = 0.06f;
    [SerializeField] private float fadeDuration = 0.14f;
    [SerializeField] private float popScale = 1.3f;

    private SpriteRenderer sr;
    private Action<BrickCorpse> onFinished;

    private void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Play(Vector3 position, Vector3 scale, Color color,
                     float amount, Action<BrickCorpse> finished)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        onFinished = finished;
        transform.position = position;
        transform.localScale = scale;
        sr.color = color;

        gameObject.SetActive(true);
        StartCoroutine(Routine(scale, color, Mathf.Clamp01(amount)));
    }

    private IEnumerator Routine(Vector3 scale, Color color, float amount)
    {
        Vector3 popTarget = scale * Mathf.Lerp(1f, popScale, amount);
        Color flash = Color.Lerp(color, Color.white, amount);

        // 1) Kısa ve sert bir büyüme + beyaza kaçma
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / popDuration;
            float k = Easing.OutQuad(Mathf.Clamp01(t));
            transform.localScale = Vector3.Lerp(scale, popTarget, k);
            sr.color = Color.Lerp(color, flash, k);
            yield return null;
        }

        // 2) Küçülerek ve solarak kaybol
        Color transparent = new Color(flash.r, flash.g, flash.b, 0f);
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            float k = Easing.InQuad(Mathf.Clamp01(t));
            transform.localScale = Vector3.Lerp(popTarget, Vector3.zero, k);
            sr.color = Color.Lerp(flash, transparent, k);
            yield return null;
        }

        gameObject.SetActive(false);
        onFinished?.Invoke(this);
    }
}