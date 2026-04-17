using System.Collections;
using UnityEngine;

/// <summary>
/// Sistema de IA reutilizável para qualquer unidade (aliada ou inimiga).
///
/// ── Estados ───────────────────────────────────────────────────────────────
///  Ocioso     → sem alvo, segue waypoints (castelo)
///  Perseguindo→ alvo detectado, movimento livre em direção ao alvo
///  Atacando   → alvo dentro do alcance (por distância), velocidade = 0, ataca
///  Combate    → colisão física com inimigo, velocidade = 0, ataca, sem empurrão
///  Morto      → tudo parado
///
/// ── Prioridade ────────────────────────────────────────────────────────────
///  Unidade atacante (Jogador) > Castelo (waypoints)
///  Torres ignoradas (não têm UnidadeController → filtradas automaticamente)
///  Após matar → retorna Ocioso → retoma waypoints → segue para o castelo
///
/// ── Anti-empurrão ─────────────────────────────────────────────────────────
///  Em Atacando e Combate: rb.linearVelocity zerada todo FixedUpdate.
///  Em Combate: rb.bodyType = Kinematic (física de empurrão desabilitada por completo).
///
/// ── Anti-stack ────────────────────────────────────────────────────────────
///  Separação de aliados (CalcularForcaSeparacaoAliados) — sem alocação GC.
///  Repulsão do inimigo (CalcularRepulsaoDeInimigo) em Atacando e Combate:
///    impede que aliados por trás joguem esta unidade para dentro do inimigo.
///  distanciaMinimaMelee controla a distância mínima mantida (padrão: 65% alcanceAtaque).
///
/// ── Anti-preso ────────────────────────────────────────────────────────────
///  Detecção de stuck: se a unidade não avançou em N segundos → steering lateral.
///  Desvio de obstáculos: CircleCast à frente + força perpendicular ao bloqueio.
/// </summary>
[RequireComponent(typeof(UnidadeController))]
public class SistemaIAUnidade : MonoBehaviour
{
    // ─── Detecção ─────────────────────────────────────────────────────────

    [Header("Detecção (Aggro / Scan)")]
    [Tooltip("Raio de detecção de inimigos.")]
    public float alcanceDeteccao = 12f;

    [Tooltip("Intervalo entre scans (segundos).")]
    [Range(0.05f, 1f)]
    public float intervaloScan = 0.15f;

    [Tooltip("Layer dos alvos inimigos. Se vazio, detecta automaticamente pela facção.")]
    public LayerMask layerAlvos;

    // ─── Ataque ──────────────────────────────────────────────────────────

    [Header("Ataque")]
    [Tooltip("Distância máxima para transição Perseguindo→Atacando.")]
    public float alcanceAtaque = 1.8f;

    [Tooltip("Cooldown entre ataques (segundos).")]
    public float intervaloAtaque = 1f;

    // ─── Perseguição e Anti-Stack ─────────────────────────────────────────

    [Header("Perseguição e Anti-Stack")]
    [Tooltip("Se verdadeiro, a unidade persegue o alvo quando fora do alcance de ataque.")]
    public bool perseguirAlvo = true;

    [Tooltip("Velocidade de perseguição. 0 = usa MovimentoUnidade.velocidade.")]
    public float velocidadePerseguicao = 0f;

    [Tooltip("Raio virtual da unidade para cálculo de separação entre aliados.")]
    public float raioSeparacao = 0.8f;

    [Tooltip("Intensidade da força de repulsão entre unidades aliadas próximas.")]
    public float forcaSeparacao = 20f;

    [Tooltip("Layer de unidades para cálculo de separação anti-stack.")]
    public LayerMask layerUnidades;

    [Tooltip("Distância mínima mantida do inimigo nos estados Atacando e Combate. " +
             "0 = usa 65% do alcanceAtaque automaticamente. " +
             "Evita que empurrões de aliados joguem a unidade para dentro do inimigo.")]
    public float distanciaMinimaMelee = 0f;

    // ─── Desvio de obstáculos ─────────────────────────────────────────────

