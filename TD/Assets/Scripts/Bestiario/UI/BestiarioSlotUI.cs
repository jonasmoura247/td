using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BestiarioSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    private Image imagemIcone;
    private TextMeshProUGUI textoRodape;
    private TextMeshProUGUI textoNome;
    private Button botao;
    private PainelBestiario painel;
    private TalentoTooltipUI tooltip;
    private BestiarioCriaturaDef def;
    private int mortes;

    private void DefinirReferencias(Image icone, TextMeshProUGUI rodape, TextMeshProUGUI nome, Button btn)
    {
        imagemIcone = icone;
        textoRodape = rodape;
        textoNome = nome;
        botao = btn;
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
        {
            if (mortesAtuais <= 0)
                textoRodape.text = "???";
            else
                textoRodape.text = mortesAtuais.ToString();
        }

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

    public static BestiarioSlotUI CriarSlot(
        Transform parent,
        BestiarioCriaturaDef entrada,
        int mortesAtuais,
        PainelBestiario painelRef,
        TalentoTooltipUI tooltipRef,
        Vector2 tamanhoCelula)
    {
        if (tamanhoCelula.x < 1f || tamanhoCelula.y < 1f)
            tamanhoCelula = new Vector2(100f, 100f);

        GameObject raiz = new GameObject("Bestiario_" + (entrada != null ? entrada.id : "null"), typeof(RectTransform));
        raiz.transform.SetParent(parent, false);
        RectTransform rt = raiz.GetComponent<RectTransform>();
        rt.sizeDelta = tamanhoCelula;

        LayoutElement le = raiz.AddComponent<LayoutElement>();
        le.preferredWidth = tamanhoCelula.x;
        le.preferredHeight = tamanhoCelula.y;
        le.minWidth = tamanhoCelula.x;
        le.minHeight = tamanhoCelula.y;

        Image borda = raiz.AddComponent<Image>();
        borda.color = new Color(0.35f, 0.28f, 0.15f, 1f);
        borda.raycastTarget = true;

        GameObject interno = new GameObject("Interno", typeof(RectTransform));
        interno.transform.SetParent(raiz.transform, false);
        RectTransform rtIn = interno.GetComponent<RectTransform>();
        rtIn.anchorMin = Vector2.zero;
        rtIn.anchorMax = Vector2.one;
        rtIn.offsetMin = new Vector2(3f, 3f);
        rtIn.offsetMax = new Vector2(-3f, -3f);

        Image fundoInterno = interno.AddComponent<Image>();
        fundoInterno.color = new Color(0.12f, 0.12f, 0.14f, 1f);
        fundoInterno.raycastTarget = false;

        GameObject iconeGo = new GameObject("Icone", typeof(RectTransform));
        iconeGo.transform.SetParent(interno.transform, false);
        RectTransform rtIc = iconeGo.GetComponent<RectTransform>();
        rtIc.anchorMin = new Vector2(0.1f, 0.28f);
        rtIc.anchorMax = new Vector2(0.9f, 0.95f);
        rtIc.offsetMin = Vector2.zero;
        rtIc.offsetMax = Vector2.zero;
        Image imgIcone = iconeGo.AddComponent<Image>();
        imgIcone.raycastTarget = false;

        GameObject nomeGo = new GameObject("Nome", typeof(RectTransform));
        nomeGo.transform.SetParent(interno.transform, false);
        RectTransform rtNm = nomeGo.GetComponent<RectTransform>();
        rtNm.anchorMin = new Vector2(0.05f, 0.72f);
        rtNm.anchorMax = new Vector2(0.95f, 0.98f);
        rtNm.offsetMin = Vector2.zero;
        rtNm.offsetMax = Vector2.zero;
        TextMeshProUGUI tmpNome = nomeGo.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
            tmpNome.font = TMP_Settings.defaultFontAsset;
        tmpNome.fontSize = Mathf.Clamp(tamanhoCelula.y * 0.11f, 10f, 16f);
        tmpNome.alignment = TextAlignmentOptions.Top;
        tmpNome.color = Color.white;
        tmpNome.raycastTarget = false;

        GameObject rodapeGo = new GameObject("Rodape", typeof(RectTransform));
        rodapeGo.transform.SetParent(interno.transform, false);
        RectTransform rtRd = rodapeGo.GetComponent<RectTransform>();
        rtRd.anchorMin = new Vector2(0f, 0f);
        rtRd.anchorMax = new Vector2(1f, 0.28f);
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

        BestiarioSlotUI slot = raiz.AddComponent<BestiarioSlotUI>();
        slot.DefinirReferencias(imgIcone, tmpRodape, tmpNome, btn);
        slot.Configurar(entrada, mortesAtuais, painelRef, tooltipRef);

        return slot;
    }
}
