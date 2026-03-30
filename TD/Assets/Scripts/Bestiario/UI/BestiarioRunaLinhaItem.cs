using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BestiarioRunaLinhaItem : MonoBehaviour
{
    [SerializeField] private Image imagemIcone;
    [SerializeField] private TextMeshProUGUI textoLinha;
    [SerializeField] private Button botaoAcao;
    [SerializeField] private TextMeshProUGUI textoBotao;
    [SerializeField] private Image imagemBotao;

    public void Configurar(
        BestiarioRunaDef runa,
        string textoLinhaCompleto,
        string textoBotaoAcao,
        bool botaoInteractavel,
        Color corBotao,
        Action<string> aoClicar,
        Sprite fallbackSprite)
    {
        if (runa == null)
            return;

        if (imagemIcone != null)
        {
            Sprite sp = runa.icone != null ? runa.icone : runa.iconeSilhueta;
            if (sp == null)
                sp = fallbackSprite;
            imagemIcone.sprite = sp;
            imagemIcone.enabled = sp != null;
            imagemIcone.color = botaoInteractavel ? Color.white : new Color(0.45f, 0.45f, 0.48f, 1f);
        }

        if (textoLinha != null)
            textoLinha.text = textoLinhaCompleto ?? "";

        if (imagemBotao != null)
            imagemBotao.color = corBotao;

        if (textoBotao != null)
            textoBotao.text = textoBotaoAcao ?? "";

        if (botaoAcao != null)
        {
            botaoAcao.onClick.RemoveAllListeners();
            botaoAcao.interactable = botaoInteractavel && aoClicar != null;
            if (aoClicar != null)
            {
                string rid = runa.id;
                botaoAcao.onClick.AddListener(() => aoClicar(rid));
            }
        }
    }
}
