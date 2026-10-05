using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Demo paneli. Satırları JuiceManager'ın modül listesinden üretir —
/// yeni bir modül yazıldığında panele elle satır eklemek gerekmez.
/// </summary>
public class JuiceDebugPanel : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private JuiceManager juiceManager;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform rowContainer;
    [SerializeField] private JuiceRowView rowPrefab;

    [Header("Global")]
    [SerializeField] private Slider globalSlider;
    [SerializeField] private TMP_Text globalValueLabel;

    [Header("Hazır profiller")]
    [SerializeField] private Button noJuiceButton;
    [SerializeField] private Button fullJuiceButton;
    [SerializeField] private JuiceProfile noJuiceProfile;
    [SerializeField] private JuiceProfile fullJuiceProfile;

    [Header("Kısayol")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    private readonly List<JuiceRowView> rows = new List<JuiceRowView>();

    private void Start()
    {
        // JuiceManager modülleri Awake'te topluyor; Start hepsinden sonra çalışır.
        BuildRows();

        globalSlider.SetValueWithoutNotify(JuiceManager.GlobalIntensity);
        globalSlider.onValueChanged.AddListener(OnGlobalChanged);
        UpdateGlobalLabel(JuiceManager.GlobalIntensity);

        if (noJuiceButton != null)
            noJuiceButton.onClick.AddListener(() => ApplyProfile(noJuiceProfile));

        if (fullJuiceButton != null)
            fullJuiceButton.onClick.AddListener(() => ApplyProfile(fullJuiceProfile));
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && panelRoot != null)
            panelRoot.SetActive(!panelRoot.activeSelf);
    }

    private void BuildRows()
    {
        foreach (JuiceModule module in juiceManager.Modules)
        {
            JuiceRowView row = Instantiate(rowPrefab, rowContainer);
            row.Bind(module);
            rows.Add(row);
        }
    }

    private void OnGlobalChanged(float value)
    {
        juiceManager.SetGlobalIntensity(value);
        UpdateGlobalLabel(value);
    }

    private void ApplyProfile(JuiceProfile profile)
    {
        if (profile == null) return;

        juiceManager.ApplyProfile(profile);

        // Profil modüllerin değerlerini değiştirdi; UI'yi ona göre tazele.
        globalSlider.SetValueWithoutNotify(JuiceManager.GlobalIntensity);
        UpdateGlobalLabel(JuiceManager.GlobalIntensity);

        foreach (JuiceRowView row in rows) row.Refresh();
    }

    private void UpdateGlobalLabel(float value)
    {
        if (globalValueLabel != null)
            globalValueLabel.text = $"JUICE  {Mathf.RoundToInt(value * 100f)}%";
    }
}