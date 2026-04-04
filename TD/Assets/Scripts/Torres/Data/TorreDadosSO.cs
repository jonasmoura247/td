using UnityEngine;

[CreateAssetMenu(fileName = "TorreDados", menuName = "TD/Torre Dados", order = 0)]
public class TorreDadosSO : ScriptableObject
{
    [Header("Identidade")]
    public TipoRaca raca = TipoRaca.Goblin;
    public string nomeExibicao = "Torre";

    [Header("Economia e vida base")]
    public int custoConstrucao = 0;
    public int vidaMaxima = 50;

    [Header("Combate")]
    public int danoBase = 1;
    public float alcanceAtaque = 8f;
    public float intervaloEntreAtaques = 1.5f;

    [Header("Projétil (reutiliza LancaProjetil do GoblinLanceiro)")]
    public GameObject prefabProjetil;
    public float velocidadeProjetil = 60f;

    [Header("Deteção")]
    public LayerMask layerMaskAlvos;

    [Header("Animação (opcional)")]
    [Tooltip("Nome do parâmetro trigger no Animator. Vazio = disparo imediato como GoblinLanceiro.")]
    public string nomeTriggerAtaque = "";

    [Header("Debug")]
    public bool mostrarGizmos = true;
}
