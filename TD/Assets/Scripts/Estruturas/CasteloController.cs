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

        ReceberDano(unidade.danoBase);
        unidade.ImpactarCastelo(transform.position);
    }
}