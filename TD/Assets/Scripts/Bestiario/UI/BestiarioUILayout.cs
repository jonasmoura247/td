using UnityEngine;

[CreateAssetMenu(fileName = "BestiarioUILayout", menuName = "TD/Bestiario/UI Layout")]
public class BestiarioUILayout : ScriptableObject
{
    [Header("Prefabs (instanciados em runtime)")]
    public GameObject prefabSlotCriatura;
    public GameObject prefabLinhaRuna;
    public GameObject prefabItemCatalogoRuna;

    [Header("Grelha de criaturas")]
    [Min(1)] public int colunasGrelha = 3;
    public float espacamentoColunas = 24f;
    public float espacamentoLinhas = 16f;
    public float larguraMinimaColuna = 128f;
    public bool contentAlturaAutomatica;

    [Header("Catálogo de runas")]
    [Range(0.15f, 1f)] public float alphaRunaBloqueadaCatalogo = 0.42f;
}
