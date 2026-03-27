/// <summary>
/// Quando true, o equipamento de relíquias não pode ser alterado (apenas fora da partida).
/// </summary>
public static class EstadoPartidaReliquias
{
    public static bool EmPartida { get; private set; }

    public static void EntrarNaPartida()
    {
        EmPartida = true;
    }

    public static void SairDaPartida()
    {
        EmPartida = false;
    }
}
