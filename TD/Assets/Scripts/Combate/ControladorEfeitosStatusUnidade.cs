using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Gerencia DoT, tint composta em SpriteRenderer e números flutuantes por tick.</summary>
[RequireComponent(typeof(UnidadeController))]
public class ControladorEfeitosStatusUnidade : MonoBehaviour
{
    private UnidadeController _unidade;
    private SpriteRenderer[] _renderers;
    private Color[] _coresBase;
    private bool _cacheInicializado;

    private readonly Dictionary<TipoEfeitoStatusDot, Coroutine> _rotinasPorTipo = new();
    private readonly Dictionary<TipoEfeitoStatusDot, Color> _tintsAtivasPorTipo = new();

    private bool _resolvendoExplosaoCarvao;
    private float _proximaExplosaoCarvaoPermitidaEm;

    private void Awake()
    {
        _unidade = GetComponent<UnidadeController>();
    }

    private void OnDisable()
    {
        foreach (var kv in _rotinasPorTipo)
        {
            if (kv.Value != null)
                StopCoroutine(kv.Value);
        }

        _rotinasPorTipo.Clear();
        _tintsAtivasPorTipo.Clear();
        RestaurarCoresBase();
    }

    public void AplicarOuRenovar(DefinicaoDanoPorTempoSO definicao)
    {
        if (definicao == null || _unidade == null || !_unidade.EstaVivo)
            return;

        var resistencia = _unidade.GetComponent<ResistenciaDano>();
        if (resistencia != null)
        {
            if (definicao.tipoEfeito == TipoEfeitoStatusDot.Veneno && resistencia.EhImuneVeneno) return;
            if (definicao.tipoEfeito == TipoEfeitoStatusDot.Queimadura && resistencia.EhImuneFogo) return;
        }

        GarantirCacheRenderers();

        if (!_resolvendoExplosaoCarvao && TentarExplosaoCarvaoAntesDeAplicarDot(definicao))
            return;

        var tipo = definicao.tipoEfeito;
        if (_rotinasPorTipo.TryGetValue(tipo, out Coroutine antiga) && antiga != null)
            StopCoroutine(antiga);

        _tintsAtivasPorTipo[tipo] = definicao.corTint;
        AplicarTintComposta();

        _rotinasPorTipo[tipo] = StartCoroutine(ExecutarDotCoroutine(definicao));
    }

    private bool TentarExplosaoCarvaoAntesDeAplicarDot(DefinicaoDanoPorTempoSO definicao)
    {
        if (!ReliquiaCarvaoEquipada())
            return false;

        var tipoNovo = definicao.tipoEfeito;
        if (tipoNovo != TipoEfeitoStatusDot.Queimadura && tipoNovo != TipoEfeitoStatusDot.Veneno)
            return false;

        TipoEfeitoStatusDot tipoOutro = ObterOutroDot(tipoNovo);
        if (!_rotinasPorTipo.ContainsKey(tipoOutro))
            return false;

        if (Time.time < _proximaExplosaoCarvaoPermitidaEm)
            return false;

        ConfiguracaoExplosaoCarvaoEnxofreSalitreSO cfg = EfeitoCarvaoEnxofreSalitre.ConfiguracaoAtiva;
        float chance = cfg != null ? cfg.chanceProc : 1f;
        if (Random.value > chance)
            return false;

        int dano = cfg != null ? cfg.danoExplosao : 30;
        TipoDanoCombate tipoDano = cfg != null ? cfg.tipoDano : TipoDanoCombate.ExplosaoAlquimica;

        PedidoDanoFlutuante pedido;
        if (cfg != null && cfg.usarCorPersonalizada)
            pedido = PedidoDanoFlutuante.CriarComCorOverride(dano, tipoDano, cfg.corTextoExplosao);
        else
            pedido = PedidoDanoFlutuante.Criar(dano, tipoDano);

        _resolvendoExplosaoCarvao = true;
        try
        {
            RemoverDoTInternoSemExplosao(TipoEfeitoStatusDot.Queimadura);
            RemoverDoTInternoSemExplosao(TipoEfeitoStatusDot.Veneno);

            ServicoAplicacaoDano.Aplicar(
                _unidade,
                transform.position,
                dano,
                pedido,
                dispararProcsRunaBestiario: false);

            float cd = cfg != null ? cfg.cooldownPorAlvoSegundos : 0f;
            _proximaExplosaoCarvaoPermitidaEm = Time.time + Mathf.Max(0f, cd);
        }
        finally
        {
            _resolvendoExplosaoCarvao = false;
        }

        return true;
    }

