using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnidadeUI : MonoBehaviour
{
    public UnidadeController unidade;

    [Header("Refer�ncias UI")]
    public Image barraVidaFill;
    public TextMeshProUGUI textoInfo;

    private void Update()
    {
        if (unidade == null)
            return;

        AtualizarVida();
        AtualizarDano();
    }

    private void AtualizarVida()
    {
        float porcentagem = (float)unidade.GetVidaAtual() / unidade.vidaMaxima;
        barraVidaFill.fillAmount = porcentagem;
    }

    private void AtualizarDano()
    {
        int dano = unidade.GetDanoAtaque();
        textoInfo.text = $"HP: {unidade.GetVidaAtual()}  DMG: {dano}";
    }
}