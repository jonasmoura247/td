using UnityEngine;
using UnityEngine.UI;

public class LobbyArvoreTalentosController : MonoBehaviour
{
    [SerializeField] private GameObject painelRaiz;
    [SerializeField] private PainelArvoreTalentos painelArvoreTalentos;
    [SerializeField] private Button botaoAbrir;
    [SerializeField] private Button botaoFechar;

    private void Awake()
    {
        SaveSlotManager.GarantirExistencia();
        LootManager.GarantirExistencia();

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
            Debug.LogWarning("[Talentos] Nenhum perfil ativo. Carregue um slot de save antes.");
            return;
        }

        if (painelRaiz != null)
            painelRaiz.SetActive(true);

        if (painelArvoreTalentos != null)
            painelArvoreTalentos.AoAbrirPainel();
    }

    public void FecharPainel()
    {
        if (painelRaiz != null)
            painelRaiz.SetActive(false);
    }
}
