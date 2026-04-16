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

    private bool _patrulhando = false;
    private Transform _destinoAtual;

    // GameObjects temporários criados em runtime para os pontos
    private GameObject _goA;
    private GameObject _goB;

    public bool EstaPatrulhando => _patrulhando;

    private void Awake()
    {
        _movimento = GetComponent<MovimentoUnidade>();
        _unidade = GetComponent<UnidadeController>();
    }

    private void Update()
    {
        if (!_patrulhando) return;
        if (_unidade != null && !_unidade.EstaVivo)
        {
            _patrulhando = false;
            return;
        }

        // Aguarda enquanto o movimento está suspenso (combate)
        if (!_movimento.EstaMovimentando()) return;

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
        _destinoAtual = pontoA;

        // Sobrescreve velocidade se configurada
        if (velocidadePatrulha > 0f)
            _movimento.velocidade = velocidadePatrulha;
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
        if (rb == null) return;

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
