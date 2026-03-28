using UnityEngine;
using UnityEngine.UI;

public class LobbyBestiarioController : MonoBehaviour
{
    [SerializeField] private GameObject painelRaiz;
    [SerializeField] private PainelBestiario painelBestiario;
    [SerializeField] private Button botaoAbrir;
    [SerializeField] private Button botaoFechar;

    private void Awake()
    {
        SaveSlotManager.GarantirExistencia();

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
            Debug.LogWarning("[Bestiario] Nenhum perfil ativo. Carregue um slot de save antes.");
            return;
        }

        if (painelRaiz != null)
            painelRaiz.SetActive(true);

        if (painelBestiario != null)
            painelBestiario.AoAbrirPainel();
    }

    public void FecharPainel()
    {
        if (painelRaiz != null)
            painelRaiz.SetActive(false);
    }
}
