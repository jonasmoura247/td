using UnityEngine;

/// <summary>
/// Regenera vida passivamente ao longo do tempo.
/// Adicione ao prefab do mob junto com UnidadeController.
/// </summary>
[RequireComponent(typeof(UnidadeController))]
public class RegeneracaoVida : MonoBehaviour
{
    [Tooltip("Pontos de vida regenerados por segundo.")]
    public float vidaPorSegundo = 5f;

    private UnidadeController _unidade;
    private float _acumulador;

    private void Awake()
    {
        _unidade = GetComponent<UnidadeController>();
    }

    private void Update()
    {
        if (_unidade == null || !_unidade.EstaVivo) return;
        if (_unidade.GetVidaAtual() >= _unidade.vidaMaxima) return;

        _acumulador += Time.deltaTime;

        int cura = RegeneracaoVidaLogica.CalcularCura(vidaPorSegundo, _acumulador);
        if (cura > 0)
        {
            _unidade.CurarVida(cura);
            _acumulador = 0f;
        }
    }
}
