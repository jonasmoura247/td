using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// HUD inferior com slots de torre; delega colocação ao ServicoColocacaoTorre.
/// </summary>
public class ControladorHUDConstrucaoTorres : MonoBehaviour
{
    [SerializeField] private ServicoColocacaoTorre servicoColocacao;

    [SerializeField] private Button[] botoesSlots = new Button[5];

    [Tooltip("Ícone de cada slot (filho do botão). Se vazio, tenta encontrar Image chamada Icone ou primeiro Image filho.")]
    [SerializeField] private Image[] iconesSlots = new Image[5];

    [SerializeField] private DadosSlotConstrucaoTorre[] definicoesSlots = new DadosSlotConstrucaoTorre[5];

    [Header("Feedback slot selecionado")]
    [SerializeField] private Color corBotaoNormal = Color.white;
    [SerializeField] private Color corBotaoSelecionado = new Color(0.7f, 0.95f, 0.75f, 1f);

    private int _slotVisualDestacado = -1;
    private UnityAction[] _listenersSlots;

    private void Awake()
    {
        if (servicoColocacao == null)
            servicoColocacao = FindFirstObjectByType<ServicoColocacaoTorre>(FindObjectsInactive.Include);

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
            if (iconesSlots[i] == null || definicoesSlots[i] == null || definicoesSlots[i].prefabTorre == null)
                continue;

            Sprite sp = UtilSpritesPrefab.ObterSpritePrincipal(definicoesSlots[i].prefabTorre);
            if (sp != null)
            {
                iconesSlots[i].sprite = sp;
                iconesSlots[i].preserveAspect = true;
                iconesSlots[i].enabled = true;
            }
        }
    }

    private void AoClicarSlot(int indice)
    {
        if (servicoColocacao == null)
            return;

        if (indice < 0 || indice >= definicoesSlots.Length)
            return;

        DadosSlotConstrucaoTorre dados = definicoesSlots[indice];
        if (dados == null || dados.prefabTorre == null || dados.dados == null)
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
