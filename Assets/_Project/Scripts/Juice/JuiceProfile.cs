using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hazır juice ayar setleri. Kod değiştirmeden farklı "his reçeteleri"
/// kaydedip anında geçiş yapabilmek için.
/// </summary>
[CreateAssetMenu(fileName = "JuiceProfile", menuName = "JuiceLab/Juice Profile")]
public class JuiceProfile : ScriptableObject
{
    [Serializable]
    public class ModuleSetting
    {
        [Tooltip("Modülün DisplayName değeriyle birebir aynı olmalı.")]
        public string moduleName;
        public bool enabled = true;
        [Range(0f, 1f)] public float intensity = 1f;
    }

    public string profileName = "New Profile";
    [Range(0f, 1f)] public float globalIntensity = 1f;
    public List<ModuleSetting> moduleSettings = new List<ModuleSetting>();
}