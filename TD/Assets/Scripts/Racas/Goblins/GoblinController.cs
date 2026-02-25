using UnityEngine;

public class GoblinController : UnidadeController
{
    [Header("Atributos Raciais Goblin")]
    protected float multiplicadorDanoRacial = 1f;

    [Header("Configura��es de Recuo")]
    public float tempoInvulnerabilidade = 1f;
    private float proximoTempoDano = 0f;
    private Rigidbody2D rb;
    private MovimentoUnidade movimento;

    protected override void Start()
    {
        base.Start();

        // Configura��es padr�o goblin
        vidaMaxima = 3;
        danoBase = 1;

        rb = GetComponent<Rigidbody2D>();
        movimento = GetComponent<MovimentoUnidade>();

        if (rb == null)
        {
            Debug.LogError("Goblin precisa de um Rigidbody2D!");
        }
    }

    public void AplicarBuffRacial(float bonus)
    {
        multiplicadorDanoRacial += bonus;
    }

    public int GetDanoFinal()
    {
        return Mathf.RoundToInt(danoBase * multiplicadorDanoRacial);
    }

    public void AplicarRecuo(Vector2 direcao, float forca)
    {
        if (rb != null)
        {
            // Para o movimento atual
            if (movimento != null)
            {
                movimento.PararMovimento();
            }

            // Aplica for�a de recuo
            rb.linearVelocity = Vector2.zero; // Reseta velocidade atual
            rb.AddForce(direcao * forca, ForceMode2D.Impulse);

            // Define o tempo para o pr�ximo dano
            proximoTempoDano = Time.time + tempoInvulnerabilidade;

            // Reativa o movimento ap�s um pequeno delay
            Invoke(nameof(ReativarMovimento), 0.3f);
        }
    }

    private void ReativarMovimento()
    {
        if (movimento != null)
        {
            movimento.ContinuarMovimento();
        }
    }

    public float GetProximoTempoDano()
    {
        return proximoTempoDano;
    }

    protected override void Morrer()
    {
        Debug.Log("Goblin morreu!");
        base.Morrer();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"🔵 [GOBLIN] OnCollisionEnter2D disparado! Colidiu com: {collision.gameObject.name}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"🟢 [GOBLIN] OnTriggerEnter2D disparado! Trigger com: {other.gameObject.name}");
    }
}