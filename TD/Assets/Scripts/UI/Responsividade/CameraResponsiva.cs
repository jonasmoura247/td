using UnityEngine;

/// <summary>
/// Ajusta o orthographicSize da Camera principal para que o mapa sempre caiba
/// na tela independente do aspect ratio e da resolucao.
///
/// Como usar:
///   1. Adicione este script ao GameObject da Camera principal do Mapa_1.
///   2. Configure larguraMapaMundial e alturaMapaMundial com as dimensoes do seu mapa em unidades do mundo.
///   3. O script calcula automaticamente o orthographicSize ideal ao iniciar e quando a resolucao muda.
///
/// Comportamento por aspect ratio:
///   - Widescreen (16:9): orthographicSize baseado na altura do mapa.
///   - Ultrawide (21:9+): adiciona margem horizontal; orthographicSize reduzido proporcionalmente.
///   - Portrait/Tablet:   orthographicSize baseado na largura, garantindo que o mapa caiba horizontalmente.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraResponsiva : MonoBehaviour
{
    [Header("Dimensoes do mapa em unidades do mundo")]
    [Tooltip("Largura total do mapa jogavel em unidades Unity (world units).")]
    [SerializeField] private float larguraMapaMundial = 20f;

    [Tooltip("Altura total do mapa jogavel em unidades Unity (world units).")]
    [SerializeField] private float alturaMapaMundial  = 12f;

    [Header("Margem de seguranca")]
    [Tooltip("Percentual de margem adicionado em volta do mapa (0.05 = 5%). Evita que elementos fiquem colados na borda.")]
    [Range(0f, 0.3f)]
    [SerializeField] private float margemRelativa = 0.05f;

    [Header("Limites de orthographicSize")]
    [SerializeField] private float tamanhoMinimo = 3f;
    [SerializeField] private float tamanhoMaximo = 30f;

    [Header("Debug")]
    [SerializeField] private bool mostrarGizmo = true;

    private Camera _camera;
    private int    _larguraAnterior;
    private int    _alturaAnterior;

    // ─── Ciclo de vida ───────────────────────────────────────────────────────
    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        _larguraAnterior = Screen.width;
        _alturaAnterior  = Screen.height;
        AjustarCamera();

        // Registra no manager centralizado se existir
        UIResponsividadeManager.OnLayoutAtualizado += AjustarCamera;
    }

    private void OnDestroy()
    {
        UIResponsividadeManager.OnLayoutAtualizado -= AjustarCamera;
    }

    private void Update()
    {
        // Fallback: detecta mudanca de resolucao sem depender do manager
        if (Screen.width != _larguraAnterior || Screen.height != _alturaAnterior)
        {
            _larguraAnterior = Screen.width;
            _alturaAnterior  = Screen.height;
            AjustarCamera();
        }
    }

    // ─── Logica de ajuste ────────────────────────────────────────────────────

    /// <summary>
    /// Calcula e aplica o orthographicSize correto para o aspect ratio atual.
    /// </summary>
    public void AjustarCamera()
    {
        if (_camera == null || !_camera.orthographic)
            return;

        float aspectAtual = (float)Screen.width / Mathf.Max(Screen.height, 1);
        float novoTamanho = CalcularOrthographicSize(aspectAtual);

        novoTamanho = Mathf.Clamp(novoTamanho, tamanhoMinimo, tamanhoMaximo);
        _camera.orthographicSize = novoTamanho;
    }

    /// <summary>
    /// Calcula o orthographicSize necessario para que o mapa inteiro caiba na tela
    /// com o aspect ratio informado, respeitando a margem configurada.
    /// </summary>
    private float CalcularOrthographicSize(float aspect)
    {
        float larguraComMargem = larguraMapaMundial  * (1f + margemRelativa);
        float alturaComMargem  = alturaMapaMundial   * (1f + margemRelativa);

        // orthographicSize = metade da altura visivel
        // Para caber a altura do mapa: size = alturaComMargem / 2
        float sizePelaAltura   = alturaComMargem / 2f;

        // Para caber a largura do mapa: size = (larguraComMargem / 2) / aspect
        float sizePelaLargura  = (larguraComMargem / 2f) / Mathf.Max(aspect, 0.01f);

        // Usa o maior dos dois para garantir que o mapa sempre caiba inteiro
        return Mathf.Max(sizePelaAltura, sizePelaLargura);
    }

    // ─── API publica ─────────────────────────────────────────────────────────

    /// <summary>
    /// Atualiza as dimensoes do mapa em tempo de execucao e recalcula a camera.
    /// Util se o mapa for gerado proceduralmente.
    /// </summary>
    public void DefinirDimensoesMapa(float largura, float altura)
    {
        larguraMapaMundial = largura;
        alturaMapaMundial  = altura;
        AjustarCamera();
    }

    // ─── Gizmos ─────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (!mostrarGizmo) return;

        // Desenha os limites do mapa no editor
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
        Gizmos.DrawWireCube(transform.position, new Vector3(larguraMapaMundial, alturaMapaMundial, 0f));

        // Dimensoes com margem
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawWireCube(transform.position,
            new Vector3(larguraMapaMundial  * (1f + margemRelativa),
                        alturaMapaMundial   * (1f + margemRelativa), 0f));
    }
}
