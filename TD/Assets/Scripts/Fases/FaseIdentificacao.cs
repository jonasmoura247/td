/// <summary>
/// Comunica entre cenas qual fase o jogador está jogando.
/// Definido em BotaoFaseController antes de carregar a cena do mapa,
/// lido por RunExpPartida ao registrar a vitória.
/// </summary>
public static class FaseIdentificacao
{
    public static string FaseAtualId { get; set; }

    public static void Limpar() => FaseAtualId = null;
}
