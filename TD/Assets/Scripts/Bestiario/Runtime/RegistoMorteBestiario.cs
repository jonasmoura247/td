using UnityEngine;

/// <summary>
/// Incrementa mortes no bestiário do slot ativo quando o inimigo morre.
/// Também exibe notificações de descoberta (primeira morte) e de progresso de estágio
/// via <see cref="NotificacaoToastUI"/>.
/// </summary>
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

        string criaturaId = id.CriaturaId;

        // ── Captura estado ANTES da mutação (valores primitivos) ────────────
        int mortesAntes = 0;
        int estagiosAntes = 0;
        BestiarioCriaturaDef def = null;

        int slotIdx = SaveSlotManager.Instance.GetSlotAtivo();
        DadosSlotSave dadosRef = slotIdx >= 0 ? SaveSlotManager.Instance.GetSlot(slotIdx) : null;

        if (dadosRef != null)
        {
            dadosRef.NormalizarBestiario();
            mortesAntes = BestiarioRegras.ObterMortes(dadosRef, criaturaId);

            var db = BestiarioCatalogoUtil.CarregarOuNull(database);
            def = db?.ObterCriatura(criaturaId);
            if (def != null)
                estagiosAntes = BestiarioRegras.ObterEstagiosCompletos(def, mortesAntes);
        }

        // ── Registra a morte ────────────────────────────────────────────────
        SaveSlotManager.Instance.AtualizarSlotAtivo(d =>
            BestiarioRegras.RegistrarMorte(criaturaId, database, d));

        // ── Notificações (usa o mesmo dadosRef, já mutado) ──────────────────
        if (NotificacaoToastUI.Instance == null || dadosRef == null)
            return;

        int mortesDepois = BestiarioRegras.ObterMortes(dadosRef, criaturaId);

        // Primeira descoberta
        if (mortesAntes == 0 && mortesDepois == 1)
        {
            string nome = def?.nomeExibicao ?? criaturaId;
            NotificacaoToastUI.Instance.Mostrar($"Você descobriu a criatura: {nome}!");
            return;
        }

        // Novo estágio completado
        if (def != null)
        {
            int estagiosDepois = BestiarioRegras.ObterEstagiosCompletos(def, mortesDepois);
            if (estagiosDepois > estagiosAntes)
            {
                string nome = def.nomeExibicao ?? criaturaId;
                NotificacaoToastUI.Instance.Mostrar(
                    $"{nome}: estágio {estagiosDepois}/{def.QuantidadeEstagios} concluído!");
            }
        }
    }
}
