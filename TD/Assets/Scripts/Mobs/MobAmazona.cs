using UnityEngine;

/// <summary>
/// Mob ranged. Dispara projéteis via LancaProjetil — o prefab deve ser atribuído no Inspector.
/// Usa SistemaIAUnidade para gerenciar perseguição; ExecutarAtaqueIA sobrescreve para disparar.
/// </summary>
public class MobAmazona : MobController
{
    [Header("Ataque Ranged")]
    public GameObject projetilPrefab;
    public Transform pontoDisparo;

    protected override void Start()
    {
        vidaMaxima = 70;
        danoBase   = 16;
        movimento.velocidade = 7f;
        base.Start();
    }

    public override void ExecutarAtaqueIA(UnidadeController alvo)
    {
        if (alvo == null || !alvo.EstaVivo) return;
        if (projetilPrefab == null) return;

        NotificarAtaque();

        Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
        GameObject proj = Instantiate(projetilPrefab, origem, Quaternion.identity);
        LancaProjetil p = proj.GetComponent<LancaProjetil>();
        if (p != null)
            p.Inicializar(alvo.transform, GetDanoAtaque(), gameObject.name);
    }
}
