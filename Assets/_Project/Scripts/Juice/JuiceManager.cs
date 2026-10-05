using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sahnedeki tüm juice modüllerini toplar ve global yoğunluğu yönetir.
/// Efektlerin kendisini hiç bilmez — sadece onları açıp kapatır ve ölçekler.
/// </summary>
public class JuiceManager : MonoBehaviour
{
    public static JuiceManager Instance { get; private set; }

    /// <summary>Modüllerin okuduğu global çarpan. Statik, çünkü modüller
    /// Awake sırasında Instance henüz hazır olmadan okuyabilir.</summary>
    public static float GlobalIntensity { get; private set; } = 1f;

    [SerializeField, Range(0f, 1f)] private float globalIntensity = 1f;
    [SerializeField] private JuiceProfile startupProfile;

    private readonly List<JuiceModule> modules = new List<JuiceModule>();
    public IReadOnlyList<JuiceModule> Modules => modules;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Hiyerarşide nerede olurlarsa olsunlar tüm modülleri topla.
        modules.Clear();
        modules.AddRange(FindObjectsByType<JuiceModule>(
            FindObjectsInactive.Include, FindObjectsSortMode.None));

        GlobalIntensity = globalIntensity;

        if (startupProfile != null) ApplyProfile(startupProfile);
    }

    /// <summary>Inspector'daki kaydırıcıyı oyun çalışırken oynatabilmek için.</summary>
    private void OnValidate()
    {
        GlobalIntensity = Mathf.Clamp01(globalIntensity);
    }

    public void SetGlobalIntensity(float value)
    {
        globalIntensity = Mathf.Clamp01(value);
        GlobalIntensity = globalIntensity;
    }

    public void ApplyProfile(JuiceProfile profile)
    {
        if (profile == null) return;

        SetGlobalIntensity(profile.globalIntensity);

        foreach (JuiceModule module in modules)
        {
            JuiceProfile.ModuleSetting setting =
                profile.moduleSettings.Find(s => s.moduleName == module.DisplayName);

            // Profilde tanımlı değilse modülün kendi ayarına dokunma.
            if (setting == null) continue;

            module.ModuleEnabled = setting.enabled;
            module.Intensity = setting.intensity;
        }
    }
}