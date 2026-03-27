using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Carrega uma cena pelo nome. Use no OnClick do botão.
/// </summary>
public class CarregadorCena : MonoBehaviour
{
    [SerializeField] private string nomeCena = "Mapa_1";

    public void Carregar()
    {
        if (string.IsNullOrEmpty(nomeCena))
        {
            Debug.LogError("[CarregadorCena] Nome da cena nao definido!");
            return;
        }

        Time.timeScale = 1f; // Garante que o tempo nao esta pausado
        SceneManager.LoadScene(nomeCena);
    }

    /// <summary>
    /// Versao para chamar passando o nome da cena via codigo.
    /// </summary>
    public void Carregar(string cena)
    {
        if (string.IsNullOrEmpty(cena))
        {
            Debug.LogError("[CarregadorCena] Nome da cena invalido!");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(cena);
    }

    public void CarregarLobbyDoPerfilAtivo()
    {
        SaveSlotManager.GarantirExistencia();

        if (SaveSlotManager.Instance == null)
        {
            Debug.LogError("[CarregadorCena] SaveSlotManager nao encontrado.");
            return;
        }

        TipoRaca raca = PerfilJogadorAtivo.TemPerfilAtivo ? PerfilJogadorAtivo.Raca : TipoRaca.Goblin;
        string cena = SaveSlotManager.Instance.ObterCenaLobby(raca);
        if (string.IsNullOrEmpty(cena))
        {
            Debug.LogError("[CarregadorCena] Nome da cena do lobby vazio.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(cena);
    }
}
