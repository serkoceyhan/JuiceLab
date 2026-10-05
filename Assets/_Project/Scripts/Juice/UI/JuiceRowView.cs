using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panelde tek bir modül satırı: açma/kapama kutusu, isim ve yoğunluk kaydırıcısı.
/// Hangi modüle bağlandığını Bind ile öğrenir; modülün ne yaptığını bilmez.
/// </summary>
public class JuiceRowView : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Slider intensitySlider;

    private JuiceModule module;

    public void Bind(JuiceModule target)
    {
        module = target;
        label.text = module.DisplayName;

        // SetIsOnWithoutNotify / SetValueWithoutNotify kritik:
        // normal atama onValueChanged'i tetikler ve modüle geri yazar,
        // profil uygularken sonsuz döngüye ya da değer ezilmesine yol açar.
        toggle.SetIsOnWithoutNotify(module.ModuleEnabled);
        intensitySlider.SetValueWithoutNotify(module.Intensity);

        toggle.onValueChanged.RemoveAllListeners();
        intensitySlider.onValueChanged.RemoveAllListeners();

        toggle.onValueChanged.AddListener(value => module.ModuleEnabled = value);
        intensitySlider.onValueChanged.AddListener(value => module.Intensity = value);
    }

    /// <summary>Profil uygulandıktan sonra UI'yi modülün gerçek değerine geri senkronlar.</summary>
    public void Refresh()
    {
        if (module == null) return;

        toggle.SetIsOnWithoutNotify(module.ModuleEnabled);
        intensitySlider.SetValueWithoutNotify(module.Intensity);
    }
}