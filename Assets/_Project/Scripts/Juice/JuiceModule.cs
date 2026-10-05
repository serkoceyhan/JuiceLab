using UnityEngine;

/// <summary>
/// Her juice efektinin temel sınıfı.
/// Her modül bağımsız açılıp kapanabilir ve kendi yoğunluğuna sahiptir.
/// Efekt büyüklüğü daima Amount ile ölçeklenir — böylece tek bir global
/// kaydırıcı bütün sistemi 0'dan 1'e kademeli açabilir.
/// </summary>
public abstract class JuiceModule : MonoBehaviour
{
    [SerializeField] private bool moduleEnabled = true;
    [SerializeField, Range(0f, 1f)] private float intensity = 1f;

    /// <summary>Debug panelinde görünecek isim.</summary>
    public abstract string DisplayName { get; }

    public bool ModuleEnabled
    {
        get => moduleEnabled;
        set => moduleEnabled = value;
    }

    public float Intensity
    {
        get => intensity;
        set => intensity = Mathf.Clamp01(value);
    }

    /// <summary>Modülün kendi yoğunluğu × global juice kaydırıcısı.</summary>
    protected float Amount => moduleEnabled ? intensity * JuiceManager.GlobalIntensity : 0f;

    protected bool IsActive => Amount > 0.001f;
}