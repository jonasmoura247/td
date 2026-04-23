using UnityEngine;

/// <summary>
/// Exibe um círculo visual mostrando o alcance de ataque de uma unidade ranged.
/// Adicione este componente no mesmo GameObject que possui SistemaIAUnidade.
/// Chame MostrarAlcance() ao selecionar a unidade e EsconderAlcance() ao deselecionar.
/// O círculo segue a unidade automaticamente pois usa world-space no LineRenderer.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class VisualizadorAlcanceUnidade : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private int segmentos = 48;
    [SerializeField] private float larguraLinha = 0.06f;
    [SerializeField] private Color corAlcance = new Color(0.2f, 0.8f, 1f, 0.55f);

    [Header("Alcance Manual (0 = lê do SistemaIAUnidade)")]
    [SerializeField] private float alcanceFixo = 0f;

    private LineRenderer _lr;
    private SistemaIAUnidade _sistemaIA;
    private bool _visivel;

    public bool Visivel => _visivel;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _sistemaIA = GetComponent<SistemaIAUnidade>();
        ConfigurarLineRenderer();
        _lr.enabled = false;
    }

    private void LateUpdate()
    {
        if (!_visivel) return;
        float raio = ObterAlcance();
        if (raio > 0f) DesenharCirculo(raio);
    }

    // ─── API pública ──────────────────────────────────────────────────────

    public void MostrarAlcance()
    {
        _visivel = true;
        _lr.enabled = true;
    }

    public void EsconderAlcance()
    {
        _visivel = false;
        _lr.enabled = false;
    }

    /// <summary>Alterna visibilidade — útil para binding direto de botão de UI.</summary>
    public void AlternarAlcance()
    {
        if (_visivel) EsconderAlcance(); else MostrarAlcance();
    }

    // ─── Internos ─────────────────────────────────────────────────────────

    private float ObterAlcance()
    {
        if (alcanceFixo > 0f) return alcanceFixo;
        return _sistemaIA != null ? _sistemaIA.alcanceAtaque : 0f;
    }

    private void ConfigurarLineRenderer()
    {
        _lr.useWorldSpace = true;
        _lr.loop = true;
        _lr.positionCount = segmentos;
        _lr.startWidth = larguraLinha;
        _lr.endWidth = larguraLinha;
        _lr.startColor = corAlcance;
        _lr.endColor = corAlcance;

        var shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            _lr.material = new Material(shader);
            _lr.material.color = corAlcance;
        }
    }

    private void DesenharCirculo(float raio)
    {
        Vector3 centro = transform.position;
        float passo = 2f * Mathf.PI / segmentos;
        for (int i = 0; i < segmentos; i++)
        {
            float ang = i * passo;
            _lr.SetPosition(i, new Vector3(
                centro.x + raio * Mathf.Cos(ang),
                centro.y + raio * Mathf.Sin(ang),
                centro.z));
        }
    }
}
