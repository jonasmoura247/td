using UnityEngine;

[CreateAssetMenu(fileName = "ConfiguracaoExplosaoCarvaoEnxofreSalitre", menuName = "TD/Reliquias/Explosão Carvão-Enxofre-Salitre")]
public class ConfiguracaoExplosaoCarvaoEnxofreSalitreSO : ScriptableObject
{
    [Min(0)] public int danoExplosao = 30;
    public TipoDanoCombate tipoDano = TipoDanoCombate.ExplosaoAlquimica;
    [Range(0f, 1f)] public float chanceProc = 1f;
    [Min(0f)] public float cooldownPorAlvoSegundos = 0f;
    public bool usarCorPersonalizada = false;
    public Color corTextoExplosao = new Color(1f, 0.85f, 0.2f, 1f);
    [Min(0f)] public float raioAreaFuturo = 0f;
}
