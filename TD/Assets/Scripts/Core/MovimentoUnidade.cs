using UnityEngine;

public class MovimentoUnidade : MonoBehaviour
{
    public float velocidade = 3f;

    private Transform[] pontos;
    private int pontoAtual = 0;
    private Rigidbody2D rb;
    private bool movimentando = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void DefinirRota(Transform rota)
    {
        int quantidade = rota.childCount;
        pontos = new Transform[quantidade];

        for (int i = 0; i < quantidade; i++)
        {
            pontos[i] = rota.GetChild(i);
        }

        rb.position = pontos[0].position;
        movimentando = true;
        pontoAtual = 0;
    }

    void FixedUpdate()
    {
        if (!movimentando || pontos == null || pontoAtual >= pontos.Length)
            return;

        Transform destino = pontos[pontoAtual];

        Vector2 novaPosicao = Vector2.MoveTowards(
            rb.position,
            destino.position,
            velocidade * Time.fixedDeltaTime
        );

        rb.MovePosition(novaPosicao);

        if (Vector2.Distance(rb.position, destino.position) < 0.05f)
        {
            pontoAtual++;
        }
    }

    public void PararMovimento()
    {
        movimentando = false;
    }

    public void ContinuarMovimento()
    {
        movimentando = true;
    }

    public void ResetarMovimento()
    {
        pontoAtual = 0;
        movimentando = true;
    }
}