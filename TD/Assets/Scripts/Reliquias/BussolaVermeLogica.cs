/// <summary>
/// Lógica pura da relíquia Bússola do Verme: converte LadoSpawn em dados de UI.
/// Sem dependências Unity — totalmente testável em EditMode.
/// </summary>
public static class BussolaVermeLogica
{
    /// <summary>Rotação em Z (graus) para apontar a seta na direção correta.</summary>
    public static float RotacaoZPorLado(LadoSpawn lado) => lado switch
    {
        LadoSpawn.Esquerda => 180f,
        LadoSpawn.Direita  => 0f,
        LadoSpawn.Cima     => 90f,
        LadoSpawn.Baixo    => 270f,
        _                  => 0f
    };

    /// <summary>Rótulo legível para exibir no HUD.</summary>
    public static string NomePorLado(LadoSpawn lado) => lado switch
    {
        LadoSpawn.Esquerda => "ESQUERDA",
        LadoSpawn.Direita  => "DIREITA",
        LadoSpawn.Cima     => "CIMA",
        LadoSpawn.Baixo    => "BAIXO",
        _                  => lado.ToString().ToUpper()
    };
}
