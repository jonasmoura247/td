using UnityEngine;

/// <summary>Aplica dano em <see cref="IDanificavel"/> e exibe texto flutuante (unidades com opção de procs de runa).</summary>
public static class ServicoAplicacaoDano
{
    public static void Aplicar(
        IDanificavel alvo,
        Vector3 posicaoFeedback,
        int quantidade,
        PedidoDanoFlutuante pedidoApresentacao,
        bool dispararProcsRunaBestiario)
    {
        if (alvo == null || quantidade <= 0 || !alvo.EstaVivo)
            return;

        PedidoDanoFlutuante pedido = pedidoApresentacao;
        if (pedido.ValorExibicao <= 0)
            pedido.ValorExibicao = quantidade;

        ServicoDanoFlutuante.Mostrar(posicaoFeedback, pedido);

        switch (alvo)
        {
            case UnidadeController unidade:
                if (dispararProcsRunaBestiario)
                    unidade.ReceberDano(quantidade);
                else
                    unidade.ReceberDanoSemProcs(quantidade);
                break;
            case TorreController torre:
                torre.AplicarDanoSomenteVida(quantidade);
                break;
            case EstruturasController estrutura:
                estrutura.AplicarDanoSomenteVida(quantidade);
                break;
            default:
                alvo.ReceberDano(quantidade);
                break;
        }
    }
}
