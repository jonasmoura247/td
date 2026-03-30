using UnityEngine;

/// <summary>
/// Quando <see cref="Ativo"/> é true, liberar/equipar runas ignora pontos, estágios e custo de liberação
/// (útil para testar efeitos em jogo). Não altera remoção com ouro, a menos que configures o painel.
/// </summary>
public static class BestiarioModoTeste
{
    public static bool Ativo { get; set; }
}
