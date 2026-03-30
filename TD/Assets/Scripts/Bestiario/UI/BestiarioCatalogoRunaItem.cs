using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BestiarioCatalogoRunaItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private CanvasGroup canvasGroupRaiz;
    [SerializeField] private Image imagemIcone;
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoRequisitos;
    [SerializeField] private TextMeshProUGUI textoEfeito;
    [SerializeField] private TextMeshProUGUI textoDescricaoCatalogo;
    [SerializeField] private Button botaoRaiz;

    private TalentoTooltipUI tooltip;
    private string tooltipTexto;

    public void Configurar(
        BestiarioRunaDef runa,
        BestiarioCriaturaDef criaturaAlvo,
        bool habilitada,
        float alphaBloqueado,
        string motivoBloqueio,
        Action aoClicar,
        TalentoTooltipUI tooltipRef,
        Sprite fallbackSprite)
    {
        tooltip = tooltipRef;
        tooltipTexto = runa != null
            ? (string.IsNullOrEmpty(runa.descricaoTooltip) ? runa.nomeExibicao : runa.descricaoTooltip)
            : "";

        if (canvasGroupRaiz != null)
        {
            canvasGroupRaiz.alpha = habilitada ? 1f : alphaBloqueado;
            canvasGroupRaiz.interactable = true;
            canvasGroupRaiz.blocksRaycasts = true;
        }

        if (imagemIcone != null && runa != null)
        {
            Sprite sp = runa.icone != null ? runa.icone : runa.iconeSilhueta;
            if (sp == null)
                sp = fallbackSprite;
            imagemIcone.sprite = sp;
            imagemIcone.enabled = sp != null;
        }

        if (textoTitulo != null && runa != null)
            textoTitulo.text = runa.nomeExibicao;

        if (textoRequisitos != null && runa != null)
        {
            string baseReq = BestiarioTextoRunaUtil.TextoRequisitos(runa, criaturaAlvo);
            if (!habilitada && !string.IsNullOrEmpty(motivoBloqueio))
                baseReq += "\n" + motivoBloqueio;
            textoRequisitos.text = baseReq;
        }

        if (textoEfeito != null && runa != null)
            textoEfeito.text = BestiarioTextoRunaUtil.ResumoEfeito(runa);

        if (textoDescricaoCatalogo != null)
        {
            bool temDesc = runa != null && !string.IsNullOrEmpty(runa.descricaoCatalogo);
            textoDescricaoCatalogo.gameObject.SetActive(temDesc);
            if (temDesc)
                textoDescricaoCatalogo.text = runa.descricaoCatalogo;
        }

        if (botaoRaiz != null)
        {
            botaoRaiz.onClick.RemoveAllListeners();
            botaoRaiz.onClick.AddListener(() => aoClicar?.Invoke());
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip == null || string.IsNullOrEmpty(tooltipTexto))
            return;
        tooltip.Mostrar(tooltipTexto, Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.Ocultar();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (tooltip == null || string.IsNullOrEmpty(tooltipTexto))
            return;
        tooltip.Mostrar(tooltipTexto, Input.mousePosition);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Só no Editor: preenche textos/alpha para alinhar o layout do catálogo sem dar Play.
    /// Não chamar em builds de jogador.
    /// </summary>
    public void AplicarDadosPreviewEditor(string titulo, string requisitos, string efeito, float alphaGrupo, string descricaoCatalogo = null)
    {
        if (textoTitulo != null)
            textoTitulo.text = titulo;
        if (textoRequisitos != null)
            textoRequisitos.text = requisitos;
        if (textoEfeito != null)
            textoEfeito.text = efeito;
        if (canvasGroupRaiz != null)
            canvasGroupRaiz.alpha = alphaGrupo;
        if (textoDescricaoCatalogo != null)
        {
            bool tem = !string.IsNullOrEmpty(descricaoCatalogo);
            textoDescricaoCatalogo.gameObject.SetActive(tem);
            if (tem)
                textoDescricaoCatalogo.text = descricaoCatalogo;
        }
    }
#endif
}
