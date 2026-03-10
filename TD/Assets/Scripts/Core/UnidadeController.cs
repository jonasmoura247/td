using UnityEngine;
using System.Collections;

public class UnidadeController : MonoBehaviour, IDanificavel
{
    [Header("Status Base")]
    public int vidaMaxima = 3;
    protected int vidaAtual;
    public int danoBase = 1;
    protected bool estaVivo = true;

    [Header("Faccao")]
    public Faccao faccao;

    [Header("Configuracaoo de Impacto")]
    [SerializeField] private float distanciaRecuo = 1f;
    [SerializeField] private float duracaoRecuo = 0.2f;
    [SerializeField] private AnimationCurve curvaRecuo = AnimationCurve.EaseInOut(0, 0, 1, 1);

    protected Rigidbody2D rb;
    protected Collider2D col;
    protected MovimentoUnidade movimento;

    private bool sofrendoRecuo = false;

    public bool EstaVivo => estaVivo;

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

        if (vidaAtual <= 0)
            Morrer();
    }

    public void ImpactarCastelo(Vector2 origemImpacto)
    {
        if (!estaVivo || sofrendoRecuo)
            return;

        sofrendoRecuo = true;

        ReceberDano(1);

        if (!estaVivo)
            return;

        StartCoroutine(RecuoControlado(origemImpacto));
    }

    private IEnumerator RecuoControlado(Vector2 origemImpacto)
    {
        if (movimento != null)
            movimento.PararMovimento();

        Vector2 direcao = ((Vector2)transform.position - origemImpacto).normalized;

        Vector2 posInicial = rb.position;
        Vector2 posFinal = posInicial + direcao * distanciaRecuo;

        float tempo = 0f;

        while (tempo < duracaoRecuo)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracaoRecuo);
            float avaliacaoCurva = curvaRecuo.Evaluate(progresso);

            rb.MovePosition(Vector2.Lerp(posInicial, posFinal, avaliacaoCurva));

            yield return null;
        }

        if (movimento != null && estaVivo)
            movimento.ContinuarMovimento();

        sofrendoRecuo = false;
    }

    protected virtual void Morrer()
    {
        if (!estaVivo)
            return;

        estaVivo = false;

        NotificarMorte();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (movimento != null)
            movimento.PararMovimento();

        if (col != null)
            col.enabled = false;

        Destroy(gameObject, 0.1f);
    }

    private void NotificarMorte()
    {
        var componentes = GetComponents<MonoBehaviour>();
        foreach (var componente in componentes)
        {
            if (componente is IMorteListener listener)
                listener.OnMorte(this);
        }
    }

    public int GetVidaAtual() => vidaAtual;

    /// <summary>Retorna o dano que esta unidade causa ao atacar. Sobrescreva em subclasses (ex: GoblinController usa GetDanoFinal).</summary>
    public virtual int GetDanoAtaque() => danoBase;
}