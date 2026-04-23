using UnityEngine;

/// <summary>
/// Singleton que decide, para cada fase, se haverá tenda e de que tipo.
/// Persiste a decisão em EstadoEventosRun para impedir re-rolls via restart.
/// Integrado com RNGDeterministico para seed fixa por fase.
/// </summary>
public class SistemaTendasManager : MonoBehaviour
{
    public static SistemaTendasManager Instance { get; private set; }

    private readonly SistemaTendasLogica _logica = new SistemaTendasLogica();
    private readonly GeradorReliquiasTenda _geradorReliquias = new GeradorReliquiasTenda();
    private readonly GeradorHeroisTenda _geradorHerois = new GeradorHeroisTenda();

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

    /// <summary>
    /// Retorna o evento de tenda da fase. Se já gerado → reutiliza (anti-exploit).
    /// Se não → sorteia e persiste no estadoEventos.
    /// </summary>
    public EventoGerado ObterOuGerarTenda(string saveUuid, int mundoId, int faseId, EstadoEventosRun estado)
    {
        if (estado == null)
        {
            Debug.LogError("[TENDA] EstadoEventosRun nulo. Verifique o save.");
            return null;
        }

        var existente = estado.ObterEvento("tenda", mundoId, faseId);
        if (existente != null)
            return existente;

        int seed = RNGDeterministico.GerarSeed(saveUuid, mundoId, faseId, estado.contadorEventos);
        estado.seedAtual = seed;
        Debug.LogFormat("[SEED] Tenda — seed={0} uuid={1} mundo={2} fase={3}", seed, saveUuid, mundoId, faseId);

        var rng = new RNGDeterministico(seed);
        bool temTenda = _logica.DecidarTemTenda(rng, estado.fasesSemTenda);

        var ev = estado.CriarEvento("tenda", mundoId, faseId);
        ev.seedEvento = seed;
        ev.temTenda = temTenda;

        if (temTenda)
        {
            ev.tipoTendaInt = (int)_logica.DecidarTipoTenda(rng);
            estado.fasesSemTenda = 0;
        }
        else
        {
            estado.fasesSemTenda++;
        }

        return ev;
    }

    public EventoGerado PrepararTendaPerfilAtivo(int mundoId, int faseId, ReliquiaDatabase reliquiaDb, HeroiDatabase heroiDb)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return null;

        EventoGerado evento = null;
        SaveSlotManager.Instance.AtualizarSlotAtivo(dados =>
        {
            if (dados == null)
                return;

            dados.NormalizarEstadoEventos();
            evento = ObterOuGerarTenda(dados.uuid, mundoId, faseId, dados.estadoEventos);
            if (evento == null || !evento.temTenda)
                return;

            GarantirOpcoesTenda(evento, dados, reliquiaDb, heroiDb);
        });