    private static TipoEfeitoStatusDot ObterOutroDot(TipoEfeitoStatusDot tipo) =>
        tipo == TipoEfeitoStatusDot.Queimadura ? TipoEfeitoStatusDot.Veneno : TipoEfeitoStatusDot.Queimadura;

    private static bool ReliquiaCarvaoEquipada()
    {
        if (ReliquiasManager.Instance == null)
            return false;
        IReadOnlyList<string> eq = ReliquiasManager.Instance.GetReliquiasEquipadasParaEfeitos();
        for (int i = 0; i < eq.Count; i++)
        {
            if (eq[i] == ReliquiaIds.CarvaoEnxofreSalitre)
                return true;
        }

        return false;
    }

    private void RemoverDoTInternoSemExplosao(TipoEfeitoStatusDot tipo)
    {
        if (_rotinasPorTipo.TryGetValue(tipo, out Coroutine c) && c != null)
            StopCoroutine(c);

        _rotinasPorTipo.Remove(tipo);
        _tintsAtivasPorTipo.Remove(tipo);
        AplicarTintComposta();
    }

    private IEnumerator ExecutarDotCoroutine(DefinicaoDanoPorTempoSO def)
    {
        var tipo = def.tipoEfeito;

        for (int i = 0; i < def.quantidadeTicks; i++)
        {
            yield return new WaitForSeconds(def.intervaloSegundos);

            if (_unidade == null || !_unidade.EstaVivo)
                yield break;

            TipoDanoCombate tipoDano = MapearDotParaTipoDano(def.tipoEfeito);
            var pedido = PedidoDanoFlutuante.Criar(def.danoPorTick, tipoDano);
            ServicoAplicacaoDano.Aplicar(
                _unidade,
                transform.position,
                def.danoPorTick,
                pedido,
                dispararProcsRunaBestiario: false);
        }

        _tintsAtivasPorTipo.Remove(tipo);
        _rotinasPorTipo.Remove(tipo);
        AplicarTintComposta();
    }

    private static TipoDanoCombate MapearDotParaTipoDano(TipoEfeitoStatusDot dot) =>
        dot == TipoEfeitoStatusDot.Queimadura ? TipoDanoCombate.Fogo : TipoDanoCombate.Veneno;

    private void GarantirCacheRenderers()
    {
        if (_cacheInicializado)
            return;

        var lista = new List<SpriteRenderer>();
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr == null)
                continue;
            string n = sr.gameObject.name;
            if (n.IndexOf("Aura", System.StringComparison.OrdinalIgnoreCase) >= 0)
                continue;
            lista.Add(sr);
        }

        _renderers = lista.ToArray();
        _coresBase = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _coresBase[i] = _renderers[i].color;

        _cacheInicializado = true;
    }

    private void AplicarTintComposta()
    {
        if (!_cacheInicializado || _renderers == null)
            return;

        Color fator = Color.white;
        foreach (var kv in _tintsAtivasPorTipo)
            fator = MultiplicarCores(fator, kv.Value);

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null)
                continue;
            _renderers[i].color = MultiplicarCores(_coresBase[i], fator);
        }
    }

    private void RestaurarCoresBase()
    {
        if (!_cacheInicializado || _renderers == null || _coresBase == null)
            return;

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
                _renderers[i].color = _coresBase[i];
        }
    }

    private static Color MultiplicarCores(Color a, Color b)
    {
        return new Color(
            Mathf.Clamp01(a.r * b.r),
            Mathf.Clamp01(a.g * b.g),
            Mathf.Clamp01(a.b * b.b),
            Mathf.Clamp01(a.a * b.a));
    }
}
