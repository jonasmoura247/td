using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Painel simples de tenda para testes.
/// Funciona com botões dinâmicos e textos, sem depender de arte final.
/// </summary>
public class PainelTendaController : MonoBehaviour
{
    [Header("Estrutura")]
    [SerializeField] private GameObject painel;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoDescricao;

    [Header("Seção Relíquias")]
    [SerializeField] private GameObject secaoReliquias;
    [SerializeField] private Transform containerReliquias;
    [SerializeField] private GameObject prefabBotaoOpcao;

    [Header("Seção Heróis")]
    [SerializeField] private GameObject secaoHerois;
    [SerializeField] private Transform containerHerois;

    [Header("Bancos de Dados")]
    [SerializeField] private ReliquiaDatabase reliquiaDatabase;
    [SerializeField] private HeroiDatabase heroiDatabase;

    private readonly List<GameObject> _instancias = new List<GameObject>();
    private EventoGerado _eventoAtual;
    private TendaMarcadorMapa _marcadorAtual;

    private void Start()
    {
        if (painel != null)
            painel.SetActive(false);

        Debug.LogFormat(
            "[TENDA DEBUG] PainelTenda Start. painel={0} titulo={1} descricao={2} secaoReliquias={3} secaoHerois={4} prefabBotao={5}",
            painel != null ? painel.name : "null",
            textoTitulo != null ? textoTitulo.name : "null",
            textoDescricao != null ? textoDescricao.name : "null",
            secaoReliquias != null ? secaoReliquias.name : "null",
            secaoHerois != null ? secaoHerois.name : "null",
            prefabBotaoOpcao != null ? prefabBotaoOpcao.name : "null");
    }

    public void MostrarTenda(TendaMarcadorMapa marcador, EventoGerado evento)
    {
        Debug.LogFormat(
            "[TENDA DEBUG] MostrarTenda chamado. marcador={0} eventoNull={1} painelNull={2} temTenda={3}",
            marcador != null ? marcador.name : "null",
            evento == null,
            painel == null,
            evento != null && evento.temTenda);

        _marcadorAtual = marcador;
        _eventoAtual = evento;

        if (painel != null)
        {
            painel.SetActive(true);
            Debug.LogFormat("[TENDA DEBUG] Painel '{0}' ativado.", painel.name);
        }
        else
        {
            Debug.LogError("[TENDA DEBUG] PainelTendaController está sem referência do painel.");
        }

        PreencherCabecalho(evento);
        RecriarOpcoes();
    }

    public void FecharTenda()
    {
        Debug.Log("[TENDA DEBUG] FecharTenda chamado.");
        _eventoAtual = null;
        _marcadorAtual = null;

        LimparInstancias();
        if (painel != null)
            painel.SetActive(false);
    }

    private void PreencherCabecalho(EventoGerado evento)
    {
        if (textoTitulo != null)
            textoTitulo.text = evento != null && evento.temTenda
                ? $"Tenda: {SistemaTendasManager.ObterTipoTenda(evento)}"
                : "Sem tenda";

        if (textoDescricao == null)
            return;

        if (evento == null || !evento.temTenda)
        {
            textoDescricao.text = "Nenhum evento disponível.";
            return;
        }

        textoDescricao.text = SistemaTendasManager.ObterTipoTenda(evento) switch
        {
            TipoTenda.Mercador => "Escolha 1 relíquia entre as opções abaixo.",
            TipoTenda.Herois => "Contrate ou renove 1 herói nesta parada.",
            TipoTenda.Mista => "Você pode pegar 1 relíquia e 1 herói nesta tenda.",
            TipoTenda.EventoEspecial => "Evento especial de teste. Clique para marcar como visitado.",
            _ => "Evento desconhecido."
        };
    }

    private void RecriarOpcoes()
    {
        Debug.LogFormat(
            "[TENDA DEBUG] RecriarOpcoes. eventoNull={0} secaoReliquias={1} secaoHerois={2} prefabBotao={3}",
            _eventoAtual == null,
            secaoReliquias != null,
            secaoHerois != null,
            prefabBotaoOpcao != null);
        LimparInstancias();

        bool exibirReliquias = _eventoAtual != null &&
                               (_eventoAtual.tipoTendaInt == (int)TipoTenda.Mercador ||
                                _eventoAtual.tipoTendaInt == (int)TipoTenda.Mista);
        bool exibirHerois = _eventoAtual != null &&
                            (_eventoAtual.tipoTendaInt == (int)TipoTenda.Herois ||
                             _eventoAtual.tipoTendaInt == (int)TipoTenda.Mista);

        if (secaoReliquias != null) secaoReliquias.SetActive(exibirReliquias);
        if (secaoHerois != null) secaoHerois.SetActive(exibirHerois);

        if (_eventoAtual == null)
            return;

        if (SistemaTendasManager.ObterTipoTenda(_eventoAtual) == TipoTenda.EventoEspecial)
        {
            CriarBotao(containerHerois != null ? containerHerois : containerReliquias, "Receber evento", OnEventoEspecialEscolhido);
            return;
        }

        if (exibirReliquias)
            PopularReliquias();
        if (exibirHerois)
            PopularHerois();
    }

