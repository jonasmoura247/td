using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla a exibição do painel de unidades. BotaoUnidades alterna a visibilidade;
/// clique fora (no overlay) fecha o painel.
/// </summary>
public class PainelUnidadesController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Button botaoUnidades;
    [SerializeField] private GameObject painelUnidades;
    [SerializeField] private Button overlayFundo;

    private void OnEnable()
    {
        if (botaoUnidades != null)
            botaoUnidades.onClick.AddListener(TogglePainel);
        if (overlayFundo != null)
            overlayFundo.onClick.AddListener(FecharPainel);
    }

    private void OnDisable()
    {
        if (botaoUnidades != null)
            botaoUnidades.onClick.RemoveListener(TogglePainel);
        if (overlayFundo != null)
            overlayFundo.onClick.RemoveListener(FecharPainel);
    }

    private void TogglePainel()
    {
        if (painelUnidades == null) return;
        painelUnidades.SetActive(!painelUnidades.activeSelf);
    }

    private void FecharPainel()
    {
        if (painelUnidades != null)
            painelUnidades.SetActive(false);
    }
}
