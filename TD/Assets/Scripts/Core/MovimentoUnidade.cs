using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovimentoUnidade : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 10f;
    public float distanciaParada = 0.3f;

    [Header("Espa�o F�sico Virtual")]
    public float raioUnidade = 1.5f;
    public float forcaSeparacao = 25f;
    public LayerMask layerUnidades;

    private Transform[] pontos;
    private int pontoAtual = 0;

    private Rigidbody2D rb;
    private bool movimentando = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void Start()
    {
        ResolverSobreposicaoInicial();
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
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 posicaoAtual = rb.position;
        Vector2 destino = pontos[pontoAtual].position;

        float distancia = Vector2.Distance(posicaoAtual, destino);

        if (distancia <= distanciaParada)
        {
            pontoAtual++;
            return;
        }

        Vector2 direcao = (destino - posicaoAtual).normalized;

        Vector2 repulsao = CalcularSeparacao();

        Vector2 velocidadeFinal = (direcao * velocidade) + repulsao;

        rb.linearVelocity = velocidadeFinal;
    }

    private Vector2 CalcularSeparacao()
    {
        Collider2D[] proximos = Physics2D.OverlapCircleAll(
            transform.position,
            raioUnidade * 2f,
            layerUnidades
        );

        Vector2 repulsao = Vector2.zero;

        foreach (var col in proximos)
        {
            if (col.gameObject == gameObject) continue;

            Vector2 diff = (Vector2)(transform.position - col.transform.position);
            float dist = diff.magnitude;

            if (dist <= 0.001f)
            {
                diff = Random.insideUnitCircle.normalized;
                dist = 0.01f;
            }

            float distanciaMinima = raioUnidade * 2f;

            if (dist < distanciaMinima)
            {
                float forca = (distanciaMinima - dist) / distanciaMinima;
                repulsao += diff.normalized * forca;
            }
        }

        return repulsao * forcaSeparacao;
    }

    private void ResolverSobreposicaoInicial()
    {
        Collider2D[] proximos = Physics2D.OverlapCircleAll(
            transform.position,
            raioUnidade * 2f,
            layerUnidades
        );

        foreach (var col in proximos)
        {
            if (col.gameObject == gameObject) continue;

            Vector2 diff = (Vector2)(transform.position - col.transform.position);
            float dist = diff.magnitude;

            if (dist == 0)
            {
                diff = Random.insideUnitCircle.normalized;
                dist = 0.01f;
            }

            float distanciaMinima = raioUnidade * 2f;

            if (dist < distanciaMinima)
            {
                float ajuste = distanciaMinima - dist;
                transform.position += (Vector3)(diff.normalized * ajuste * 0.5f);
            }
        }
    }

    public void PararMovimento()
    {
        movimentando = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void ContinuarMovimento()
    {
        movimentando = true;
    }

    public bool EstaMovimentando()
    {
        return movimentando;
    }

    /// <summary>�ndice do waypoint para o qual a unidade se dirige (somente leitura).</summary>
    public int IndiceWaypointAtual => pontoAtual;

    /// <summary>Quantidade de pontos na rota atual, ou 0 se n�o houver rota.</summary>
    public int TotalPontosNaRota => pontos != null ? pontos.Length : 0;

    /// <summary>
    /// Dist�ncia aproximada restante ao longo da polilinha at� o �ltimo ponto.
    /// Menor valor = unidade mais avan�ada no percurso (�til para prioridade de torres TD).
    /// </summary>
    public float CalcularDistanciaRestanteNaRota()
    {
        if (pontos == null || pontos.Length == 0)
            return float.MaxValue;

        if (pontoAtual >= pontos.Length)
            return 0f;

        Vector2 pos = rb != null ? rb.position : (Vector2)transform.position;
        float total = Vector2.Distance(pos, pontos[pontoAtual].position);

        for (int i = pontoAtual; i < pontos.Length - 1; i++)
        {
            total += Vector2.Distance(pontos[i].position, pontos[i + 1].position);
        }

        return total;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, raioUnidade);
    }
}