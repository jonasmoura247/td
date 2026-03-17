using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia as relíquias equipadas do perfil ativo.
/// Cada slot de save tem seu próprio conjunto de relíquias (máx 5).
/// </summary>
public class ReliquiasManager : MonoBehaviour
{
    public static ReliquiasManager Instance { get; private set; }

    private const int MAX_RELIQUIAS = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Retorna as relíquias equipadas do slot ativo.</summary>
    public IReadOnlyList<string> GetReliquiasEquipadas()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            return new List<string>();

        return PerfilJogadorAtivo.Dados?.reliquiasEquipadas ?? new List<string>();
    }

    /// <summary>Equipa uma relíquia. Retorna true se conseguiu.</summary>
    public bool EquiparReliquia(string idReliquia)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        var lista = PerfilJogadorAtivo.Dados.reliquiasEquipadas;
        if (lista.Count >= MAX_RELIQUIAS)
            return false;

        if (lista.Contains(idReliquia))
            return false;

        lista.Add(idReliquia);
        SaveSlotManager.Instance.AtualizarSlotAtivo(d => { /* já alteramos Dados diretamente */ });
        return true;
    }

    /// <summary>Desequipa uma relíquia.</summary>
    public bool DesequiparReliquia(string idReliquia)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        bool removido = PerfilJogadorAtivo.Dados.reliquiasEquipadas.Remove(idReliquia);
        if (removido)
            SaveSlotManager.Instance.AtualizarSlotAtivo(d => { });

        return removido;
    }

    /// <summary>Se pode equipar mais relíquias.</summary>
    public bool PodeEquiparMais() => GetReliquiasEquipadas().Count < MAX_RELIQUIAS;
}
