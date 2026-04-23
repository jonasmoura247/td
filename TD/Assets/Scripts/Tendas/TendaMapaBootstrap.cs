using UnityEngine;

/// <summary>
/// Procura a proxima fase disponivel do mapa e cria um marcador de tenda ao lado dela.
/// Tambem pode forcar uma tenda de teste para facilitar a validacao no Unity.
/// </summary>
public class TendaMapaBootstrap : MonoBehaviour
{
    [SerializeField] private PainelTendaController painelTenda;
    [SerializeField] private ReliquiaDatabase reliquiaDatabase;
    [SerializeField] private HeroiDatabase heroiDatabase;
    [SerializeField] private TendaMarcadorMapa prefabMarcadorTenda;
    [SerializeField] private Vector3 offsetMarcador = new Vector3(1.5f, 0.75f, 0f);

    [Header("Teste")]
    [SerializeField] private bool forcarTendaEmTeste = true;  // manter true ate ter mapa mundo de producao
    [SerializeField] private TipoTenda tipoTendaForcada = TipoTenda.Mista;

    private TendaMarcadorMapa _instanciaMarcador;

    private void Start()
    {
        Debug.LogFormat(
            "[TENDA DEBUG] Bootstrap Start. painel={0} reliquiaDb={1} heroiDb={2} prefabMarcador={3} sistemaTendas={4} forcarTenda={5} tipoForcado={6}",
            painelTenda != null ? painelTenda.name : "null",
            reliquiaDatabase != null ? reliquiaDatabase.name : "null",
            heroiDatabase != null ? heroiDatabase.name : "null",
            prefabMarcadorTenda != null ? prefabMarcadorTenda.name : "null",
            SistemaTendasManager.Instance != null ? "ok" : "null",
            forcarTendaEmTeste,
            tipoTendaForcada);
        RecriarMarcador();
    }

    public void RecriarMarcador()
    {
        Debug.Log("[TENDA DEBUG] RecriarMarcador chamado.");

        if (prefabMarcadorTenda == null || painelTenda == null || SistemaTendasManager.Instance == null)
        {
            Debug.LogWarningFormat(
                "[TENDA DEBUG] RecriarMarcador abortado. prefabMarcador={0} painel={1} sistemaTendas={2}",
                prefabMarcadorTenda != null,
                painelTenda != null,
                SistemaTendasManager.Instance != null);
            return;
        }

        if (_instanciaMarcador != null)
        {
            Debug.LogFormat("[TENDA DEBUG] Destruindo marcador antigo '{0}'.", _instanciaMarcador.name);
            Destroy(_instanciaMarcador.gameObject);
        }

        var bandeira = EncontrarProximaFaseDisponivel();
        if (bandeira == null)
        {
            Debug.LogWarning("[TENDA DEBUG] Nenhuma bandeira disponivel encontrada.");
            return;
        }

        Debug.LogFormat(
            "[TENDA DEBUG] Proxima bandeira disponivel: {0} mundo={1} indice={2} pos={3}",
            bandeira.FaseId,
            bandeira.MundoId,
            bandeira.IndiceFase,
            bandeira.transform.position);

        var evento = SistemaTendasManager.Instance.PrepararTendaPerfilAtivo(
            bandeira.MundoId,
            bandeira.IndiceFase,
            reliquiaDatabase,
            heroiDatabase);

        DebugEvento("Evento preparado", evento);

        if (forcarTendaEmTeste && (evento == null || !evento.temTenda || SistemaTendasManager.EventoFoiResolvido(evento)))
        {
            Debug.LogWarningFormat(
                "[TENDA DEBUG] Forcando tenda de teste para mundo={0} fase={1}. tipo={2}",
                bandeira.MundoId,
                bandeira.IndiceFase,
                tipoTendaForcada);

            evento = SistemaTendasManager.Instance.ForcarTendaPerfilAtivo(
                bandeira.MundoId,
                bandeira.IndiceFase,
                tipoTendaForcada,
                reliquiaDatabase,
                heroiDatabase);

            DebugEvento("Evento apos forcamento", evento);
        }

        if (evento == null || !evento.temTenda || SistemaTendasManager.EventoFoiResolvido(evento))
        {
            Debug.LogWarning("[TENDA DEBUG] Marcador nao sera criado para esta fase.");
            return;
        }

        _instanciaMarcador = Instantiate(prefabMarcadorTenda, bandeira.transform.position + offsetMarcador, Quaternion.identity, transform);
        _instanciaMarcador.name = $"Tenda_{bandeira.FaseId}";
        Debug.LogFormat("[TENDA DEBUG] Marcador instanciado: {0} pos={1}", _instanciaMarcador.name, _instanciaMarcador.transform.position);
        _instanciaMarcador.Configurar(painelTenda, evento);
    }

    private static void DebugEvento(string prefixo, EventoGerado evento)
    {
        Debug.LogFormat(
            "[TENDA DEBUG] {0}. null={1} temTenda={2} resolvido={3} tipo={4}",
            prefixo,
            evento == null,
            evento != null && evento.temTenda,
            evento != null && SistemaTendasManager.EventoFoiResolvido(evento),
            evento != null ? ((TipoTenda)evento.tipoTendaInt).ToString() : "n/a");
    }

    private static BandeiraFaseController EncontrarProximaFaseDisponivel()
    {
        var bandeiras = FindObjectsByType<BandeiraFaseController>(FindObjectsSortMode.None);
        Debug.LogFormat("[TENDA DEBUG] EncontrarProximaFaseDisponivel: {0} bandeiras encontradas.", bandeiras.Length);
        for (int i = 0; i < bandeiras.Length; i++)
        {
            if (bandeiras[i] != null)
            {
                Debug.LogFormat(
                    "[TENDA DEBUG] Bandeira candidata: {0} estado={1} mundo={2} indice={3}",
                    bandeiras[i].FaseId,
                    bandeiras[i].ObterEstado(),
                    bandeiras[i].MundoId,
                    bandeiras[i].IndiceFase);
            }

            if (bandeiras[i] != null && bandeiras[i].ObterEstado() == EstadoBandeira.Disponivel)
                return bandeiras[i];
        }

        return null;
    }
}
