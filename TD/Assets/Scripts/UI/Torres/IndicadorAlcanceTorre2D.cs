using UnityEngine;

/// <summary>
/// Círculo de alcance em mundo (LineRenderer), reutilizável para preview de construção.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class IndicadorAlcanceTorre2D : MonoBehaviour
{
    [SerializeField] private int segmentos = 64;
    [SerializeField] private float larguraLinha = 0.07f;
    [SerializeField] private Color cor = new Color(0.25f, 0.85f, 0.35f, 0.65f);
    [SerializeField] private int sortingOrder = 3;

    private LineRenderer _linha;

    private void Awake()
    {
        _linha = GetComponent<LineRenderer>();
        _linha.loop = true;
        _linha.useWorldSpace = false;
        _linha.widthMultiplier = larguraLinha;
        _linha.positionCount = Mathf.Max(8, segmentos);
        _linha.startColor = cor;
        _linha.endColor = cor;
        _linha.sortingOrder = sortingOrder;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
            _linha.material = new Material(shader);
    }

    /// <summary>Raio em unidades de mundo (plano XY local ao transform pai).</summary>
    public void DefinirRaio(float raio)
    {
        if (_linha == null)
            Awake();

        raio = Mathf.Max(0.01f, raio);
        int n = _linha.positionCount;
        for (int i = 0; i < n; i++)
        {
            float t = (i / (float)n) * Mathf.PI * 2f;
            _linha.SetPosition(i, new Vector3(Mathf.Cos(t) * raio, Mathf.Sin(t) * raio, 0f));
        }
    }
}
