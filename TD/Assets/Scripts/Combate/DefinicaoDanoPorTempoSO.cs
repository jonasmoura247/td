using UnityEngine;

[CreateAssetMenu(fileName = "DefinicaoDanoPorTempo", menuName = "TD/Dano por tempo", order = 2)]
public class DefinicaoDanoPorTempoSO : ScriptableObject
{
    [Header("Identidade")]
    public TipoEfeitoStatusDot tipoEfeito = TipoEfeitoStatusDot.Queimadura;

    [Header("Dano")]
    [Min(1)] public int danoPorTick = 1;
    [Min(0.05f)] public float intervaloSegundos = 1f;
    [Min(1)] public int quantidadeTicks = 3;

    [Header("Visual unidade (multiplicador por SpriteRenderer.color)")]
    public Color corTint = Color.white;

    [Header("Texto flutuante")]
    [Tooltip("Prefab com TextoDanoFlutuante na raiz (Canvas world space + TMP).")]
    public GameObject prefabTextoFlutuante;
    public Color corDoTexto = Color.white;
}
