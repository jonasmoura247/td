using UnityEngine;

public class GoblinController : UnidadeController
{
    [Header("Configurações Base do Goblin")]
    [SerializeField] protected float multiplicadorDanoRacial = 1f;

    [Header("Configurações de Recuo")]
    public float tempoInvulnerabilidade = 0.2f;
    public float forcaRecuo = 5f;

    protected float proximoTempoDano = 0f;

    public virtual void AplicarRecuo(Vector2 direcao, float forca)
    {
        if (rb == null || !estaVivo) return;

        if (movimento != null)
            movimento.PararMovimento();

        Vector2 novaPos = rb.position + direcao.normalized * forca * 0.2f;
        rb.MovePosition(novaPos);

        proximoTempoDano = Time.time + tempoInvulnerabilidade;

        CancelInvoke(nameof(ReativarMovimento));
        Invoke(nameof(ReativarMovimento), 0.3f);
    }

    protected virtual void ReativarMovimento()
    {
        if (movimento != null && estaVivo)
        {
            movimento.ContinuarMovimento();
        }
    }

    public float GetProximoTempoDano() => proximoTempoDano;

    public virtual int GetDanoFinal() => Mathf.RoundToInt(danoBase * multiplicadorDanoRacial);

    public override void ReceberDano(int quantidade)
    {
        if (Time.time < proximoTempoDano)
        {
            Debug.Log($"{name} está invulnerável!");
            return;
        }

        base.ReceberDano(quantidade);
    }
}