using UnityEngine;

/// <summary>Incrementa mortes no bestiário do slot ativo quando o inimigo morre.</summary>
public class RegistoMorteBestiario : MonoBehaviour, IMorteListener
{
    [SerializeField] private BestiarioDatabase database;

    public void OnMorte(UnidadeController unidadeQueMorreu)
    {
        if (unidadeQueMorreu == null || unidadeQueMorreu.faccao != Faccao.Inimigo)
            return;
        if (SaveSlotManager.Instance == null || !PerfilJogadorAtivo.TemPerfilAtivo)
            return;

        var id = unidadeQueMorreu.GetComponent<IdentificadorBestiarioInimigo>();
        if (id == null || string.IsNullOrEmpty(id.CriaturaId))
            return;

        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
            BestiarioRegras.RegistrarMorte(id.CriaturaId, database, d));
    }
}
