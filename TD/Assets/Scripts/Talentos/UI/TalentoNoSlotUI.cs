using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TalentoNoSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    private Image imagemFundo;
    private Image imagemIcone;
    private TextMeshProUGUI textoRodape;
    private Button botao;

    private PainelArvoreTalentos painel;
    private TalentoTooltipUI tooltip;
    private TalentNodeEntry no;

    public void DefinirReferencias(Image fundo, Image icone, TextMeshProUGUI rodape, Button btn)
    {
        imagemFundo = fundo;
        imagemIcone = icone;
        textoRodape = rodape;
        botao = btn;
    }

    public void Configurar(
        TalentNodeEntry entrada,
        bool jaDesbloqueado,
        PainelArvoreTalentos painelArvore,
        TalentoTooltipUI tooltipRef)
    {
        no = entrada;
        painel = painelArvore;
        tooltip = tooltipRef;

        if (imagemIcone != null)
        {
            if (entrada.icone != null)
            {
                imagemIcone.sprite = entrada.icone;
                imagemIcone.enabled = true;
                imagemIcone.color = jaDesbloqueado ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else
            {
                imagemIcone.enabled = false;
            }
        }

        Transform nomeTf = transform.Find("Interno/Nome");
        if (nomeTf != null)
        {
            var tmpNome = nomeTf.GetComponent<TextMeshProUGUI>();
            if (tmpNome != null)
            {
                tmpNome.text = entrada.ObterNomeCurto();
                tmpNome.enabled = entrada.icone == null;
            }
        }

        if (imagemFundo != null)
        {
            imagemFundo.color = jaDesbloqueado
                ? new Color(0.22f, 0.42f, 0.22f, 1f)
                : new Color(0.16f, 0.16f, 0.2f, 1f);
        }

        if (textoRodape != null)
        {
            if (TMP_Settings.defaultFontAsset != null)
                textoRodape.font = TMP_Settings.defaultFontAsset;
            textoRodape.text = jaDesbloqueado ? "1/1" : "0/1";
        }

        if (botao != null)
        {
            botao.interactable = true;
            botao.onClick.RemoveAllListeners();
            string id = entrada.id;
            botao.onClick.AddListener(() =>
            {
                if (painel != null)
                    painel.TentarAlternarNoPorId(id);
            });
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null || no == null)
            return;
        tooltip.Mostrar(no.ObterTextoTooltipCompleto(), Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.Ocultar();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (tooltip == null || no == null)
            return;
        tooltip.Mostrar(no.ObterTextoTooltipCompleto(), Input.mousePosition);
    }

    public static TalentoNoSlotUI CriarSlot(
        Transform parent,
        TalentNodeEntry entrada,
        bool jaDesbloqueado,
        PainelArvoreTalentos painelArvore,
        TalentoTooltipUI tooltipRef,
        Vector2 tamanhoCelula)
    {
        if (tamanhoCelula.x < 1f || tamanhoCelula.y < 1f)
            tamanhoCelula = new Vector2(72f, 72f);

        GameObject raiz = new GameObject("Slot_" + entrada.id, typeof(RectTransform));
        raiz.transform.SetParent(parent, false);
        RectTransform rt = raiz.GetComponent<RectTransform>();
        rt.sizeDelta = tamanhoCelula;

        LayoutElement le = raiz.AddComponent<LayoutElement>();
        le.preferredWidth = tamanhoCelula.x;
        le.preferredHeight = tamanhoCelula.y;
        le.minWidth = tamanhoCelula.x;
        le.minHeight = tamanhoCelula.y;

        Image borda = raiz.AddComponent<Image>();
        borda.color = new Color(0.75f, 0.62f, 0.2f, 1f);
        borda.raycastTarget = true;

        GameObject interno = new GameObject("Interno", typeof(RectTransform));
        interno.transform.SetParent(raiz.transform, false);
        RectTransform rtIn = interno.GetComponent<RectTransform>();
        rtIn.anchorMin = Vector2.zero;
        rtIn.anchorMax = Vector2.one;
        rtIn.offsetMin = new Vector2(3f, 3f);
        rtIn.offsetMax = new Vector2(-3f, -3f);

        Image fundoInterno = interno.AddComponent<Image>();
        fundoInterno.color = jaDesbloqueado ? new Color(0.22f, 0.42f, 0.22f, 1f) : new Color(0.16f, 0.16f, 0.2f, 1f);
        fundoInterno.raycastTarget = false;

        GameObject iconeGo = new GameObject("Icone", typeof(RectTransform));
        iconeGo.transform.SetParent(interno.transform, false);
        RectTransform rtIc = iconeGo.GetComponent<RectTransform>();
        rtIc.anchorMin = new Vector2(0.1f, 0.2f);
        rtIc.anchorMax = new Vector2(0.9f, 0.95f);
        rtIc.offsetMin = Vector2.zero;
        rtIc.offsetMax = Vector2.zero;
        Image imgIcone = iconeGo.AddComponent<Image>();
        imgIcone.raycastTarget = false;

        GameObject nomeGo = new GameObject("Nome", typeof(RectTransform));
        nomeGo.transform.SetParent(interno.transform, false);
        RectTransform rtNm = nomeGo.GetComponent<RectTransform>();
        rtNm.anchorMin = new Vector2(0.05f, 0.2f);
        rtNm.anchorMax = new Vector2(0.95f, 0.88f);
        rtNm.offsetMin = Vector2.zero;
        rtNm.offsetMax = Vector2.zero;
        TextMeshProUGUI tmpNome = nomeGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            tmpNome.font = TMP_Settings.defaultFontAsset;
        tmpNome.text = entrada.ObterNomeCurto();
        tmpNome.fontSize = Mathf.Clamp(tamanhoCelula.y * 0.13f, 12f, 18f);
        tmpNome.textWrappingMode = TextWrappingModes.Normal;
        tmpNome.alignment = TextAlignmentOptions.Midline;
        tmpNome.color = Color.white;
        tmpNome.raycastTarget = false;

        GameObject rodapeGo = new GameObject("Rodape", typeof(RectTransform));
        rodapeGo.transform.SetParent(interno.transform, false);
        RectTransform rtRd = rodapeGo.GetComponent<RectTransform>();
        rtRd.anchorMin = new Vector2(0f, 0f);
        rtRd.anchorMax = new Vector2(1f, 0.22f);
        rtRd.offsetMin = new Vector2(2f, 1f);
        rtRd.offsetMax = new Vector2(-2f, -1f);
        TextMeshProUGUI tmpRodape = rodapeGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            tmpRodape.font = TMP_Settings.defaultFontAsset;
        tmpRodape.fontSize = Mathf.Clamp(tamanhoCelula.y * 0.12f, 11f, 16f);
        tmpRodape.alignment = TextAlignmentOptions.BottomRight;
        tmpRodape.color = new Color(1f, 0.92f, 0.4f, 1f);

        Button btn = raiz.AddComponent<Button>();
        btn.targetGraphic = borda;

        TalentoNoSlotUI slot = raiz.AddComponent<TalentoNoSlotUI>();
        slot.DefinirReferencias(fundoInterno, imgIcone, tmpRodape, btn);
        slot.Configurar(entrada, jaDesbloqueado, painelArvore, tooltipRef);

        return slot;
    }
}
