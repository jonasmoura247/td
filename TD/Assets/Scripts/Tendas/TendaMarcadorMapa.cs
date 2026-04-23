using UnityEngine;

/// <summary>
/// Marcador clicável da tenda no mapa mundo.
/// Pode ser um quadrado simples com SpriteRenderer + Collider2D.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TendaMarcadorMapa : MonoBehaviour
{
    [SerializeField] private PainelTendaController painelTenda;

    private EventoGerado _evento;
    private bool _foiConfigurado;

    private void Awake()
    {
        Debug.LogFormat("[TENDA DEBUG] Marcador '{0}' Awake. collider={1}", name, GetComponent<Collider2D>() != null);
    }

    public void Configurar(PainelTendaController painel, EventoGerado evento)
    {
        painelTenda = painel;
        _evento = evento;
        _foiConfigurado = true;
        Debug.LogFormat(
            "[TENDA DEBUG] Configurar marcador '{0}'. painel={1} eventoNull={2} temTenda={3} tipo={4}",
            name,
            painelTenda != null ? painelTenda.name : "null",
            _evento == null,
            _evento != null && _evento.temTenda,
            _evento != null ? ((TipoTenda)_evento.tipoTendaInt).ToString() : "n/a");
        AtualizarVisibilidade();
    }

    private void Start()
    {
        if (!_foiConfigurado)
        {
            Debug.LogWarningFormat("[TENDA DEBUG] Marcador '{0}' existe na cena mas não foi configurado pelo bootstrap. Ocultando objeto.", name);
            gameObject.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        Debug.LogFormat(
            "[TENDA DEBUG] Clique no marcador '{0}'. eventoNull={1} painelNull={2} temTenda={3} activeSelf={4}",
            name,
            _evento == null,
            painelTenda == null,
            _evento != null && _evento.temTenda,
            gameObject.activeSelf);

        if (_evento == null || painelTenda == null || !_evento.temTenda)
        {
            Debug.LogWarningFormat("[TENDA DEBUG] Clique ignorado em '{0}'.", name);
            return;
        }

        painelTenda.MostrarTenda(this, _evento);
    }

    public void AtualizarVisibilidade()
    {
        bool ativo = _evento != null && _evento.temTenda && !SistemaTendasManager.EventoFoiResolvido(_evento);
        Debug.LogFormat(
            "[TENDA DEBUG] AtualizarVisibilidade marcador '{0}'. ativo={1} eventoNull={2} temTenda={3} resolvido={4}",
            name,
            ativo,
            _evento == null,
            _evento != null && _evento.temTenda,
            _evento != null && SistemaTendasManager.EventoFoiResolvido(_evento));
        gameObject.SetActive(ativo);
    }
}
