using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Dados de um slot de save. Cada slot representa um perfil/jogo completo.
/// Raça do jogador, gênero do chefe, progresso, etc.
/// </summary>
[Serializable]
public class DadosSlotSave
{
    /// <summary>Indice do slot (0 a 3).</summary>
    public int indiceSlot;

    /// <summary>UUID único deste save. Vincula inventário e outros dados ao perfil.</summary>
    public string uuid;

    /// <summary>Raça escolhida pelo jogador para este perfil.</summary>
    public TipoRaca raca;

    /// <summary>Gênero do chefe deste perfil.</summary>
    public GeneroChefe generoChefe;

    /// <summary>Se o slot está ocupado (tem um jogo criado).</summary>
    public bool ocupado;

    /// <summary>Data/hora da criação do perfil (formato ISO).</summary>
    public string dataCriacao;

    /// <summary>Data/hora do último save/jogo.</summary>
    public string dataUltimoJogo;

    /// <summary>Nome opcional do perfil (ex: "Meu Goblin").</summary>
    public string nomePerfil;

    /// <summary>Tempo total de jogo em segundos (para exibição).</summary>
    public float tempoJogoSegundos;

    /// <summary>Legado: lista linear de equipadas. Migrado para reliquiasPorSlot ao carregar.</summary>
    public List<string> reliquiasEquipadas = new List<string>();

    /// <summary>Relíquias que o jogador possui neste save (inventário). Não compartilhado entre slots.</summary>
    public List<string> reliquiasPossuidas = new List<string>();

    /// <summary>Cinco slots de equipamento (índice 0–4). String vazia = slot vazio.</summary>
    public string[] reliquiasPorSlot = new string[5] { "", "", "", "", "" };

    /// <summary>IDs dos heróis contratados (máx 3). Cada slot tem seu próprio conjunto.</summary>
    public List<string> heroisContratados = new List<string>();

    public DadosSlotSave()
    {
        indiceSlot = 0;
        raca = TipoRaca.Goblin;
        generoChefe = GeneroChefe.Masculino;
        ocupado = false;
        dataCriacao = "";
        dataUltimoJogo = "";
        nomePerfil = "";
        tempoJogoSegundos = 0f;
    }

    public DadosSlotSave(int slot, TipoRaca racaEscolhida, GeneroChefe genero)
    {
        indiceSlot = slot;
        uuid = Guid.NewGuid().ToString();
        raca = racaEscolhida;
        generoChefe = genero;
        ocupado = true;
        dataCriacao = DateTime.Now.ToString("O");
        dataUltimoJogo = dataCriacao;
        nomePerfil = ObterNomePadrao(racaEscolhida);
        tempoJogoSegundos = 0f;
        reliquiasPossuidas = new List<string>();
        reliquiasPorSlot = new string[5] { "", "", "", "", "" };
        if (racaEscolhida == TipoRaca.Goblin)
            reliquiasPossuidas.Add(ReliquiaIds.OlhoQueNadaVe);
    }

    /// <summary>Garante que o save tem um UUID. Usado na migração de saves antigos.</summary>
    public void GarantirUuid()
    {
        if (string.IsNullOrEmpty(uuid))
            uuid = Guid.NewGuid().ToString();
    }

    /// <summary>Normaliza listas, migra legado e garante relíquia inicial do Goblin.</summary>
    public void NormalizarReliquias()
    {
        if (reliquiasPossuidas == null)
            reliquiasPossuidas = new List<string>();
        if (reliquiasPorSlot == null || reliquiasPorSlot.Length != 5)
            reliquiasPorSlot = new string[5] { "", "", "", "", "" };

        if (reliquiasEquipadas != null && reliquiasEquipadas.Count > 0)
        {
            for (int i = 0; i < reliquiasEquipadas.Count && i < 5; i++)
            {
                if (!string.IsNullOrEmpty(reliquiasEquipadas[i]))
                    reliquiasPorSlot[i] = reliquiasEquipadas[i];
            }
            reliquiasEquipadas.Clear();
        }

        if (ocupado && raca == TipoRaca.Goblin && !reliquiasPossuidas.Contains(ReliquiaIds.OlhoQueNadaVe))
            reliquiasPossuidas.Add(ReliquiaIds.OlhoQueNadaVe);
    }

    private static string ObterNomePadrao(TipoRaca raca)
    {
        return raca switch
        {
            TipoRaca.Goblin => "Goblin",
            TipoRaca.Elfo => "Elfo",
            TipoRaca.Anao => "Anão",
            TipoRaca.Humano => "Humano",
            _ => "Perfil"
        };
    }

    /// <summary>Retorna descrição curta para exibir no slot (ex: "Goblin - 2h").</summary>
    public string ObterDescricaoResumida()
    {
        if (!ocupado) return "Vazio";
        string tempoStr = FormatarTempoJogo(tempoJogoSegundos);
        return $"{nomePerfil} - {tempoStr}";
    }

    public static string FormatarTempoJogo(float segundos)
    {
        int total = Mathf.FloorToInt(segundos);
        int h = total / 3600;
        int m = (total % 3600) / 60;
        if (h > 0)
            return $"{h}h {m}min";
        if (m > 0)
            return $"{m} min";
        return $"{total}s";
    }
}
