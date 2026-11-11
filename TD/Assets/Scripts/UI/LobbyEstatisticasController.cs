using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Conecta o BotaoEstatisticas do lobby ao PainelEstatisticas.
/// Segue o mesmo padrao de LobbyBestiarioController.
/// </summary>
public class LobbyEstatisticasController : MonoBehaviour
{
    [SerializeField] private GameObject painelRaiz;
    [SerializeField] private PainelEstatisticas painelEstatisticas;
    [SerializeField] private Button botaoAbrir;
    [SerializeField] private Button botaoFechar;

    private void Awake()
    {
        SaveSlotManager.GarantirExistencia();

        if (EstatisticasManager.Instance == null)
        {
            var go = new GameObject("EstatisticasManager");
            go.AddComponent<EstatisticasManager>();
        }

        if (botaoAbrir != null)
            botaoAbrir.onClick.AddListener(AbrirPainel);
        if (botaoFechar != null)
            botaoFechar.onClick.AddListener(FecharPainel);

        if (painelRaiz != null)
            painelRaiz.SetActive(false);
    }

    public void AbrirPainel()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo)
        {
            Debug.LogWarning("[Estatisticas] Nenhum perfil ativo. Carregue um slot de save antes.");
            return;
        }

        if (painelRaiz != null)
            painelRaiz.SetActive(true);

        if (painelEstatisticas != null)
            painelEstatisticas.AoAbrirPainel();
    }

    public void FecharPainel()
    {
        if (painelRaiz != null)
            painelRaiz.SetActive(false);
    }
}
