using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia os heróis contratados do perfil ativo.
/// Cada slot de save tem seu próprio conjunto de heróis (máx 3).
/// </summary>
public class HeroisManager : MonoBehaviour
{
    public static HeroisManager Instance { get; private set; }

    private const int MAX_HEROIS = 3;

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

    /// <summary>Retorna os heróis contratados do slot ativo.</summary>
    public IReadOnlyList<string> GetHeroisContratados()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            return new List<string>();

        return PerfilJogadorAtivo.Dados?.heroisContratados ?? new List<string>();
    }

    /// <summary>Contrata um herói. Retorna true se conseguiu.</summary>
    public bool ContratarHeroi(string idHeroi)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        var lista = PerfilJogadorAtivo.Dados.heroisContratados;
        if (lista.Count >= MAX_HEROIS)
            return false;

        if (lista.Contains(idHeroi))
            return false;

        lista.Add(idHeroi);
        SaveSlotManager.Instance.AtualizarSlotAtivo(d => { });
        return true;
    }

    /// <summary>Remove um herói.</summary>
    public bool RemoverHeroi(string idHeroi)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        bool removido = PerfilJogadorAtivo.Dados.heroisContratados.Remove(idHeroi);
        if (removido)
            SaveSlotManager.Instance.AtualizarSlotAtivo(d => { });

        return removido;
    }

    /// <summary>Se pode contratar mais heróis.</summary>
    public bool PodeContratarMais() => GetHeroisContratados().Count < MAX_HEROIS;
}