        return evento;
    }

    public EventoGerado ForcarTendaPerfilAtivo(int mundoId, int faseId, TipoTenda tipoTenda, ReliquiaDatabase reliquiaDb, HeroiDatabase heroiDb)
    {
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return null;

        EventoGerado evento = null;
        SaveSlotManager.Instance.AtualizarSlotAtivo(dados =>
        {
            if (dados == null)
                return;

            dados.NormalizarEstadoEventos();
            string chave = EstadoEventosRun.CriarChave("tenda", mundoId, faseId);
            evento = dados.estadoEventos.BuscarPorChave(chave) ?? dados.estadoEventos.CriarEvento("tenda", mundoId, faseId);

            if (evento.seedEvento == 0)
                evento.seedEvento = RNGDeterministico.GerarSeed(dados.uuid, mundoId, faseId, dados.estadoEventos.contadorEventos);

            evento.temTenda = true;
            evento.tipoTendaInt = (int)tipoTenda;
            evento.foiVisitada = false;
            evento.reliquiaResgatada = false;
            evento.heroiResgatado = false;
            evento.opcoesReliquias = new System.Collections.Generic.List<string>();
            evento.opcoesHerois = new System.Collections.Generic.List<string>();
            evento.decisoesTomadas = new System.Collections.Generic.List<string> { "debug:tenda_forcada" };

            GarantirOpcoesTenda(evento, dados, reliquiaDb, heroiDb);
        });

        return evento;
    }

    public bool ReivindicarReliquiaPerfilAtivo(EventoGerado evento, string idReliquia)
    {
        if (evento == null || string.IsNullOrEmpty(idReliquia))
            return false;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        bool sucesso = false;
        SaveSlotManager.Instance.AtualizarSlotAtivo(dados =>
        {
            if (dados == null)
                return;

            var salvo = dados.estadoEventos?.BuscarPorChave(evento.chaveEvento);
            if (salvo == null || !salvo.opcoesReliquias.Contains(idReliquia) || salvo.reliquiaResgatada)
                return;

            if (dados.reliquiasPossuidas == null)
                dados.reliquiasPossuidas = new System.Collections.Generic.List<string>();
            if (dados.reliquiasPossuidas.Contains(idReliquia))
                return;

            dados.reliquiasPossuidas.Add(idReliquia);
            salvo.reliquiaResgatada = true;
            salvo.foiVisitada = true;
            if (salvo.decisoesTomadas == null) salvo.decisoesTomadas = new System.Collections.Generic.List<string>();
            salvo.decisoesTomadas.Add($"reliquia:{idReliquia}");
            sucesso = true;
        });

        return sucesso;
    }

    public bool ReivindicarHeroiPerfilAtivo(EventoGerado evento, string idHeroi, int duracaoBase = 3, int custoBase = 100, int duracaoRenovacao = 2)
    {
        if (evento == null || string.IsNullOrEmpty(idHeroi))
            return false;
        if (!PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        bool sucesso = false;
        SaveSlotManager.Instance.AtualizarSlotAtivo(dados =>
        {
            if (dados == null)
                return;

            var salvo = dados.estadoEventos?.BuscarPorChave(evento.chaveEvento);
            if (salvo == null || !salvo.opcoesHerois.Contains(idHeroi) || salvo.heroiResgatado)
                return;

            if (dados.heroisContratados == null) dados.heroisContratados = new System.Collections.Generic.List<string>();
            if (dados.contratosHerois == null) dados.contratosHerois = new System.Collections.Generic.List<ContratoHeroi>();

            var contrato = ObterContrato(dados, idHeroi);
            if (contrato != null)
            {
                contrato.fasesRestantes += duracaoRenovacao;
                contrato.fasesTotais += duracaoRenovacao;
                contrato.vezesRenovado++;
                contrato.avisoPenultimaFaseEnviado = false;
            }
            else
            {
                if (dados.heroisContratados.Count >= 3)
                    return;

                dados.heroisContratados.Add(idHeroi);
                dados.contratosHerois.Add(new ContratoHeroi
                {
                    idHeroi = idHeroi,
                    fasesRestantes = duracaoBase,
                    fasesTotais = duracaoBase,
                    custoRenovacaoBase = custoBase,
                    vezesRenovado = 0
                });
            }

            salvo.heroiResgatado = true;
            salvo.foiVisitada = true;
            if (salvo.decisoesTomadas == null) salvo.decisoesTomadas = new System.Collections.Generic.List<string>();
            salvo.decisoesTomadas.Add($"heroi:{idHeroi}");
            sucesso = true;
        });

        return sucesso;
    }

    public bool MarcarEventoEspecialVisitadoPerfilAtivo(EventoGerado evento)
    {
        if (evento == null || !PerfilJogadorAtivo.TemPerfilAtivo || SaveSlotManager.Instance == null)
            return false;

        bool sucesso = false;
        SaveSlotManager.Instance.AtualizarSlotAtivo(dados =>
        {
            var salvo = dados?.estadoEventos?.BuscarPorChave(evento.chaveEvento);
            if (salvo == null)
                return;

            salvo.foiVisitada = true;
            if (salvo.decisoesTomadas == null) salvo.decisoesTomadas = new System.Collections.Generic.List<string>();
            salvo.decisoesTomadas.Add("evento_especial:visitado");
            sucesso = true;
        });

        return sucesso;
    }

    public static bool EventoFoiResolvido(EventoGerado evento)
    {
        if (evento == null || !evento.temTenda)
            return true;

        TipoTenda tipo = ObterTipoTenda(evento);
        return tipo switch
        {
            TipoTenda.Mercador => evento.reliquiaResgatada,
            TipoTenda.Herois => evento.heroiResgatado,
            TipoTenda.Mista => evento.reliquiaResgatada && evento.heroiResgatado,
            TipoTenda.EventoEspecial => evento.foiVisitada,
            _ => evento.foiVisitada
        };
    }

    /// <summary>Converte o int salvo no EventoGerado para o enum TipoTenda.</summary>
    public static TipoTenda ObterTipoTenda(EventoGerado ev) => (TipoTenda)ev.tipoTendaInt;

    private void GarantirOpcoesTenda(EventoGerado evento, DadosSlotSave dados, ReliquiaDatabase reliquiaDb, HeroiDatabase heroiDb)
    {
        if (evento.seedEvento == 0)
            evento.seedEvento = RNGDeterministico.GerarSeed(dados.uuid, evento.mundoId, evento.faseId, dados.estadoEventos.contadorEventos);

        TipoTenda tipo = ObterTipoTenda(evento);
        bool precisaReliquia = tipo == TipoTenda.Mercador || tipo == TipoTenda.Mista;
        bool precisaHeroi = tipo == TipoTenda.Herois || tipo == TipoTenda.Mista;

        if (precisaReliquia && (evento.opcoesReliquias == null || evento.opcoesReliquias.Count == 0))
        {
            var rngReliquia = new RNGDeterministico(evento.seedEvento ^ 0x55AA10);
            evento.opcoesReliquias = _geradorReliquias.GerarOpcoes(reliquiaDb, dados.reliquiasPossuidas, rngReliquia);
        }

        if (precisaHeroi && (evento.opcoesHerois == null || evento.opcoesHerois.Count == 0))
        {
            var rngHeroi = new RNGDeterministico(evento.seedEvento ^ 0x33CC77);
            evento.opcoesHerois = _geradorHerois.GerarOpcoes(heroiDb, rngHeroi);
        }
    }

    private static ContratoHeroi ObterContrato(DadosSlotSave dados, string idHeroi)
    {
        if (dados?.contratosHerois == null)
            return null;

        for (int i = 0; i < dados.contratosHerois.Count; i++)
        {
            var contrato = dados.contratosHerois[i];
            if (contrato != null && contrato.idHeroi == idHeroi)
                return contrato;
        }

        return null;
    }
}
