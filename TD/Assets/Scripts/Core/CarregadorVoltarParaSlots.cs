using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Botão "Sair" dos lobbies. Volta para a tela de seleção de slots
/// e limpa o perfil ativo (jogador não está mais "em partida").
/// </summary>
public class CarregadorVoltarParaSlots : MonoBehaviour
{
    [SerializeField] private string cenaSelecaoSlots = "SelecaoSlots";

    public void VoltarParaSelecaoSlots()
    {
        PerfilJogadorAtivo.LimparPerfil();

        Time.timeScale = 1f;
        SceneManager.LoadScene(cenaSelecaoSlots);
    }
}
