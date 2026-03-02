using UnityEngine;

public class MovimentoUnidade : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 3f;
    public float distanciaParada = 0.4f;

    [Header("Separação")]
    public float raioSeparacao = 0.6f;
    public float forcaSeparacao = 2f;

    private Transform[] pontos;
    private int pontoAtual = 0;
    private Rigidbody2D rb;
    private bool movimentando = true;

    private UnidadeController unidade;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        unidade = GetComponent<UnidadeController>();
    }

    public void DefinirRota(Transform rota)
    {
        pontos = new Transform[rota.childCount];

        for (int i = 0; i < rota.childCount; i++)
            pontos[i] = rota.GetChild(i);

        pontoAtual = 0;
        movimentando = true;
    }

    void FixedUpdate()
    {
        if (!movimentando || pontos == null || pontoAtual >= pontos.Length)
            return;

        Vector2 destino = pontos[pontoAtual].position;
        float distancia = Vector2.Distance(rb.position, destino);

        if (distancia <= distanciaParada)
        {
            pontoAtual++;
            return;
        }

        Vector2 direcao = (destino - rb.position).normalized;

        Collider2D[] proximos = Physics2D.OverlapCircleAll(transform.position, raioSeparacao);

        Vector2 separacao = Vector2.zero;
        bool bloqueadoFrente = false;

        foreach (var col in proximos)
        {
            if (col.gameObject == gameObject) continue;

            UnidadeController outra = col.GetComponent<UnidadeController>();
            if (outra == null) continue;

            Vector2 diff = (Vector2)(transform.position - col.transform.position);
            float dist = diff.magnitude;

            if (dist > 0.01f)
                separacao += diff.normalized / dist;

            Vector2 direcaoParaOutra = (col.transform.position - transform.position).normalized;
            float alinhamento = Vector2.Dot(direcao, direcaoParaOutra);

            if (alinhamento > 0.7f)
            {
                MovimentoUnidade movOutra = col.GetComponent<MovimentoUnidade>();

                if (movOutra != null && movOutra.EstaMovimentando())
                    bloqueadoFrente = true;
            }
        }

        if (bloqueadoFrente)
            return;

        Vector2 movimentoFinal = direcao + separacao * forcaSeparacao;

        if (movimentoFinal != Vector2.zero)
            movimentoFinal.Normalize();

        rb.MovePosition(rb.position + movimentoFinal * velocidade * Time.fixedDeltaTime);
    }

    public void PararMovimento()
    {
        movimentando = false;
    }

    public void ContinuarMovimento()
    {
        movimentando = true;
    }

    public bool EstaMovimentando()
    {
        return movimentando;
    }
}