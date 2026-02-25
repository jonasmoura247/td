using UnityEngine;

public class CasteloController : EstruturasController
{
    [Header("Defesa do Castelo")]
    public int danoNoInvasor = 1;
    public float forcaRecuo = 8f;

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (destruida) return;

        
        UnidadeController unidade = collision.gameObject.GetComponent<UnidadeController>();

        if (unidade != null)
        {
            
            if (unidade is GoblinController goblin)
            {
                if (Time.time < goblin.GetProximoTempoDano()) return;

                goblin.ReceberDano(danoNoInvasor);
                Vector2 direcao = -collision.contacts[0].normal;
                goblin.AplicarRecuo(direcao, forcaRecuo);
            }

            ReceberDano(unidade.danoBase);
        }
    }
}