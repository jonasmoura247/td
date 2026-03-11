using UnityEngine;
using TMPro;

/// <summary>
/// Exibe o ouro atual do jogador na HUD. Atualiza a partir do OuroManager.
/// </summary>
public class TextoOuro : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private TextMeshProUGUI textoOuro;

    [Header("Formato")]
    [Tooltip("Ex: '{0}' para apenas o número. {0} = valor")]
    [SerializeField] private string formatoExibicao = "{0}";

    private void OnEnable()
    {
        if (OuroManager.Instance != null)
        {
            OuroManager.Instance.OnOuroAlterado += AtualizarTexto;
            AtualizarTexto(OuroManager.Instance.GetOuro());
        }
    }

    private void OnDisable()
    {
        if (OuroManager.Instance != null)
            OuroManager.Instance.OnOuroAlterado -= AtualizarTexto;
    }

    private void Start()
    {
        if (textoOuro == null) return;
        if (OuroManager.Instance != null)
            AtualizarTexto(OuroManager.Instance.GetOuro());
        else
            textoOuro.text = string.Format(formatoExibicao, 0);
    }

    private void AtualizarTexto(int valor)
    {
        if (textoOuro != null)
            textoOuro.text = string.Format(formatoExibicao, valor);
    }
}
