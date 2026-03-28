using UnityEngine;

public class ProgressaoRacaNotificacaoLevelUpLobby : MonoBehaviour
{
    [SerializeField] private string chavePrefsPrefixo = "TD_NivelRacaVisto_";
    [SerializeField] private string formatoMensagem = "Raca {0} subiu para o Level {1}!";

    private void OnEnable()
    {
        AvaliarENotificar();
    }

    public void AvaliarENotificar()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || string.IsNullOrEmpty(PerfilJogadorAtivo.Uuid))
            return;

        ProgressaoRacaSnapshot snap = ProgressaoRacaAtivaLeitura.ObterSnapshotRacaAtiva();
        if (!snap.TemDadosValidos)
            return;

        string chave = chavePrefsPrefixo + PerfilJogadorAtivo.Uuid;
        int armazenado = PlayerPrefs.GetInt(chave, -1);

        if (armazenado < 0)
        {
            PlayerPrefs.SetInt(chave, snap.Nivel);
            PlayerPrefs.Save();
            return;
        }

        if (snap.Nivel > armazenado && NotificacaoToastUI.Instance != null)
        {
            string msg = string.Format(formatoMensagem, snap.NomeExibicao, snap.Nivel);
            NotificacaoToastUI.Instance.Mostrar(msg);
        }

        PlayerPrefs.SetInt(chave, snap.Nivel);
        PlayerPrefs.Save();
    }
}
