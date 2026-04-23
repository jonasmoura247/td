using UnityEngine;

/// <summary>
/// Permite que uma unidade aliada patrulhe entre dois pontos (A ↔ B).
///
/// Como usar em jogo:
///   1. Selecione a unidade (por referência ou clique).
///   2. Chame <see cref="IniciarMarcacaoPontoA"/> → clique no mundo → <see cref="DefinirPontoA"/>.
///   3. Chame <see cref="IniciarMarcacaoPontoB"/> → clique no mundo → <see cref="DefinirPontoB"/>.
///   4. A patrulha começa automaticamente.
///   5. Chame <see cref="CancelarPatrulha"/> para retomar comportamento normal.
///
/// A patrulha é suspensa automaticamente quando a unidade entra em combate
/// (qualquer subclasse que chame PararMovimento/ContinuarMovimento manterá compatibilidade,
/// pois a patrulha relança o ciclo quando o movimento é retomado).
/// </summary>
[RequireComponent(typeof(MovimentoUnidade))]
public class PatrulhaUnidade : MonoBehaviour
{
    [Header("Pontos de patrulha (definíveis em runtime)")]
    [SerializeField] private Transform pontoA;
    [SerializeField] private Transform pontoB;

    [Header("Configuração")]
    [Tooltip("Distância mínima para considerar o ponto atingido e inverter direção.")]
    [SerializeField] private float distanciaChegada = 0.4f;
    [Tooltip("Velocidade de patrulha (0 = usa a velocidade padrão da unidade).")]
    [SerializeField] private float velocidadePatrulha = 0f;

    private MovimentoUnidade _movimento;
    private UnidadeController _unidade;
    private SistemaIAUnidade _sistemaIA;

    private bool _patrulhando = false;
    private bool _patrulhaSuspensa = false;
    private bool _pausada = false;
    private Transform _destinoAtual;

    // GameObjects temporários criados em runtime para os pontos
    private GameObject _goA;
    private GameObject _goB;

    public bool EstaPatrulhando => _patrulhando;
    /// <summary>Verdadeiro quando o jogador pausou manualmente a patrulha.</summary>
    public bool IsPausada => _pausada;

    private void Awake()
    {
        _movimento = GetComponent<MovimentoUnidade>();
        _unidade = GetComponent<UnidadeController>();
        _sistemaIA = GetComponent<SistemaIAUnidade>();
    }

    private void Update()
    {
        if (!_patrulhando || _patrulhaSuspensa || _pausada) return;
        if (_unidade != null && !_unidade.EstaVivo)
        {
            _patrulhando = false;
            return;
        }

        if (_destinoAtual == null) return;

        float dist = Vector2.Distance(transform.position, _destinoAtual.position);
        if (dist <= distanciaChegada)
            InverterDestino();
        else
            MoverAteDestino();
    }

    // ─── API pública ─────────────────────────────────────────────────────────

    /// <summary>Inicia a patrulha com os pontos já atribuídos (<see cref="pontoA"/> e <see cref="pontoB"/>).</summary>
    public void IniciarPatrulha()
    {
        if (pontoA == null || pontoB == null)
        {
            Debug.LogWarning("[PatrulhaUnidade] Ponto A ou B não definido.");
            return;
        }

        _patrulhando = true;
        float distA = Vector2.Distance(transform.position, pontoA.position);
        _destinoAtual = distA <= distanciaChegada ? pontoB : pontoA;

        if (_sistemaIA != null)
        {
            // SistemaIA gerencia a suspensão: começa suspensa, SistemaIA chama RetomarPatrulha
            _patrulhaSuspensa = true;
        }
        else
        {
            // Sem SistemaIA: assume controle direto do rb.
            // EntrarModoLivre impede FixedUpdate do MovimentoUnidade de zerar a velocidade.
            _patrulhaSuspensa = false;
            _movimento.EntrarModoLivre();
            // Seta velocidade imediatamente para não depender do timing do Update.
            MoverAteDestino();
        }

        Debug.Log($"[PatrulhaUnidade] {name} — IniciarPatrulha. A={pontoA.position} B={pontoB.position} | suspensa={_patrulhaSuspensa} (temIA={_sistemaIA != null})");

        // Sobrescreve velocidade se configurada
        if (velocidadePatrulha > 0f)
            _movimento.velocidade = velocidadePatrulha;
    }