    private void PopularReliquias()
    {
        if (containerReliquias == null || prefabBotaoOpcao == null)
            return;

        if (_eventoAtual.reliquiaResgatada)
        {
            CriarBotao(containerReliquias, "Relíquia já resgatada", null, false);
            return;
        }

        for (int i = 0; i < _eventoAtual.opcoesReliquias.Count; i++)
        {
            string idReliquia = _eventoAtual.opcoesReliquias[i];
            string nome = ObterNomeReliquia(idReliquia);
            CriarBotao(containerReliquias, nome, () => OnReliquiaEscolhida(idReliquia));
        }

        if (_eventoAtual.opcoesReliquias.Count == 0)
            CriarBotao(containerReliquias, "Sem relíquias disponíveis", null, false);
    }

    private void PopularHerois()
    {
        if (containerHerois == null || prefabBotaoOpcao == null)
            return;

        if (_eventoAtual.heroiResgatado)
        {
            CriarBotao(containerHerois, "Herói já definido", null, false);
            return;
        }

        for (int i = 0; i < _eventoAtual.opcoesHerois.Count; i++)
        {
            string idHeroi = _eventoAtual.opcoesHerois[i];
            bool emContrato = HeroisManager.Instance != null && HeroisManager.Instance.GetContrato(idHeroi) != null;
            string texto = emContrato
                ? $"{ObterNomeHeroi(idHeroi)} (Renovar +2 fases)"
                : $"{ObterNomeHeroi(idHeroi)} (Contrato 3 fases)";
            CriarBotao(containerHerois, texto, () => OnHeroiEscolhido(idHeroi));
        }

        if (_eventoAtual.opcoesHerois.Count == 0)
            CriarBotao(containerHerois, "Sem heróis disponíveis", null, false);
    }

    private void OnReliquiaEscolhida(string idReliquia)
    {
        if (_eventoAtual == null || SistemaTendasManager.Instance == null)
            return;

        bool sucesso = SistemaTendasManager.Instance.ReivindicarReliquiaPerfilAtivo(_eventoAtual, idReliquia);
        Debug.Log(sucesso
            ? $"[TENDA] Relíquia escolhida: {idReliquia}"
            : $"[TENDA] Não foi possível resgatar a relíquia: {idReliquia}");
        AtualizarEventoAtual();
        RecriarOpcoes();
    }

    private void OnHeroiEscolhido(string idHeroi)
    {
        if (_eventoAtual == null || SistemaTendasManager.Instance == null)
            return;

        bool sucesso = SistemaTendasManager.Instance.ReivindicarHeroiPerfilAtivo(_eventoAtual, idHeroi);
        Debug.Log(sucesso
            ? $"[TENDA] Herói escolhido: {idHeroi}"
            : $"[TENDA] Não foi possível contratar/renovar o herói: {idHeroi}");
        AtualizarEventoAtual();
        RecriarOpcoes();
    }

    private void OnEventoEspecialEscolhido()
    {
        if (_eventoAtual == null || SistemaTendasManager.Instance == null)
            return;

        SistemaTendasManager.Instance.MarcarEventoEspecialVisitadoPerfilAtivo(_eventoAtual);
        AtualizarEventoAtual();
        RecriarOpcoes();
    }

    private void AtualizarEventoAtual()
    {
        if (_eventoAtual == null || !PerfilJogadorAtivo.TemPerfilAtivo)
            return;

        _eventoAtual = PerfilJogadorAtivo.Dados?.estadoEventos?.BuscarPorChave(_eventoAtual.chaveEvento);
    }

    private void CriarBotao(Transform container, string texto, System.Action onClick, bool interativo = true)
    {
        if (container == null || prefabBotaoOpcao == null)
            return;

        var instancia = Instantiate(prefabBotaoOpcao, container);
        _instancias.Add(instancia);

        var controlador = instancia.GetComponent<TendaOpcaoBotaoController>();
        if (controlador != null)
        {
            controlador.Configurar(texto, onClick, interativo);
            return;
        }

        var fallbackBotao = instancia.GetComponent<UnityEngine.UI.Button>();
        if (fallbackBotao != null)
        {
            fallbackBotao.interactable = interativo;
            fallbackBotao.onClick.RemoveAllListeners();
            if (onClick != null)
                fallbackBotao.onClick.AddListener(() => onClick.Invoke());
        }

        var textoTmp = instancia.GetComponentInChildren<TMP_Text>();
        if (textoTmp != null)
            textoTmp.text = texto;
    }

    private void LimparInstancias()
    {
        for (int i = 0; i < _instancias.Count; i++)
        {
            if (_instancias[i] != null)
                Destroy(_instancias[i]);
        }
        _instancias.Clear();
    }

    private string ObterNomeReliquia(string idReliquia)
    {
        var def = reliquiaDatabase != null ? reliquiaDatabase.ObterPorId(idReliquia) : null;
        return def != null && !string.IsNullOrEmpty(def.nomeExibicao) ? def.nomeExibicao : idReliquia;
    }

    private string ObterNomeHeroi(string idHeroi)
    {
        var def = heroiDatabase != null ? heroiDatabase.ObterPorId(idHeroi) : null;
        return def != null && !string.IsNullOrEmpty(def.nomeExibicao) ? def.nomeExibicao : idHeroi;
    }
}
