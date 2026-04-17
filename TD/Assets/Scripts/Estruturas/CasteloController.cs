using System.Collections.Generic;
using UnityEngine;

public class CasteloController : EstruturasController
{
    [Header("Ataque ao Castelo")]
    [Tooltip("Intervalo mínimo (em segundos) entre ataques de uma mesma unidade ao castelo.")]
    [SerializeField] private float intervaloAtaqueInimigo = 1f;

    /// <summary>Rastreia o momento do último ataque de cada unidade para throttle.</summary>
    private readonly Dictionary<UnidadeController, float> _ultimoAtaque
        = new Dictionary<UnidadeController, float>();

    protected override void Start()
    {
        base.Start();
    }

    // ─── Colisão contínua: unidades melee permanecem encostadas e atacam ──────

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessarAtaqueAoCastelo(collision.gameObject);
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        ProcessarAtaqueAoCastelo(collision.gameObject);
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        // Limpa o registro ao sair (morte, knockback externo, etc.)
        UnidadeController unidade = collision.gameObject.GetComponent<UnidadeController>();
        if (unidade != null)
            _ultimoAtaque.Remove(unidade);
    }

    private void ProcessarAtaqueAoCastelo(GameObject go)
    {
        if (destruida) return;

        UnidadeController unidade = go.GetComponent<UnidadeController>();
        if (unidade == null || unidade.faccao == faccao || !unidade.EstaVivo)
            return;

        float agora = Time.time;
        if (_ultimoAtaque.TryGetValue(unidade, out float ultimo) &&
            agora - ultimo < intervaloAtaqueInimigo)
            return;

        _ultimoAtaque[unidade] = agora;

        int danoBase = unidade.GetDanoAtaque();
        int dano = BestiarioAtaqueInimigo.CalcularDanoAoCastelo(unidade, danoBase, null, out bool critico);
        var pedido = PedidoDanoFlutuante.Criar(dano, TipoDanoCombate.Fisico, critico);
        ServicoAplicacaoDano.Aplicar(this, transform.position, dano, pedido, dispararProcsRunaBestiario: false);

        // Para o movimento (sem recuo): unidade fica ao redor do castelo
        unidade.ImpactarCastelo(transform.position);

        if (unidade is GoblinController goblin)
            Debug.Log($"[Castelo] {unidade.name} causou {dano} de dano (crit:{critico}, buff aura: +{goblin.GetBonusDanoAura()})");
        else
            Debug.Log($"[Castelo] {unidade.name} causou {dano} de dano (crit:{critico})");
    }
}
