using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia os heróis contratados do perfil ativo e seus contratos.
/// Cada slot de save tem seu próprio conjunto de heróis (máx 3).
/// Suporta spawn de heróis via HeroiDatabase e sistema de contratos com duração.
/// </summary>
public class HeroisManager : MonoBehaviour
{
    public static HeroisManager Instance { get; private set; }

    [SerializeField] private HeroiDatabase heroiDatabase;

    private const int MaxHerois = 3;

    /// <summary>Disparado quando um contrato entra na penúltima fase.</summary>
    public event Action<ContratoHeroi> OnContratoExpirando;
    /// <summary>Disparado quando um contrato expira (fases zeradas).</summary>
    public event Action<ContratoHeroi> OnContratoExpirado;

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

    // ── Heróis ativos ────────────────────────────────────────────────────

    /// <summary>Retorna os IDs dos heróis contratados do slot ativo.</summary>
    public IReadOnlyList<string> GetHeroisContratados()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            return new List<string>();
        return PerfilJogadorAtivo.Dados?.heroisContratados ?? new List<string>();
    }

    /// <summary>Se pode contratar mais heróis.</summary>
    public bool PodeContratarMais() => GetHeroisContratados().Count < MaxHerois;

    // ── Contratos ────────────────────────────────────────────────────────

    /// <summary>Retorna todos os contratos ativos do perfil.</summary>
    public IReadOnlyList<ContratoHeroi> GetContratosAtivos()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            return new List<ContratoHeroi>();
        var dados = PerfilJogadorAtivo.Dados;
        if (dados.contratosHerois == null) dados.contratosHerois = new List<ContratoHeroi>();
        return dados.contratosHerois;
    }

    /// <summary>Retorna o contrato de um herói específico, ou null se não contratado.</summary>
    public ContratoHeroi GetContrato(string idHeroi)
    {
        var contratos = GetContratosAtivos();
        foreach (var c in contratos)
            if (c.idHeroi == idHeroi)
                return c;
        return null;
    }

    /// <summary>
    /// Contrata um herói com contrato de duração fixa (3–5 fases).
    /// Persiste no save — não pode ser re-rolado via restart.
    /// </summary>
    public bool ContratarHeroiComContrato(string idHeroi, int duracaoFases, int custoBase = 100)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        var dados = PerfilJogadorAtivo.Dados;
        if (dados.heroisContratados == null) dados.heroisContratados = new List<string>();
        if (dados.contratosHerois == null) dados.contratosHerois = new List<ContratoHeroi>();

        if (dados.heroisContratados.Count >= MaxHerois) return false;
        if (dados.heroisContratados.Contains(idHeroi)) return false;

        var contrato = new ContratoHeroi
        {
            idHeroi = idHeroi,
            fasesRestantes = duracaoFases,
            fasesTotais = duracaoFases,
            custoRenovacaoBase = custoBase,
            vezesRenovado = 0
        };
        dados.heroisContratados.Add(idHeroi);
        dados.contratosHerois.Add(contrato);

        SaveSlotManager.Instance.AtualizarSlotAtivo(d => { });
        Debug.LogFormat("[CONTRATO] Herói '{0}' contratado por {1} fases. Custo base: {2}", idHeroi, duracaoFases, custoBase);
        return true;
    }

    /// <summary>
    /// Renova o contrato de um herói. Custo cresce a cada renovação.
    /// </summary>
    public bool RenovarContrato(string idHeroi, int novasDuracaoFases)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        var contrato = GetContrato(idHeroi);
        if (contrato == null) return false;

        contrato.fasesRestantes += novasDuracaoFases;
        contrato.fasesTotais += novasDuracaoFases;
        contrato.vezesRenovado++;

        SaveSlotManager.Instance.AtualizarSlotAtivo(d => { });
        Debug.LogFormat("[RENOVAÇÃO] Herói '{0}' renovado por mais {1} fases. Custo: {2}",
            idHeroi, novasDuracaoFases, contrato.CalcularCustoRenovacao());
        return true;
    }

    /// <summary>
    /// Deve ser chamado ao completar uma fase.
    /// Decrementa fases restantes de todos os contratos e dispara avisos/expirações.
    /// </summary>
    public void AvançarFase()
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo) return;

        var dados = PerfilJogadorAtivo.Dados;
        if (dados.contratosHerois == null) return;

        var expirados = new List<ContratoHeroi>();
        foreach (var contrato in dados.contratosHerois)
        {
            if (contrato.fasesRestantes > 0)
                contrato.fasesRestantes--;

            if (contrato.fasesRestantes <= 0)
            {
                expirados.Add(contrato);
            }
            else if (contrato.NaPenultimaFase && !contrato.avisoPenultimaFaseEnviado)
            {
                contrato.avisoPenultimaFaseEnviado = true;
                OnContratoExpirando?.Invoke(contrato);
                Debug.LogFormat("[CONTRATO] Herói '{0}' na penúltima fase!", contrato.idHeroi);
            }
        }

        foreach (var expirado in expirados)
        {
            dados.heroisContratados.Remove(expirado.idHeroi);
            dados.contratosHerois.Remove(expirado);
            OnContratoExpirado?.Invoke(expirado);
            Debug.LogFormat("[CONTRATO] Contrato de '{0}' expirou.", expirado.idHeroi);
        }

        if (SaveSlotManager.Instance != null)
            SaveSlotManager.Instance.AtualizarSlotAtivo(d => { });
    }

    // ── Spawn ────────────────────────────────────────────────────────────

    /// <summary>
    /// Instancia o prefab do herói na posição indicada.
    /// Requer HeroiDatabase configurado neste Manager.
    /// </summary>
    public GameObject SpawnarHeroi(string idHeroi, Vector3 posicao)
    {
        if (heroiDatabase == null)
        {
            Debug.LogError("[HeroisManager] HeroiDatabase não configurado.");
            return null;
        }

        var def = heroiDatabase.ObterPorId(idHeroi);
        if (def?.prefab == null)
        {
            Debug.LogWarning($"[HeroisManager] Prefab não encontrado para herói: {idHeroi}");
            return null;
        }

        var go = Instantiate(def.prefab, posicao, Quaternion.identity);
        return go;
    }

    public string ObterNomeExibicao(string idHeroi)
    {
        if (heroiDatabase == null || string.IsNullOrEmpty(idHeroi))
            return idHeroi;

        var def = heroiDatabase.ObterPorId(idHeroi);
        return def != null && !string.IsNullOrEmpty(def.nomeExibicao) ? def.nomeExibicao : idHeroi;
    }

    // ── Legacy (mantém compatibilidade) ──────────────────────────────────

    /// <summary>Contrata herói sem sistema de contratos (legado). Prefira ContratarHeroiComContrato.</summary>
    public bool ContratarHeroi(string idHeroi)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;
        var lista = PerfilJogadorAtivo.Dados.heroisContratados;
        if (lista.Count >= MaxHerois || lista.Contains(idHeroi)) return false;
        lista.Add(idHeroi);
        SaveSlotManager.Instance.AtualizarSlotAtivo(d => { });
        return true;
    }

    /// <summary>Remove um herói e seu contrato associado.</summary>
    public bool RemoverHeroi(string idHeroi)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        var dados = PerfilJogadorAtivo.Dados;
        bool removido = dados.heroisContratados.Remove(idHeroi);

        if (removido)
        {
            dados.contratosHerois?.RemoveAll(c => c.idHeroi == idHeroi);
            SaveSlotManager.Instance.AtualizarSlotAtivo(d => { });
        }
        return removido;
    }
}
