using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wrapper simples para botões de opção da tenda.
/// </summary>
public class TendaOpcaoBotaoController : MonoBehaviour
{
    [SerializeField] private Button botao;
    [SerializeField] private TMP_Text textoTmp;
    [SerializeField] private Text textoLegacy;

    public void Configurar(string texto, System.Action onClick, bool interativo)
    {
        if (textoTmp != null)
            textoTmp.text = texto;
        if (textoLegacy != null)
            textoLegacy.text = texto;

        if (botao == null)
            botao = GetComponent<Button>();

        if (botao != null)
        {
            botao.interactable = interativo;
            botao.onClick.RemoveAllListeners();
            if (onClick != null)
                botao.onClick.AddListener(() => onClick.Invoke());
        }
    }
}
