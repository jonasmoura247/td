/// <summary>
/// Dados do perfil atualmente em jogo. Acessível em qualquer cena.
/// Usado por WaveManager, UI, LootManager (por slot), etc.
/// </summary>
public static class PerfilJogadorAtivo
{
    private static DadosSlotSave perfilAtual;

    /// <summary>Raça do jogador na partida atual.</summary>
    public static TipoRaca Raca => perfilAtual?.raca ?? TipoRaca.Goblin;

    /// <summary>Gênero do chefe na partida atual.</summary>
    public static GeneroChefe GeneroChefe => perfilAtual?.generoChefe ?? GeneroChefe.Masculino;

    /// <summary>Se existe um perfil ativo (jogador entrou via seleção de slot).</summary>
    public static bool TemPerfilAtivo => perfilAtual != null && perfilAtual.ocupado;

    /// <summary>Índice do slot (0 a 3) ou -1.</summary>
    public static int IndiceSlot => perfilAtual?.indiceSlot ?? -1;

    /// <summary>UUID do save ativo. Usado para vincular inventário e outros dados.</summary>
    public static string Uuid => perfilAtual?.uuid ?? null;

    /// <summary>Dados completos do perfil (para stats, saves, etc.).</summary>
    public static DadosSlotSave Dados => perfilAtual;

    /// <summary>Define o perfil ativo. Chamado pelo SaveSlotManager ao criar/carregar.</summary>
    public static void DefinirPerfil(DadosSlotSave dados)
    {
        perfilAtual = dados;
        if (dados != null)
        {
            dados.NormalizarBestiario();
            OuroPerfilUtil.AplicarOuroDoPerfilAoManager(dados);
        }
    }

    /// <summary>Limpa o perfil ativo (ex: ao voltar para tela de slots).</summary>
    public static void LimparPerfil()
    {
        perfilAtual = null;
    }

    /// <summary>Retorna o UUID para vincular arquivos (inventário, etc.).</summary>
    public static string GetCaminhoBaseParaSlot()
    {
        if (perfilAtual == null || string.IsNullOrEmpty(perfilAtual.uuid)) return null;
        return perfilAtual.uuid;
    }
}
