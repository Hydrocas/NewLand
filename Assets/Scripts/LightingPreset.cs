using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName ="Lighting Preset", menuName ="Scriptables/Lighting Preset", order = 1)]
public class LightingPreset : ScriptableObject
{
    [SerializeField] private Gradient _ambientColor = null;
    [SerializeField] private Gradient _directionalColor = null;

    public Gradient AmbiantColor => _ambientColor;
    public Gradient DirectionalColor => _directionalColor;
}
