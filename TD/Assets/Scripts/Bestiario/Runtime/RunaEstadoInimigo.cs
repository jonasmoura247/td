using UnityEngine;

/// <summary>Estado de runa aplicado ao spawn (somente leitura em runtime).</summary>
public class RunaEstadoInimigo : MonoBehaviour
{
    public bool temDanoEco;
    public float chanceDanoEco;
    [Tooltip("Fração da vida máxima (ex.: 0.05)")]
    public float fracaoVidaParaEco = 0.05f;

    public bool temCriticoAprimorado;
    public float multiplicadorCriticoComRuna = 1.5f;

    public float chanceCritico = 0.2f;
    public float multiplicadorCriticoBase = 1.2f;
}
