using UnityEngine;

public class CasteloController : EstruturasController
{
    [Header("Defesa do Castelo")]
    public int danoNoInvasor = 1;

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (destruida) return;

        UnidadeController unidade = collision.gameObject.GetComponent<UnidadeController>();

        if (unidade == null) return;

        if (unidade.faccao == this.faccao) return;

        if (!unidade.EstaVivo) return;

        unidade.ReceberDano(danoNoInvasor);

        ReceberDano(unidade.danoBase);
    }
}