using UnityEngine;

public class CasteloController : EstruturasController
{
    protected override void Start()
    {
        base.Start();
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (destruida) return;

        UnidadeController unidade = collision.gameObject.GetComponent<UnidadeController>();
        if (unidade == null) return;
        if (unidade.faccao == this.faccao) return;
        if (!unidade.EstaVivo) return;

        int danoBase = unidade.GetDanoAtaque();
        int dano = BestiarioAtaqueInimigo.CalcularDanoAoCastelo(unidade, danoBase, null, out bool critico);
        var pedido = PedidoDanoFlutuante.Criar(dano, TipoDanoCombate.Fisico, critico);
        ServicoAplicacaoDano.Aplicar(this, transform.position, dano, pedido, dispararProcsRunaBestiario: false);
        unidade.ImpactarCastelo(transform.position);

        if (unidade is GoblinController goblin)
            Debug.Log($"[Castelo] {unidade.name} causou {dano} de dano (base: {unidade.danoBase}, buff aura: +{goblin.GetBonusDanoAura()} = total {dano})");
        else
            Debug.Log($"[Castelo] {unidade.name} causou {dano} de dano");
    }
}