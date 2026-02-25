using UnityEngine;

public class UnidadeController : MonoBehaviour
{
    [Header("Status Base")]
    public int vidaMaxima = 3;
    protected int vidaAtual;
    public int danoBase = 1;
    protected bool estaVivo = true;

    protected Rigidbody2D rb;
    protected Collider2D col;
    protected MovimentoUnidade movimento;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        movimento = GetComponent<MovimentoUnidade>();
    }

    protected virtual void Start()
    {
        vidaAtual = vidaMaxima;
    }

    public virtual void ReceberDano(int quantidade)
    {
        if (!estaVivo) return;

        vidaAtual -= quantidade;
        if (vidaAtual <= 0) Morrer();
    }

    protected virtual void Morrer()
    {
        estaVivo = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (movimento != null) movimento.PararMovimento();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 0.1f);
    }

    public int GetVidaAtual() => vidaAtual;
}