    /// <summary>Suspende o movimento de patrulha sem cancelá-la (chamado pelo SistemaIA ao entrar em combate).</summary>
    public void SuspenderPatrulha()
    {
        _patrulhaSuspensa = true;
        // Zera velocidade explicitamente: quando em ModoLivre, MovimentoUnidade.FixedUpdate
        // retorna sem tocar no rb, então nada mais zeraria a velocidade ao suspender.
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Debug.Log($"[PatrulhaUnidade] {name} — SuspenderPatrulha");
    }

    /// <summary>Retoma o movimento de patrulha após combate (chamado pelo SistemaIA ao entrar em estado Patrulhando).</summary>
    public void RetomarPatrulha()
    {
        _patrulhaSuspensa = false;
        Debug.Log($"[PatrulhaUnidade] {name} — RetomarPatrulha");
    }

    /// <summary>
    /// Pausa manualmente a patrulha por ação do jogador.
    /// A unidade para imediatamente onde está. Não interfere com suspensão por combate.
    /// </summary>
    public void PausarPatrulha()
    {
        if (!_patrulhando || _pausada) return;
        _pausada = true;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Debug.Log($"[PatrulhaUnidade] {name} — PausarPatrulha (manual)");
    }

    /// <summary>
    /// Despausa a patrulha, retomando movimento em direção ao destino atual.
    /// </summary>
    public void DespausarPatrulha()
    {
        if (!_patrulhando || !_pausada) return;
        _pausada = false;
        Debug.Log($"[PatrulhaUnidade] {name} — DespausarPatrulha (manual)");
    }

    /// <summary>Alterna entre pausado/despausado. Conveniente para botão de UI.</summary>
    public void AlternarPausaPatrulha()
    {
        if (_pausada) DespausarPatrulha(); else PausarPatrulha();
    }

    /// <summary>Define o Ponto A via posição do mundo (cria um GameObject interno).</summary>
    public void DefinirPontoA(Vector3 posicaoMundo)
    {
        if (_goA == null)
        {
            _goA = new GameObject($"[Patrulha] {name} PontoA");
            _goA.hideFlags = HideFlags.HideInHierarchy;
        }
        _goA.transform.position = posicaoMundo;
        pontoA = _goA.transform;
        TentarIniciarSeCompleto();
    }

    /// <summary>Define o Ponto B via posição do mundo (cria um GameObject interno).</summary>
    public void DefinirPontoB(Vector3 posicaoMundo)
    {
        if (_goB == null)
        {
            _goB = new GameObject($"[Patrulha] {name} PontoB");
            _goB.hideFlags = HideFlags.HideInHierarchy;
        }
        _goB.transform.position = posicaoMundo;
        pontoB = _goB.transform;
        TentarIniciarSeCompleto();
    }

    /// <summary>Para a patrulha e retoma o comportamento normal de movimento.</summary>
    public void CancelarPatrulha()
    {
        _patrulhando = false;
        _destinoAtual = null;
        // Devolve controle ao MovimentoUnidade se éramos nós que tínhamos ModoLivre
        if (_sistemaIA == null)
            _movimento.PararMovimento();
    }

    // ─── Internos ────────────────────────────────────────────────────────────

    private void TentarIniciarSeCompleto()
    {
        if (pontoA != null && pontoB != null)
            IniciarPatrulha();
    }

    private void MoverAteDestino()
    {
        // Move o Rigidbody2D diretamente (sem depender da rota de waypoints)
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) { Debug.LogWarning($"[PatrulhaUnidade] {name} — sem Rigidbody2D!"); return; }

        Vector2 direcao = ((Vector2)_destinoAtual.position - rb.position).normalized;
        float vel = velocidadePatrulha > 0f ? velocidadePatrulha : _movimento.velocidade;
        rb.linearVelocity = direcao * vel;
    }

    private void InverterDestino()
    {
        _destinoAtual = (_destinoAtual == pontoA) ? pontoB : pontoA;
    }

    private void OnDestroy()
    {
        if (_goA != null) Destroy(_goA);
        if (_goB != null) Destroy(_goB);
    }

    private void OnDrawGizmosSelected()
    {
        if (pontoA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pontoA.position, 0.25f);
        }
        if (pontoB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(pontoB.position, 0.25f);
        }
        if (pontoA != null && pontoB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pontoA.position, pontoB.position);
        }
    }
}
