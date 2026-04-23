using UnityEngine;

/// <summary>
/// Relíquia "Que espécie de verme são vocês?": indica a direção da próxima wave.
/// Assina eventos do WaveManager e controla o IndicadorDirecaoWaveUI.
/// Só está ativa se a relíquia estiver equipada (instanciada pelo ReliquiasPartidaOrchestrator).
/// </summary>
public class EfeitoBussolaVerme : MonoBehaviour, IReliquiaEfeito
{
    [SerializeField] private IndicadorDirecaoWaveUI prefabIndicador;

    private IndicadorDirecaoWaveUI _indicador;

    public void Iniciar(ReliquiaContext contexto)
    {
        if (prefabIndicador == null)
        {
            Debug.LogWarning("[BussolaVerme] prefabIndicador nao atribuido. O indicador nao sera exibido.");
            return;
        }

        if (contexto?.CanvasOverlay == null)
        {
            Debug.LogWarning("[BussolaVerme] CanvasOverlay nao disponivel no contexto. O indicador nao sera exibido.");
            return;
        }

        _indicador = Instantiate(prefabIndicador, contexto.CanvasOverlay.transform);
        _indicador.Esconder();

        WaveManager.OnProximaWaveDefinida += AoProximaWaveDefinida;
        WaveManager.OnWaveIniciada += AoWaveIniciada;
    }

    public void Encerrar()
    {
        WaveManager.OnProximaWaveDefinida -= AoProximaWaveDefinida;
        WaveManager.OnWaveIniciada -= AoWaveIniciada;

        if (_indicador != null)
        {
            Destroy(_indicador.gameObject);
            _indicador = null;
        }
    }

    private void OnDestroy() => Encerrar();

    private void AoProximaWaveDefinida(LadoSpawn lado)
    {
        if (_indicador != null)
            _indicador.Mostrar(lado);
    }

    private void AoWaveIniciada()
    {
        if (_indicador != null)
            _indicador.Esconder();
    }
}