    [Header("Desvio de Obstáculos")]
    [Tooltip("Layer de paredes/obstáculos para desvio frontal. 0 = desativado.")]
    public LayerMask layerObstaculos;

    [Tooltip("Distância à frente para detectar bloqueio (CircleCast).")]
    public float distanciaDeteccaoBloqueio = 1.2f;

    // ─── Limites de tela ──────────────────────────────────────────────────

    [Header("Limites de Tela")]
    [Tooltip("Clamp de posição dentro da tela. 0 = desativado.")]
    public float margemTela = 0.5f;

    // ─── Prioridade ───────────────────────────────────────────────────────

    [Header("Prioridade de Alvo")]
    public TipoPrioridadeAlvo prioridade = TipoPrioridadeAlvo.MaisProximo;

    // ─── Estado (leitura) ─────────────────────────────────────────────────

    public EstadoIA Estado { get; private set; } = EstadoIA.Ocioso;
    public UnidadeController AlvoAtual { get; private set; }

    // ─── Internos ─────────────────────────────────────────────────────────

    private UnidadeController _eu;
    private MovimentoUnidade  _movimento;
    private Rigidbody2D       _rb;

    private float _proximoAtaque;

    // Offset angular único por instância: distribui unidades ao redor do alvo
    // mesmo quando todas chegam pela mesma direção (ex: spawn no mesmo lado).
    private Vector2 _offsetSlotAtaque;

    // Buffer reutilizável — sem GC no scan de alvos
    private readonly Collider2D[] _bufferScan = new Collider2D[32];
    // Buffer reutilizável — sem GC no cálculo de separação
    private readonly Collider2D[] _bufferSep  = new Collider2D[16];

    // Cache dos limites de câmera (atualizado uma vez no Start)
    private Vector2 _limiteMin;
    private Vector2 _limiteMax;
    private bool    _limitesCacheValidos;

    // Anti-preso (stuck detection)
    private Vector2 _posicaoAnterior;
    private float   _tempoVerificacaoStuck;
    private float   _tempoParado;
    private Vector2 _forcaDesvioExtra;
    private float   _tempoFimDesvioExtra;

    private const float MargemSaidaAtaque         = 0.6f;
    private const float IntervaloVerificacaoStuck = 0.5f;
    private const float LimiteTempoParado         = 1.5f;
    private const float DeslocamentoMinimoStuck   = 0.12f;

    // ─────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _eu       = GetComponent<UnidadeController>();
        _movimento = GetComponent<MovimentoUnidade>();
        _rb       = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Auto-detecta layer se não configurado
        if (layerAlvos.value == 0)
        {
            string nomeLayer = _eu.faccao == Faccao.Jogador ? "Inimigo" : "Jogador";
            int idx = LayerMask.NameToLayer(nomeLayer);
            if (idx >= 0) layerAlvos = 1 << idx;
        }

        // Auto-detecta layer própria para separação entre aliados se não configurado no prefab
        if (layerUnidades.value == 0)
        {
            string nomeLayerProprio = _eu.faccao == Faccao.Jogador ? "Jogador" : "Inimigo";
            int idxProprio = LayerMask.NameToLayer(nomeLayerProprio);
            if (idxProprio >= 0) layerUnidades = 1 << idxProprio;
        }

        if (_rb != null)
            _posicaoAnterior = _rb.position;

        // Offset angular fixo por instância: cada agente aponta para um "slot"
        // diferente no círculo de ataque ao redor do alvo, evitando que unidades
        // vindas da mesma direção convirjam para o exato mesmo ponto.
        float ang = (GetInstanceID() % 32) / 32f * Mathf.PI * 2f;
        _offsetSlotAtaque = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * (alcanceAtaque * 0.45f);

