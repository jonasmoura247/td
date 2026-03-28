using UnityEngine;

public class UnidadeModificadores : MonoBehaviour
{
    private float danoFlat;
    private float danoPercentual;
    private float vidaMaxFlat;
    private float vidaMaxPercentual;

    public void Limpar()
    {
        danoFlat = 0f;
        danoPercentual = 0f;
        vidaMaxFlat = 0f;
        vidaMaxPercentual = 0f;
    }

    public void AcumularEfeito(TalentEffectEntry e)
    {
        if (e == null)
            return;
        switch (e.stat)
        {
            case StatTalento.Dano:
                danoFlat += e.valorFlat;
                danoPercentual += e.valorPercentual * 0.01f;
                break;
            case StatTalento.VidaMaxima:
                vidaMaxFlat += e.valorFlat;
                vidaMaxPercentual += e.valorPercentual * 0.01f;
                break;
            case StatTalento.CritChance:
                break;
            case StatTalento.ResistenciaMagica:
                break;
            case StatTalento.VelocidadeAtaque:
                break;
        }
    }

    public int ModificarDanoFinal(int valorBase)
    {
        return Mathf.Max(0, Mathf.RoundToInt(valorBase * (1f + danoPercentual) + danoFlat));
    }

    public void AplicarVidaMaxima(UnidadeController u)
    {
        if (u == null)
            return;
        if (Mathf.Approximately(vidaMaxFlat, 0f) && Mathf.Approximately(vidaMaxPercentual, 0f))
            return;
        int vm = Mathf.Max(1, Mathf.RoundToInt((u.vidaMaxima + vidaMaxFlat) * (1f + vidaMaxPercentual)));
        u.vidaMaxima = vm;
    }
}
