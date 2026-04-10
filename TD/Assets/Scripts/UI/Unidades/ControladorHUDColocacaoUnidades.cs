using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD de unidades de combate por raça: slots delegam a <see cref="ServicoColocacaoUnidade"/> (como torres).
/// </summary>
public class ControladorHUDColocacaoUnidades : MonoBehaviour
{
    [SerializeField] private ServicoColocacaoUnidade servicoColocacao;

    [SerializeField] private Button[] botoesSlots = new Button[2];

    [Tooltip("Ícone por slot (filho Icone ou primeiro Image filho). Opcional.")]
    [SerializeField] private Image[] iconesSlots = new Image[2];

    [SerializeField] private DadosSlotConstrucaoUnidade[] definicoesSlots = new DadosSlotConstrucaoUnidade[2];

    [Tooltip("Textos de custo alinhados aos slots (opcional).")]
    [SerializeField] private TextMeshProUGUI[] textosCustoSlots = new TextMeshProUGUI[2];

    [Header("Feedback slot selecionado")]
    [SerializeField] private Color corBotaoNormal = Color.white;
    [SerializeField] private Color corBotaoSelecionado = new Color(0.7f, 0.85f, 0.95f, 1f);

    private int _slotVisualDestacado = -1;
    private UnityAction[] _listenersSlots;

    private void Awake()
    {
        if (servicoColocacao == null)
            servicoColocacao = FindFirstObjectByType<ServicoColocacaoUnidade>(FindObjectsInactive.Include);

        ResolverReferenciasIcones();

        _listenersSlots = new UnityAction[botoesSlots.Length];
        for (int i = 0; i < botoesSlots.Length; i++)
        {
            int idx = i;
            _listenersSlots[i] = () => AoClicarSlot(idx);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < botoesSlots.Length; i++)
        {
            if (botoesSlots[i] != null && _listenersSlots != null && i < _listenersSlots.Length && _listenersSlots[i] != null)
                botoesSlots[i].onClick.AddListener(_listenersSlots[i]);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < botoesSlots.Length; i++)
        {
            if (botoesSlots[i] != null && _listenersSlots != null && i < _listenersSlots.Length && _listenersSlots[i] != null)
                botoesSlots[i].onClick.RemoveListener(_listenersSlots[i]);
        }
    }

    private void Start()
    {
        AplicarIconesNosSlots();
        SincronizarTextosCusto();
    }

    private void Update()
    {
        AtualizarDestaqueSlot();
    }

    private void ResolverReferenciasIcones()
    {
        for (int i = 0; i < botoesSlots.Length; i++)
        {
            if (i >= iconesSlots.Length)
                break;

            if (iconesSlots[i] != null || botoesSlots[i] == null)
                continue;

            Transform t = botoesSlots[i].transform.Find("Icone");
            if (t != null)
                iconesSlots[i] = t.GetComponent<Image>();

            if (iconesSlots[i] == null)
            {
                Image[] imgs = botoesSlots[i].GetComponentsInChildren<Image>(true);
                foreach (Image img in imgs)
                {
                    if (img.gameObject != botoesSlots[i].gameObject)
                    {
                        iconesSlots[i] = img;
                        break;
                    }
                }
            }
        }
    }

    private void AplicarIconesNosSlots()
    {
        int n = Mathf.Min(definicoesSlots.Length, iconesSlots.Length);
        for (int i = 0; i < n; i++)
        {
            if (iconesSlots[i] == null || definicoesSlots[i] == null)
                continue;

            GameObject prefabIcone = definicoesSlots[i].prefabUnidade != null
                ? definicoesSlots[i].prefabUnidade
                : definicoesSlots[i].prefabUnidadeFeminina;
            if (prefabIcone == null)
                continue;

            Sprite sp = UtilSpritesPrefab.ObterSpritePrincipal(prefabIcone);
            if (sp != null)
            {
                iconesSlots[i].sprite = sp;
                iconesSlots[i].preserveAspect = true;
                iconesSlots[i].enabled = true;
            }
        }
    }

    private void SincronizarTextosCusto()
    {
        if (textosCustoSlots == null)
            return;

        int n = Mathf.Min(textosCustoSlots.Length, definicoesSlots.Length);
        for (int i = 0; i < n; i++)
        {
            if (textosCustoSlots[i] == null || definicoesSlots[i] == null)
                continue;
            textosCustoSlots[i].text = definicoesSlots[i].custoOuro.ToString();
        }
    }

    private void AoClicarSlot(int indice)
    {
        if (servicoColocacao == null)
            return;

        if (indice < 0 || indice >= definicoesSlots.Length)
            return;

        DadosSlotConstrucaoUnidade dados = definicoesSlots[indice];
        if (dados == null || (dados.prefabUnidade == null && dados.prefabUnidadeFeminina == null))
            return;

        servicoColocacao.IniciarColocacao(dados, indice);
    }

    private void AtualizarDestaqueSlot()
    {
        int atual = -1;
        if (servicoColocacao != null && servicoColocacao.ModoColocacaoAtivo)
            atual = servicoColocacao.IndiceSlotSelecionado;

        if (atual == _slotVisualDestacado)
            return;

        if (_slotVisualDestacado >= 0 && _slotVisualDestacado < botoesSlots.Length && botoesSlots[_slotVisualDestacado] != null)
            AplicarCorBotao(botoesSlots[_slotVisualDestacado], corBotaoNormal);

        _slotVisualDestacado = atual;

        if (_slotVisualDestacado >= 0 && _slotVisualDestacado < botoesSlots.Length && botoesSlots[_slotVisualDestacado] != null)
            AplicarCorBotao(botoesSlots[_slotVisualDestacado], corBotaoSelecionado);
    }

    private static void AplicarCorBotao(Button botao, Color corFundo)
    {
        ColorBlock cb = botao.colors;
        cb.normalColor = corFundo;
        cb.selectedColor = corFundo;
        cb.highlightedColor = corFundo;
        botao.colors = cb;
    }
}
