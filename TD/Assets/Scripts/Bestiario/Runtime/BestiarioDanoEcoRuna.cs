using UnityEngine;

public static class BestiarioDanoEcoRuna
{
    public static void TentarAposDanoDoJogador(UnidadeController alvo, int quantidadeRecebida)
    {
        if (alvo == null || !alvo.EstaVivo || quantidadeRecebida <= 0)
            return;
        if (alvo.faccao != Faccao.Inimigo)
            return;

        var st = alvo.GetComponent<RunaEstadoInimigo>();
        if (st == null || !st.temDanoEco)
            return;
        if (Random.value > st.chanceDanoEco)
            return;

        int extra = Mathf.Max(1, Mathf.CeilToInt(st.fracaoVidaParaEco * alvo.vidaMaxima));
        ServicoAplicacaoDano.Aplicar(
            alvo,
            alvo.transform.position,
            extra,
            PedidoDanoFlutuante.Criar(extra, TipoDanoCombate.EcoRuna),
            dispararProcsRunaBestiario: false);
    }
}
