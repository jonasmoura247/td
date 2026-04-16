using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Modo de colocação de unidades: preview + alcance, clique no mapa; ouro ao confirmar via <see cref="UnidadesManager"/>.
/// </summary>
public class ServicoColocacaoUnidade : MonoBehaviour
{
    public static ServicoColocacaoUnidade Instancia { get; private set; }

    public bool ModoColocacaoAtivo { get; private set; }

    [Header("Visual")]
    [SerializeField] private Color corFantasma = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] private int sortingOrderFantasma = 5;

    [Header("Opcional")]
    [SerializeField] private LayerMask camadaBloqueiaColocacao;
    [SerializeField] private float raioVerificacaoBloqueio = 0.35f;
    [SerializeField] private bool bloquearQuandoPonteiroSobreUI = false;

    private Transform _raizPreview;
    private SpriteRenderer _spriteFantasma;
    private IndicadorAlcanceTorre2D _indicadorAlcance;

    private GameObject _prefabAtual;
    private DadosSlotConstrucaoUnidade _dadosAtual;
    private string _nomeInstanciaAtual;
    private int _indiceSlotSelecionado = -1;
    private int _frameInicioColocacao = -1;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        CriarHierarquiaPreview();
    }

    private void OnDestroy()
    {
        if (_raizPreview != null)
        {
            Destroy(_raizPreview.gameObject);
            _raizPreview = null;
        }

        if (Instancia == this)
            Instancia = null;
    }

    private void CriarHierarquiaPreview()
    {
        _raizPreview = new GameObject("PreviewColocacaoUnidade").transform;
        _raizPreview.SetParent(null, false);
        _raizPreview.gameObject.SetActive(false);

        var goFantasma = new GameObject("Fantasma");
        goFantasma.transform.SetParent(_raizPreview, false);
        _spriteFantasma = goFantasma.AddComponent<SpriteRenderer>();
        _spriteFantasma.sortingOrder = sortingOrderFantasma;

        var goAlcance = new GameObject("Alcance");
        goAlcance.transform.SetParent(_raizPreview, false);
        _indicadorAlcance = goAlcance.AddComponent<IndicadorAlcanceTorre2D>();
    }

    private void Update()
    {
        if (!ModoColocacaoAtivo)
            return;

        AtualizarPosicaoPreview(ObterPontoMundoPlanoZZero());

        bool cliqueDireitoCancelamento =
            (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
            Input.GetMouseButtonDown(1);
        if (cliqueDireitoCancelamento)
        {
            CancelarColocacao();
            return;
        }

        bool escapeCancelamento =
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
            Input.GetKeyDown(KeyCode.Escape);
        if (escapeCancelamento)
        {
            CancelarColocacao();
            return;
        }

        if (!CliqueEsquerdoConfirmacaoNesteFrame())
            return;

        // Evita confirmar no mesmo frame do clique no botão do slot.
        if (Time.frameCount <= _frameInicioColocacao)
            return;

        if (bloquearQuandoPonteiroSobreUI && PointerSobreUI())
            return;

        TentarConfirmarColocacao(ObterPontoMundoPlanoZZero());
    }

    public void IniciarColocacao(DadosSlotConstrucaoUnidade slot, int indiceSlot = -1)
    {
        if (slot == null)
            return;

        GameObject prefabSelecionado = slot.ObterPrefabAleatorio(out bool feminino);
        if (prefabSelecionado == null)
            return;

        if (ServicoColocacaoTorre.Instancia != null && ServicoColocacaoTorre.Instancia.ModoColocacaoAtivo)
            ServicoColocacaoTorre.Instancia.CancelarColocacao();

        PainelUnidadesController painel = FindFirstObjectByType<PainelUnidadesController>(FindObjectsInactive.Include);
        painel?.FecharPainelSeAberto();

        _prefabAtual = prefabSelecionado;
        _dadosAtual = slot;
        _nomeInstanciaAtual = slot.ObterNomeInstancia(prefabSelecionado, feminino);
        _indiceSlotSelecionado = indiceSlot;
        _frameInicioColocacao = Time.frameCount;

        Sprite sp = UtilSpritesPrefab.ObterSpritePrincipal(prefabSelecionado);
        if (_spriteFantasma != null)
        {
            _spriteFantasma.sprite = sp;
            _spriteFantasma.color = corFantasma;
            if (sp != null)
                _spriteFantasma.flipX = ObterFlipXDoPrefab(prefabSelecionado);
        }

        _indicadorAlcance.DefinirRaio(slot.alcanceIndicadorPreview);

        ModoColocacaoAtivo = true;
        _raizPreview.gameObject.SetActive(true);
        AtualizarPosicaoPreview(ObterPontoMundoPlanoZZero());
    }
    public void CancelarColocacao()
    {
        ModoColocacaoAtivo = false;
        _prefabAtual = null;
        _dadosAtual = null;
        _nomeInstanciaAtual = null;
        _indiceSlotSelecionado = -1;
        _frameInicioColocacao = -1;
        if (_raizPreview != null)
            _raizPreview.gameObject.SetActive(false);
    }

    private void AtualizarPosicaoPreview(Vector3 posicaoMundo)
    {
        if (_raizPreview == null)
            return;
        _raizPreview.position = posicaoMundo;
    }

    private void TentarConfirmarColocacao(Vector3 posicaoMundo)
    {
        if (_prefabAtual == null || _dadosAtual == null)
            return;

        if (!PosicaoValida(posicaoMundo))
            return;

        UnidadesManager mgr = UnidadesManager.Instancia;
        if (mgr == null)
            mgr = FindFirstObjectByType<UnidadesManager>(FindObjectsInactive.Exclude);

        if (OuroManager.Instance == null || !OuroManager.Instance.GastarOuro(_dadosAtual.custoOuro))
            return;

        if (mgr != null)
        {
            UnidadeController uc = mgr.InstanciarUnidade(
                _prefabAtual,
                posicaoMundo,
                Quaternion.identity,
                _nomeInstanciaAtual);
            if (uc != null)
            {
                uc.DefinirFaccaoParaSpawn(Faccao.Jogador);
            }
        }
        else
        {
            GameObject instancia = Instantiate(_prefabAtual, posicaoMundo, Quaternion.identity);
            if (!string.IsNullOrEmpty(_nomeInstanciaAtual))
                instancia.name = _nomeInstanciaAtual;
            UnidadeController uc = instancia.GetComponent<UnidadeController>();
            if (uc != null)
            {
                uc.DefinirFaccaoParaSpawn(Faccao.Jogador);
            }
        }

        CancelarColocacao();
    }

    private bool PosicaoValida(Vector3 posicaoMundo)
    {
        if (camadaBloqueiaColocacao.value == 0)
            return true;

        Collider2D bloqueio = Physics2D.OverlapCircle(posicaoMundo, raioVerificacaoBloqueio, camadaBloqueiaColocacao);
        return bloqueio == null;
    }

    /// <summary>Posição na tela usada para preview, UI e conversão mundo (mouse ou toque).</summary>
    private static Vector2 ObterPosicaoPonteiroTela()
    {
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.position.ReadValue();
        return Input.mousePosition;
    }

    private static Vector3 ObterPontoMundoPlanoZZero()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return Vector3.zero;

        Vector2 screen = ObterPosicaoPonteiroTela();
        Ray ray = cam.ScreenPointToRay(screen);
        var plano = new Plane(Vector3.forward, Vector3.zero);
        if (plano.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        float zDist = Mathf.Abs(cam.transform.position.z);
        Vector3 w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, zDist));
        w.z = 0f;
        return w;
    }

    private static bool CliqueEsquerdoConfirmacaoNesteFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;
        return Input.GetMouseButtonDown(0);
    }

    private static bool PointerSobreUI()
    {
        if (EventSystem.current == null)
            return false;

        if (Mouse.current != null)
            return EventSystem.current.IsPointerOverGameObject(Mouse.current.deviceId);

        return EventSystem.current.IsPointerOverGameObject();
    }

    private static bool ObterFlipXDoPrefab(GameObject prefab)
    {
        if (prefab == null)
            return false;
        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = prefab.GetComponentInChildren<SpriteRenderer>(true);
        return sr != null && sr.flipX;
    }

    public int IndiceSlotSelecionado => _indiceSlotSelecionado;
}

