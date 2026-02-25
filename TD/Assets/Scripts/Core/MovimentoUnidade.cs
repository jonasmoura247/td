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
        Debug.Log($"=== DIAGNÓSTICO DO MOVIMENTO ===");
        Debug.Log($"Rigidbody2D: {(rb != null ? "OK" : "NÃO TEM")}");
        if (rb != null)
        {
            Debug.Log($"  - Body Type: {rb.bodyType}");
            Debug.Log($"  - Simulated: {rb.simulated}");
            Debug.Log($"  - Gravity Scale: {rb.gravityScale}");
        }
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
    }

    void FixedUpdate()
    {
        if (!movimentando || pontos == null || pontoAtual >= pontos.Length)
            return;

        Transform destino = pontos[pontoAtual];

        rb.MovePosition(Vector2.MoveTowards(
            rb.position,
            destino.position,
            velocidade * Time.fixedDeltaTime
        ));

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