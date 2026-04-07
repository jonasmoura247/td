using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Modo de colocação: preview + alcance, clique no mapa para instanciar via TorresManager.
/// </summary>
public class ServicoColocacaoTorre : MonoBehaviour
{
    public static ServicoColocacaoTorre Instancia { get; private set; }

    /// <summary>Verdadeiro enquanto o jogador está a posicionar uma torre.</summary>
    public bool ModoColocacaoAtivo { get; private set; }

    [Header("Visual")]
    [SerializeField] private Color corFantasma = new Color(1f, 1f, 1f, 0.45f);
    [SerializeField] private int sortingOrderFantasma = 5;

    [Header("Opcional")]
    [SerializeField] private LayerMask camadaBloqueiaColocacao;
    [SerializeField] private float raioVerificacaoBloqueio = 0.35f;

    private Transform _raizPreview;
    private SpriteRenderer _spriteFantasma;
    private IndicadorAlcanceTorre2D _indicadorAlcance;

    private GameObject _prefabAtual;
    private TorreDadosSO _dadosAtual;
    private int _indiceSlotSelecionado = -1;

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
        // Raiz em mundo: o serviço pode estar no Canvas; o preview segue coordenadas 2D do mapa.
        _raizPreview = new GameObject("PreviewColocacaoTorre").transform;
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

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelarColocacao();
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelarColocacao();
            return;
        }

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (PointerSobreUI())
            return;

        TentarConfirmarColocacao(ObterPontoMundoPlanoZZero());
    }

    /// <summary>Inicia colocação a partir de um slot da HUD.</summary>
    public void IniciarColocacao(DadosSlotConstrucaoTorre slot, int indiceSlot = -1)
    {
        if (slot == null || slot.prefabTorre == null || slot.dados == null)
            return;

        if (ServicoColocacaoUnidade.Instancia != null && ServicoColocacaoUnidade.Instancia.ModoColocacaoAtivo)
            ServicoColocacaoUnidade.Instancia.CancelarColocacao();

        _prefabAtual = slot.prefabTorre;
        _dadosAtual = slot.dados;
        _indiceSlotSelecionado = indiceSlot;

        Sprite sp = UtilSpritesPrefab.ObterSpritePrincipal(slot.prefabTorre);
        if (_spriteFantasma != null)
        {
            _spriteFantasma.sprite = sp;
            _spriteFantasma.color = corFantasma;
            if (sp != null)
            {
                _spriteFantasma.flipX = ObterFlipXDoPrefab(slot.prefabTorre);
            }
        }

        _indicadorAlcance.DefinirRaio(_dadosAtual.alcanceAtaque);

        ModoColocacaoAtivo = true;
        _raizPreview.gameObject.SetActive(true);
        AtualizarPosicaoPreview(ObterPontoMundoPlanoZZero());
    }

    public void CancelarColocacao()
    {
        ModoColocacaoAtivo = false;
        _prefabAtual = null;
        _dadosAtual = null;
        _indiceSlotSelecionado = -1;
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
        if (_prefabAtual == null)
            return;

        if (!PosicaoValida(posicaoMundo))
            return;

        TorresManager mgr = TorresManager.Instancia;
        if (mgr == null)
            mgr = FindFirstObjectByType<TorresManager>(FindObjectsInactive.Exclude);

        if (mgr != null)
            mgr.InstanciarTorre(_prefabAtual, posicaoMundo, Quaternion.identity);

        CancelarColocacao();
    }

    private bool PosicaoValida(Vector3 posicaoMundo)
    {
        if (camadaBloqueiaColocacao.value == 0)
            return true;

        Collider2D bloqueio = Physics2D.OverlapCircle(posicaoMundo, raioVerificacaoBloqueio, camadaBloqueiaColocacao);
        return bloqueio == null;
    }

    private static Vector3 ObterPontoMundoPlanoZZero()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return Vector3.zero;

        Vector2 screen = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : (Vector2)Input.mousePosition;

        float zDist = Mathf.Abs(cam.transform.position.z);
        Vector3 w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, zDist));
        w.z = 0f;
        return w;
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

/// <summary>Extrai sprite principal do prefab para ícones / fantasma.</summary>
public static class UtilSpritesPrefab
{
    public static Sprite ObterSpritePrincipal(GameObject prefab)
    {
        if (prefab == null)
            return null;

        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = prefab.GetComponentInChildren<SpriteRenderer>(true);
        return sr != null ? sr.sprite : null;
    }
}
