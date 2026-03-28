using UnityEngine;

public class ExpAoMorrerInimigo : MonoBehaviour, IMorteListener
{
    [SerializeField] private bool ehBoss;

    public void OnMorte(UnidadeController unidadeQueMorreu)
    {
        if (unidadeQueMorreu == null || unidadeQueMorreu.faccao != Faccao.Inimigo)
            return;
        if (RunExpPartida.Instance == null || RunExpPartida.Instance.RunResolvida)
            return;
        RunExpPartida.Instance.RegistrarMorteInimigo(ehBoss);
    }
}
