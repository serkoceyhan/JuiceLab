using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Skor anında zıplamaz: hedefe doğru sayarak gider ve yazı bir "punch" atar.
/// Küçük bir detay ama amatör ile profesyonel UI arasındaki farkın çoğu burada.
///
/// Modül açıkken skor yazısının sahipliğini HUD'dan devralır, kapanınca geri verir.
/// </summary>
public class ScorePopModule : JuiceModule
{
    public override string DisplayName => "Score Pop";

    [SerializeField] private HUD hud;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private RectTransform scoreRect;

    [SerializeField] private float countDuration = 0.3f;
    [SerializeField, Range(0f, 1f)] private float punchAmount = 0.35f;
    [SerializeField] private float punchDuration = 0.25f;

    private Vector3 baseScale = Vector3.one;
    private int displayed;
    private int target;
    private Coroutine counting;
    private Coroutine punching;
    private bool claimed;

    private void Awake()
    {
        if (scoreRect != null) baseScale = scoreRect.localScale;
    }

    private void OnEnable() => GameEvents.ScoreChanged += OnScoreChanged;

    private void OnDisable()
    {
        GameEvents.ScoreChanged -= OnScoreChanged;
        Release();
        if (scoreRect != null) scoreRect.localScale = baseScale;
    }

    private void Update()
    {
        // Global slider oynatıldığında sahiplik anında el değiştirsin.
        if (IsActive && !claimed) Claim();
        else if (!IsActive && claimed) Release();
    }

    private void Claim()
    {
        claimed = true;
        if (hud != null) hud.ScoreHandledExternally = true;
        displayed = target = GameManager.Instance != null ? GameManager.Instance.Score : 0;
    }

    private void Release()
    {
        claimed = false;
        if (counting != null) { StopCoroutine(counting); counting = null; }
        if (hud != null)
        {
            hud.ScoreHandledExternally = false;
            hud.RefreshScore();
        }
        if (scoreRect != null) scoreRect.localScale = baseScale;
    }

    private void OnScoreChanged(int score)
    {
        if (!IsActive) return;

        target = score;

        if (counting != null) StopCoroutine(counting);
        counting = StartCoroutine(CountRoutine());

        if (punching != null) StopCoroutine(punching);
        punching = StartCoroutine(PunchRoutine());
    }

    private IEnumerator CountRoutine()
    {
        int from = displayed;
        float duration = Mathf.Max(0.0001f, countDuration * Amount);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            displayed = Mathf.RoundToInt(
                Mathf.Lerp(from, target, Easing.OutQuad(Mathf.Clamp01(t))));
            Write();
            yield return null;
        }

        displayed = target;
        Write();
        counting = null;
    }

    private IEnumerator PunchRoutine()
    {
        if (scoreRect == null) yield break;

        float amount = punchAmount * Amount;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, punchDuration);
            // OutBack 1'i aştığı için yazı geri dönerken hafifçe küçülüp oturur.
            float k = Easing.OutBack(Mathf.Clamp01(t));
            scoreRect.localScale = baseScale * Mathf.Lerp(1f + amount, 1f, k);
            yield return null;
        }

        scoreRect.localScale = baseScale;
        punching = null;
    }

    private void Write()
    {
        if (scoreText != null) scoreText.text = $"SCORE {displayed}";
    }
}