        CachearLimitesTela();
        StartCoroutine(LoopScan());
        StartCoroutine(LoopDeteccaoOverlap());
    }

    private void Update()
    {
        if (!_eu.EstaVivo)
        {
            if (Estado != EstadoIA.Morto) MudarEstado(EstadoIA.Morto);
            return;
        }

        // Retarget imediato se alvo morreu
        if (AlvoAtual != null && !AlvoAtual.EstaVivo)
            LimparAlvo();

        AtualizarMaquinaEstados();

        if (margemTela > 0f && _eu.faccao == Faccao.Jogador)
            ClampDentroTela();
    }

    private void FixedUpdate()
    {
        switch (Estado)
        {
            case EstadoIA.Perseguindo:
                if (AlvoAtual != null && perseguirAlvo)
                    MoverParaAlvo();
                break;

            case EstadoIA.Atacando:
                // Separação de aliados + repulsão mínima do inimigo para evitar stack/overlap.
                // Remove componente "para trás" (afastando do alvo) para evitar ping-pong Atacando↔Perseguindo.
                if (_rb != null)
                {
                    Vector2 sep = CalcularForcaSeparacaoAliados() * 0.55f;
                    sep += CalcularRepulsaoDeInimigo();
                    if (AlvoAtual != null)
                    {
                        Vector2 dirAlvo = ((Vector2)AlvoAtual.transform.position - _rb.position).normalized;
                        float backward = Vector2.Dot(sep, -dirAlvo);
                        if (backward > 0) sep += dirAlvo * backward;
                    }
                    _rb.linearVelocity = sep;
                }
                break;

            case EstadoIA.Combate:
                // rb.bodyType = Kinematic (setado em MudarEstado) — só nudge de separação via MovePosition
                AplicarSeparacaoKinematica();
                break;
        }
    }

    // ─── Colisão física → entra em Combate ───────────────────────────────

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!_eu.EstaVivo || Estado == EstadoIA.Morto) return;

        // GetComponentInParent garante detecção mesmo quando o controller está no pai
        UnidadeController alvo = col.gameObject.GetComponentInParent<UnidadeController>();
        if (alvo == null || !alvo.EstaVivo || alvo.faccao == _eu.faccao) return;

        AlvoAtual = alvo;
        MudarEstado(EstadoIA.Combate);
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (Estado != EstadoIA.Combate) return;

        UnidadeController alvo = col.gameObject.GetComponentInParent<UnidadeController>();
        if (alvo == null || alvo != AlvoAtual) return;

        // Perdeu contato físico — continua perseguindo se alvo ainda vivo
        if (AlvoAtual != null && AlvoAtual.EstaVivo)
            MudarEstado(perseguirAlvo ? EstadoIA.Perseguindo : EstadoIA.Atacando);
        else
            LimparAlvo();
    }

    // ─── Scan ─────────────────────────────────────────────────────────────

    private IEnumerator LoopScan()
    {
        yield return new WaitForSeconds(Random.Range(0f, intervaloScan));

        while (true)
        {
            yield return new WaitForSeconds(intervaloScan);

            if (!_eu.EstaVivo || Estado == EstadoIA.Morto)
                continue;

            Escanear();
        }
    }

    /// <summary>
    /// Verifica a cada 0.3s se há unidades inimigas ou aliadas muito próximas e loga alertas.
    /// Não altera comportamento — apenas observação passiva para diagnóstico.
    /// Threshold inimigo: 40% do alcanceAtaque. Threshold aliado: raioSeparacao.
    /// </summary>
    private IEnumerator LoopDeteccaoOverlap()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.3f)); // stagger para não sincronizar

        while (true)
        {
            yield return new WaitForSeconds(0.3f);

            if (!_eu.EstaVivo || Estado == EstadoIA.Morto) continue;

            // ── Verifica sobreposição com inimigos ────────────────────────
            if (layerAlvos.value != 0)
            {
                float thresholdInimigo = alcanceAtaque * 0.4f;
                var filtro = new ContactFilter2D();
                filtro.SetLayerMask(layerAlvos);
                int count = Physics2D.OverlapCircle(transform.position, thresholdInimigo, filtro, _bufferScan);

                for (int i = 0; i < count; i++)
                {
                    if (_bufferScan[i] == null) continue;
                    UnidadeController uc = _bufferScan[i].GetComponentInParent<UnidadeController>();
                    if (uc == null || uc == _eu || !uc.EstaVivo) continue;

                }
            }

            // ── Verifica crowding entre aliados ───────────────────────────
            if (layerUnidades.value != 0)
            {
                float thresholdAliado = raioSeparacao * 0.5f;
                var filtroAl = new ContactFilter2D();
                filtroAl.SetLayerMask(layerUnidades);
                int countAl = Physics2D.OverlapCircle(transform.position, thresholdAliado, filtroAl, _bufferSep);

                for (int i = 0; i < countAl; i++)
                {
                    if (_bufferSep[i] == null || _bufferSep[i].gameObject == gameObject) continue;
                    UnidadeController uc = _bufferSep[i].GetComponentInParent<UnidadeController>();
                    if (uc == null || uc.faccao != _eu.faccao) continue;
                }
            }
        }
    }

    private void Escanear()
    {
        // Em Combate ou Atacando com alvo válido: não troca alvo
        if (AlvoAtual != null && AlvoAtual.EstaVivo &&
            (Estado == EstadoIA.Atacando || Estado == EstadoIA.Combate))
            return;

        var filtroScan = new ContactFilter2D();
        filtroScan.SetLayerMask(layerAlvos);
        int count = Physics2D.OverlapCircle(
            transform.position, alcanceDeteccao, filtroScan, _bufferScan);

        if (count == 0)
        {
            if (AlvoAtual != null) LimparAlvo();
            return;
        }

        UnidadeController melhor = SelecionarAlvo(count);

        if (melhor != null && melhor != AlvoAtual)
        {
            AlvoAtual = melhor;
            if (Estado == EstadoIA.Ocioso)
                MudarEstado(DefinirProximoEstadoParaAlvo());
        }
        else if (melhor == null && AlvoAtual != null)
        {
            LimparAlvo();
        }
    }

    // ─── Seleção de alvo (sem alocação) ──────────────────────────────────

    private UnidadeController SelecionarAlvo(int count)
    {
        UnidadeController melhor = null;
        float melhorScore = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = _bufferScan[i];
            if (col == null) continue;

            // Torres e estruturas não têm UnidadeController → ignoradas automaticamente
            UnidadeController u = col.GetComponentInParent<UnidadeController>();
            if (u == null || u == _eu || !u.EstaVivo || u.faccao == _eu.faccao)
                continue;

            float score = CalcularScore(u);
            if (score < melhorScore)
            {
                melhorScore = score;
                melhor = u;
            }
        }

        return melhor;
    }

    private float CalcularScore(UnidadeController alvo)
    {
        switch (prioridade)
        {
            case TipoPrioridadeAlvo.MenorVida:
                return alvo.GetVidaAtual();

            case TipoPrioridadeAlvo.MaiorAmeaca:
                return -alvo.danoBase;

            case TipoPrioridadeAlvo.TipoAlvo:
                MovimentoUnidade mov = alvo.GetComponent<MovimentoUnidade>();
                return mov != null
                    ? mov.CalcularDistanciaRestanteNaRota()
                    : Vector2.Distance(transform.position, alvo.transform.position);

            default: // MaisProximo
                return Vector2.Distance(transform.position, alvo.transform.position);
        }
    }

    // ─── Máquina de estados ───────────────────────────────────────────────

    private void AtualizarMaquinaEstados()
    {
        switch (Estado)
        {
            case EstadoIA.Ocioso:
                if (AlvoAtual != null)
                    MudarEstado(DefinirProximoEstadoParaAlvo());
                break;

            case EstadoIA.Perseguindo:
                if (AlvoAtual == null)
                {
                    MudarEstado(EstadoIA.Ocioso);
                    break;
                }
                if (DistanciaAoAlvo() <= alcanceAtaque)
                    MudarEstado(EstadoIA.Atacando);
                break;

            case EstadoIA.Atacando:
                if (AlvoAtual == null)
                {
                    MudarEstado(EstadoIA.Ocioso);
                    break;
                }
                {
                    float distAtacando = DistanciaAoAlvo();
                    // Fallback por distância: entra em Combate mesmo sem OnCollisionEnter2D
                    // (útil quando as layers Jogador/Inimigo não estão configuradas para colidir fisicamente)
                    if (alcanceAtaque > 0f && distAtacando < alcanceAtaque * 0.4f)
                    {
                        MudarEstado(EstadoIA.Combate);
                        break;
                    }
                    // Histerese: evita ping-pong Perseguindo↔Atacando
                    if (distAtacando > alcanceAtaque + MargemSaidaAtaque)
                    {
                        MudarEstado(perseguirAlvo ? EstadoIA.Perseguindo : EstadoIA.Ocioso);
                        break;
                    }
                }
                TentarAtacar();
                break;

            case EstadoIA.Combate:
                if (AlvoAtual == null)
                {
                    MudarEstado(EstadoIA.Ocioso);
                    break;
                }
                TentarAtacar();
                break;
        }
    }

    private void MudarEstado(EstadoIA novo)
    {
        if (Estado == novo) return;

        EstadoIA anterior = Estado;

        // ── Saída ─────────────────────────────────────────────────────────
        switch (Estado)
        {
            case EstadoIA.Perseguindo:
                // Devolve controle ao sistema de waypoints e para o rb
                _movimento?.EntrarModoWaypoints();
                if (_rb != null) _rb.linearVelocity = Vector2.zero;
                break;

            case EstadoIA.Atacando:
                // Retoma movimento ao sair do estado de ataque (waypoints ou modo livre entram em seguida)
                _movimento?.ContinuarMovimento();
                break;

            case EstadoIA.Combate:
                // Restaura física normal ao sair do combate
                if (_rb != null)
                {
                    _rb.bodyType = RigidbodyType2D.Dynamic;
                    _rb.linearVelocity = Vector2.zero;
                }
                break;
        }

        Estado = novo;

        // ── Entrada ───────────────────────────────────────────────────────
        switch (Estado)
        {
            case EstadoIA.Perseguindo:
                // Cede controle de velocidade ao SistemaIA; MovimentoUnidade para de tocar no rb
                _movimento?.EntrarModoLivre();
                ResetarDeteccaoStuck();
                break;

            case EstadoIA.Atacando:
                // CRÍTICO: PararMovimento() para waypoints (movimentando=false).
                // EntrarModoWaypoints() NÃO deve ser usado aqui — ele retomaria a marcha
                // para o castelo enquanto a unidade deveria estar parada atacando.
                _movimento?.PararMovimento();
                if (_rb != null) _rb.linearVelocity = Vector2.zero;
                break;

            case EstadoIA.Combate:
                // Kinematic: elimina empurrão físico entre unidades
                _movimento?.PararMovimento();
                if (_rb != null)
                {
                    _rb.linearVelocity = Vector2.zero;
                    _rb.bodyType = RigidbodyType2D.Kinematic;
                }
                break;

            case EstadoIA.Ocioso:
            case EstadoIA.Morto:
                _movimento?.EntrarModoWaypoints();
                if (_rb != null)
                {
                    _rb.bodyType = RigidbodyType2D.Dynamic;
                    _rb.linearVelocity = Vector2.zero;
                }
                break;
        }
    }

    private EstadoIA DefinirProximoEstadoParaAlvo()
    {
        if (AlvoAtual == null) return EstadoIA.Ocioso;
        return DistanciaAoAlvo() <= alcanceAtaque
            ? EstadoIA.Atacando
            : (perseguirAlvo ? EstadoIA.Perseguindo : EstadoIA.Ocioso);
    }

    // ─── Ataque ───────────────────────────────────────────────────────────

    private void TentarAtacar()
    {
        if (Time.time < _proximoAtaque) return;
        if (AlvoAtual == null || !AlvoAtual.EstaVivo) return;

        _proximoAtaque = Time.time + intervaloAtaque;

        _eu.ExecutarAtaqueIA(AlvoAtual);
    }

    // ─── Movimento livre (Perseguindo) com desvio e anti-stuck ───────────

    private void MoverParaAlvo()
    {
        if (_rb == null || AlvoAtual == null) return;

        // Freio imediato no FixedUpdate: se já está dentro do alcance, para na hora.
        // Sem isso, o movimento continua até o próximo Update() detectar a distância,
        // causando overshoot de 0.2–0.5u quando ambas as unidades se aproximam.
        Vector2 posAlvo = AlvoAtual.transform.position;
        Vector2 diff = posAlvo - _rb.position;
        float distAtual = diff.magnitude;

        if (distAtual <= alcanceAtaque)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        // Aponta para a borda do alcance de ataque, não para o centro do alvo.
        // _offsetSlotAtaque distribui unidades em slots ao redor do alvo.
        Vector2 destino = distAtual > 0.01f
            ? posAlvo - diff.normalized * (alcanceAtaque * 0.85f) + _offsetSlotAtaque
            : posAlvo;
        // Garante que o offset do slot não empurre destino para fora do alcance de ataque.
        // Sem isso, se o offset apontar para longe do alvo, a unidade chega ao destino mas
        // fica fora do alcanceAtaque e nunca transiciona para Atacando, gerando tremor.
        if (alcanceAtaque > 0f)
        {
            Vector2 desvio = destino - posAlvo;
            float maxDesvio = alcanceAtaque * 0.85f;
            if (desvio.sqrMagnitude > maxDesvio * maxDesvio)
                destino = posAlvo + desvio.normalized * maxDesvio;
        }
        Vector2 direcaoBase = (destino - _rb.position).normalized;
        float vel = velocidadePerseguicao > 0f
            ? velocidadePerseguicao
            : (_movimento != null ? _movimento.velocidade : 6f);

        // Desvio de obstáculos estáticos (paredes)
        Vector2 direcao = AplicarDesvioObstaculo(direcaoBase);

        // Anti-stuck: se preso por muito tempo, força steering lateral
        AtualizarDeteccaoStuck(direcaoBase);
        if (Time.time < _tempoFimDesvioExtra)
            direcao = (direcao + _forcaDesvioExtra * 0.6f).normalized;

        // Separação apenas de aliados (não empurra do inimigo)
        // Remove componente que freia o avanço ao alvo (evita que aliados atrás desacelerem a perseguição).
        Vector2 separacao = CalcularForcaSeparacaoAliados();
        float backwardSep = Vector2.Dot(separacao, -direcaoBase);
        if (backwardSep > 0) separacao += direcaoBase * backwardSep;
        _rb.linearVelocity = (direcao * vel) + separacao;
    }

    /// <summary>
    /// Detecta bloqueio frontal via CircleCast e retorna a direção corrigida.
    /// Se <see cref="layerObstaculos"/> não estiver configurada, retorna a direção original.
    /// </summary>
    private Vector2 AplicarDesvioObstaculo(Vector2 direcao)
    {
        if (layerObstaculos.value == 0 || _rb == null) return direcao;

        RaycastHit2D hit = Physics2D.CircleCast(
            _rb.position, 0.3f, direcao, distanciaDeteccaoBloqueio, layerObstaculos);

        if (!hit) return direcao;

        // Força perpendicular à normal do obstáculo; escolhe o lado mais próximo do alvo
        Vector2 perp = new Vector2(-hit.normal.y, hit.normal.x);
        if (AlvoAtual != null)
        {
            Vector2 paraAlvo = ((Vector2)AlvoAtual.transform.position - _rb.position).normalized;
            if (Vector2.Dot(perp, paraAlvo) < 0f) perp = -perp;
        }

        return (direcao + perp).normalized;
    }

    /// <summary>
    /// Detecta se a unidade está presa (sem deslocamento significativo).
    /// Quando presa por <see cref="LimiteTempoParado"/> segundos, aplica steering lateral aleatório.
    /// </summary>
    private void AtualizarDeteccaoStuck(Vector2 direcaoAtual)
    {
        if (_rb == null || Time.time < _tempoVerificacaoStuck) return;
        _tempoVerificacaoStuck = Time.time + IntervaloVerificacaoStuck;

        float delta = Vector2.Distance(_rb.position, _posicaoAnterior);
        _posicaoAnterior = _rb.position;

        if (delta < DeslocamentoMinimoStuck)
        {
            _tempoParado += IntervaloVerificacaoStuck;

            if (_tempoParado >= LimiteTempoParado)
            {
                // Força perpendicular aleatória para desbloquear
                Vector2 perp = new Vector2(-direcaoAtual.y, direcaoAtual.x);
                _forcaDesvioExtra = perp * (Random.value > 0.5f ? 1f : -1f);
                _tempoFimDesvioExtra = Time.time + 0.8f;
                _tempoParado = 0f;
            }
        }
        else
        {
            _tempoParado = 0f;
        }
    }

    private void ResetarDeteccaoStuck()
    {
        _tempoParado = 0f;
        _tempoFimDesvioExtra = 0f;
        if (_rb != null) _posicaoAnterior = _rb.position;
    }

    // ─── Separação (apenas aliados) ───────────────────────────────────────

    /// <summary>
    /// Separação anti-stack sem alocação GC (NonAlloc buffer por instância).
    /// Apenas unidades da MESMA facção são repelidas — inimigos são ignorados
    /// para não empurrar a unidade para longe do alvo que está atacando.
    /// </summary>
    private Vector2 CalcularForcaSeparacaoAliados()
    {
        if (layerUnidades.value == 0) return Vector2.zero;

        float diametro = raioSeparacao * 2f;
        var filtroSep = new ContactFilter2D();
        filtroSep.SetLayerMask(layerUnidades);
        int count = Physics2D.OverlapCircle(
            transform.position, diametro, filtroSep, _bufferSep);

        Vector2 forca = Vector2.zero;
        for (int i = 0; i < count; i++)
        {
            Collider2D col = _bufferSep[i];
            if (col == null || col.gameObject == gameObject) continue;

            // Ignora unidades de facção oposta: não queremos ser empurrados do inimigo
            UnidadeController uc = col.GetComponentInParent<UnidadeController>();
            if (uc != null && uc.faccao != _eu.faccao) continue;

            Vector2 diff = (Vector2)(transform.position - col.transform.position);
            float dist = diff.magnitude;

            if (dist <= 0.001f)
            {
                // Direção determinística: cada par de unidades sempre se separa em direções opostas.
                // Evita que ambas recebam a mesma direção aleatória e permaneçam sobrepostas.
                int meuId = gameObject.GetInstanceID();
                int outroId = col.gameObject.GetInstanceID();
                int hash = Mathf.Abs(meuId ^ outroId);
                float ang = (hash % 628) * 0.01f; // 0 a ~2π
                if (meuId < outroId) ang += Mathf.PI;
                diff = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                dist = 0.01f;
            }

            if (dist < diametro)
                forca += diff.normalized * ((diametro - dist) / diametro);
        }

        return forca * forcaSeparacao;
    }

    /// <summary>
    /// Repulsão suave do inimigo alvo quando a unidade está mais próxima que
    /// <see cref="distanciaMinimaMelee"/> (padrão = 65% do <see cref="alcanceAtaque"/>).
    /// Impede que aliados empurrando por trás joguem esta unidade para dentro do inimigo
    /// nos estados Atacando e Combate.
    /// </summary>
    private Vector2 CalcularRepulsaoDeInimigo()
    {
        if (AlvoAtual == null) return Vector2.zero;

        float minDist = distanciaMinimaMelee > 0f ? distanciaMinimaMelee : alcanceAtaque * 0.65f;
        Vector2 diff = (Vector2)(transform.position - AlvoAtual.transform.position);
        float dist = diff.magnitude;

        if (dist <= 0.001f)
            return Random.insideUnitCircle.normalized * forcaSeparacao;

        if (dist >= minDist) return Vector2.zero;

        float intensidade = (minDist - dist) / minDist;
        return diff.normalized * intensidade * forcaSeparacao;
    }

    /// <summary>
    /// Separação suave via MovePosition para unidade kinematic em Combate.
    /// Repele de aliados (anti-stack) e mantém distância mínima do inimigo alvo
    /// via <see cref="CalcularRepulsaoDeInimigo"/> para desfazer overlaps físicos.
    /// </summary>
    private void AplicarSeparacaoKinematica()
    {
        if (_rb == null || layerUnidades.value == 0) return;

        float diametro = raioSeparacao * 2f;
        var filtroSep = new ContactFilter2D();
        filtroSep.SetLayerMask(layerUnidades);
        int count = Physics2D.OverlapCircle(
            transform.position, diametro, filtroSep, _bufferSep);

        Vector2 forca = Vector2.zero;
        for (int i = 0; i < count; i++)
        {
            Collider2D col = _bufferSep[i];
            if (col == null || col.gameObject == gameObject) continue;

            // Ignora inimigos: não nos afastamos de quem estamos combatendo
            UnidadeController uc = col.GetComponentInParent<UnidadeController>();
            if (uc != null && uc.faccao != _eu.faccao) continue;

            Vector2 diff = (Vector2)(transform.position - col.transform.position);
            float dist = diff.magnitude;

            if (dist <= 0.001f)
            {
                // Direção determinística: garante que dois aliados sobrepostos
                // recebam forças opostas e se separem mesmo quando o inimigo está próximo.
                int meuId = gameObject.GetInstanceID();
                int outroId = col.gameObject.GetInstanceID();
                int hash = Mathf.Abs(meuId ^ outroId);
                float ang = (hash % 628) * 0.01f;
                if (meuId < outroId) ang += Mathf.PI;
                diff = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                dist = 0.01f;
            }

            if (dist < diametro)
                forca += diff.normalized * ((diametro - dist) / diametro);
        }

        // Escala a separação de aliados antes de adicionar repulsão do inimigo,
        // para que a força de separação seja forte o suficiente mesmo quando o inimigo
        // está muito próximo (evita que repulsão do inimigo domine e impeça separação).
        forca *= forcaSeparacao;

        // Repulsão suave do inimigo: desfaz overlap quando OnCollisionEnter2D disparou
        // enquanto os colliders já se sobrepunham (kinematic-kinematic não resolve pela física).
        forca += CalcularRepulsaoDeInimigo() * 0.5f;

        if (forca.sqrMagnitude > 0.001f)
        {
            Vector2 novaPos = _rb.position + forca.normalized * (Time.fixedDeltaTime * 4f);
            _rb.MovePosition(novaPos);
        }
    }

    // ─── Screen Clamp ──────────────────────────────────────────────────────

    private void CachearLimitesTela()
    {
        if (Camera.main == null) return;

        _limiteMin = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        _limiteMax = Camera.main.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
        _limitesCacheValidos = true;
    }

    private void ClampDentroTela()
    {
        if (!_limitesCacheValidos || _rb == null) return;

        Vector2 pos = _rb.position;
        float clampX = Mathf.Clamp(pos.x, _limiteMin.x + margemTela, _limiteMax.x - margemTela);
        float clampY = Mathf.Clamp(pos.y, _limiteMin.y + margemTela, _limiteMax.y - margemTela);

        if (clampX != pos.x || clampY != pos.y)
            _rb.position = new Vector2(clampX, clampY);
    }

    // ─── Utilitários ──────────────────────────────────────────────────────

    private float DistanciaAoAlvo()
    {
        if (AlvoAtual == null) return float.MaxValue;
        return Vector2.Distance(transform.position, AlvoAtual.transform.position);
    }

    private void LimparAlvo()
    {
        AlvoAtual = null;
        MudarEstado(EstadoIA.Ocioso);
    }

    // ─── Debug Gizmos ──────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, alcanceDeteccao);

        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, alcanceAtaque);

        if (AlvoAtual != null)
        {
            Gizmos.color = Estado == EstadoIA.Combate ? Color.magenta : Color.red;
            Gizmos.DrawLine(transform.position, AlvoAtual.transform.position);
        }

        // Desvio de obstáculos: visualiza o CircleCast à frente
        if (Application.isPlaying && _rb != null && layerObstaculos.value != 0 && Estado == EstadoIA.Perseguindo && AlvoAtual != null)
        {
            Vector2 dir = ((Vector2)AlvoAtual.transform.position - _rb.position).normalized;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(_rb.position, _rb.position + dir * distanciaDeteccaoBloqueio);
        }
    }
}
