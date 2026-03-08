using UnityEngine;
using System.Collections.Generic;

public class BossGoblinNivel3 : GoblinController
{
    [Header("Configuracao da Aura")]
    [SerializeField] private float raioAura = 4f;
    [SerializeField] private int bonusDanoAura = 10;
    [SerializeField] private float intervaloAtualizacao = 0.5f;

    [SerializeField] private LayerMask camadaAlvoAura;

    [Header("Visual")]
    [SerializeField] private Transform auraVisual;

    private float tempoProximaAtualizacao;
    private List<GoblinController> goblinsBuffados = new List<GoblinController>();

    protected override void Start()
    {
        AjustarTamanhoAuraVisual();
        base.Start();
    }

    private void Update()
    {
        if (!estaVivo)
            return;

        if (Time.time >= tempoProximaAtualizacao)
        {
            AtualizarAura();
            tempoProximaAtualizacao = Time.time + intervaloAtualizacao;
        }
    }

    private void AtualizarAura()
    {
        // Remove buff antigo
        foreach (var goblin in goblinsBuffados)
        {
            if (goblin != null)
                goblin.RemoverBonusDano(bonusDanoAura);
        }

        goblinsBuffados.Clear();

        Collider2D[] coliders = Physics2D.OverlapCircleAll(
            transform.position,
            raioAura,
            camadaAlvoAura
        );

        foreach (var col in coliders)
        {
            GoblinController goblin = col.GetComponent<GoblinController>();

            if (goblin != null && goblin != this)
            {
                goblin.AplicarBonusDano(bonusDanoAura);
                goblinsBuffados.Add(goblin);
            }
        }
    }

    private void AjustarTamanhoAuraVisual()
    {
        if (auraVisual == null)
            return;

        float diametro = raioAura * 2f;
        auraVisual.localScale = Vector3.one * diametro;
    }

    protected override void Morrer()
    {
        foreach (var goblin in goblinsBuffados)
        {
            if (goblin != null)
                goblin.RemoverBonusDano(bonusDanoAura);
        }

        goblinsBuffados.Clear();

        base.Morrer();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0f, 0.8f, 0.7f); // roxo
        Gizmos.DrawWireSphere(transform.position, raioAura);
    }
}