using UnityEngine;

public class GoblinController : UnidadeController
{
    [Header("Atributos Raciais Goblin")]
    [SerializeField] protected float multiplicadorDanoRacial = 1f;

    [Header("Configurações de Recuo")]
    public float tempoInvulnerabilidade = 0f;
    private float proximoTempoDano = 0f;

    protected override void Start()
    {
        vidaMaxima = 3;
        danoBase = 1;
        base.Start();
    }

    public void AplicarRecuo(Vector2 direcao, float forca)
    {
        if (rb == null) return;

        if (movimento != null) movimento.PararMovimento();

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direcao * forca, ForceMode2D.Impulse);

        proximoTempoDano = Time.time + tempoInvulnerabilidade;

        CancelInvoke(nameof(ReativarMovimento));
        Invoke(nameof(ReativarMovimento), 0.3f);
    }

    private void ReativarMovimento()
    {
        if (movimento != null && estaVivo)
        {
            movimento.ContinuarMovimento();
        }
    }

    public float GetProximoTempoDano() => proximoTempoDano;

    public int GetDanoFinal() => Mathf.RoundToInt(danoBase * multiplicadorDanoRacial);
}