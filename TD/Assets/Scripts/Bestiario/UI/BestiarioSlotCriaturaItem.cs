using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BestiarioSlotCriaturaItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private Image imagemIcone;
    [SerializeField] private TextMeshProUGUI textoRodape;
    [SerializeField] private TextMeshProUGUI textoNome;
    [SerializeField] private Button botao;
    [Tooltip("Image da moldura externa do cartão (raiz do slot). A cor muda com o estágio.")]
    [SerializeField] private Image imagemMolduraRaiz;
    [Tooltip("Opcional: mostra os marcos de mortes (ex.: 160 | 400 | 800).")]
    [SerializeField] private TextMeshProUGUI textoMarcosEstagio;

    [Header("Cores da moldura por progresso")]
    [SerializeField] private Color corMolduraNaoDescoberto = new Color(0.16f, 0.16f, 0.18f, 1f);
    [SerializeField] private Color corMolduraEtapa1 = new Color(0.42f, 0.3f, 0.16f, 1f);
    [SerializeField] private Color corMolduraEtapa2 = new Color(0.72f, 0.72f, 0.76f, 1f);
    [SerializeField] private Color corMolduraEtapa3 = new Color(0.83f, 0.65f, 0.2f, 1f);

    private PainelBestiario painel;
    private TalentoTooltipUI tooltip;
    private BestiarioCriaturaDef def;
    private int mortes;

    private void Awake()
    {
        if (textoMarcosEstagio == null)
        {
            var tr = transform.Find("Interno/MarcosEstagios");
            if (tr != null)
                textoMarcosEstagio = tr.GetComponent<TextMeshProUGUI>();
        }
    }

    public void Configurar(
        BestiarioCriaturaDef criaturaDef,
        int mortesAtuais,
        PainelBestiario painelRef,
        TalentoTooltipUI tooltipRef)
    {
        def = criaturaDef;
        mortes = mortesAtuais;
        painel = painelRef;
        tooltip = tooltipRef;

        if (textoNome != null)
            textoNome.text = criaturaDef != null ? criaturaDef.nomeExibicao : "";

        if (imagemIcone != null && criaturaDef != null)
        {
            bool descoberto = mortesAtuais > 0;
            Sprite sp = descoberto
                ? (criaturaDef.icone != null ? criaturaDef.icone : criaturaDef.iconeSilhueta)
                : (criaturaDef.iconeSilhueta != null ? criaturaDef.iconeSilhueta : criaturaDef.icone);
            imagemIcone.sprite = sp;
            imagemIcone.enabled = sp != null;
            imagemIcone.color = descoberto ? new Color(0.55f, 0.55f, 0.58f, 1f) : new Color(0.2f, 0.2f, 0.22f, 1f);
        }

        if (textoRodape != null)
            textoRodape.text = mortesAtuais <= 0 ? "???" : mortesAtuais.ToString();

        AplicarMolduraEMarcos(criaturaDef, mortesAtuais);

        if (botao != null)
        {
            botao.onClick.RemoveAllListeners();
            string id = criaturaDef != null ? criaturaDef.id : "";
            botao.onClick.AddListener(() =>
            {
                if (painel != null && !string.IsNullOrEmpty(id))
                    painel.SelecionarCriatura(id);
            });
        }
    }

    private void AplicarMolduraEMarcos(BestiarioCriaturaDef criaturaDef, int mortesAtuais)
    {
        if (imagemMolduraRaiz != null && criaturaDef != null)
        {
            int estagiosCompletos = BestiarioRegras.ObterEstagiosCompletos(criaturaDef, mortesAtuais);
            Color cor;
            if (mortesAtuais <= 0)
                cor = corMolduraNaoDescoberto;
            else if (estagiosCompletos <= 0)
                cor = corMolduraEtapa1;
            else if (estagiosCompletos == 1)
                cor = corMolduraEtapa2;
            else
                cor = corMolduraEtapa3;
            imagemMolduraRaiz.color = cor;
        }

        if (textoMarcosEstagio != null)
        {
            if (criaturaDef?.marcosMortes == null || criaturaDef.marcosMortes.Length == 0)
            {
                textoMarcosEstagio.gameObject.SetActive(false);
            }
            else
            {
                textoMarcosEstagio.gameObject.SetActive(true);
                textoMarcosEstagio.text = FormatarMarcosMortes(criaturaDef.marcosMortes);
            }
        }
    }

    private static string FormatarMarcosMortes(int[] marcos)
    {
        if (marcos == null || marcos.Length == 0)
            return "";
        return string.Join(" | ", Array.ConvertAll(marcos, m => m.ToString()));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null || def == null)
            return;
        string txt = string.IsNullOrEmpty(def.descricaoTooltip)
            ? def.nomeExibicao
            : def.descricaoTooltip;
        if (mortes <= 0)
            txt = "Ainda nao encontrado.\n\n" + txt;
        tooltip.Mostrar(txt, Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Ocultar();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (tooltip == null || def == null)
            return;
        string txt = string.IsNullOrEmpty(def.descricaoTooltip)
            ? def.nomeExibicao
            : def.descricaoTooltip;
        if (mortes <= 0)
            txt = "Ainda nao encontrado.\n\n" + txt;
        tooltip.Mostrar(txt, Input.mousePosition);
    }
}
