using UnityEngine;

/// <summary>
/// Aplica a Safe Area do dispositivo a um RectTransform do Canvas.
///
/// A Safe Area evita que elementos de HUD fiquem sob o notch (iPhone),
/// a home bar (Android/iOS) ou os cantos arredondados do display.
///
/// Como usar:
///   1. Crie um GameObject filho direto do Canvas chamado "SafeAreaContainer".
///   2. Adicione este script a ele.
///   3. Mova todos os elementos de HUD para dentro deste container.
///   O container sera redimensionado para a area segura do dispositivo.
///
/// Em plataformas sem safe area (desktop, WebGL), o container ocupa
/// 100% da tela sem qualquer recorte.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaAplicador : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Configuracao")]
    [Tooltip("Se verdadeiro, simula uma safe area customizada no Editor para testes.")]
    [SerializeField] private bool simularSafeAreaNoEditor = false;

    [Tooltip("Safe area simulada (0 a 1 de cada lado). Ordem: Left, Bottom, Right, Top.")]
    [SerializeField] private Vector4 safeAreaSimulada = new Vector4(0.05f, 0.08f, 0.95f, 0.92f);
#endif

    [Tooltip("Se verdadeiro, reaaplica sempre que a resolucao mudar (ex.: rotacao de tela).")]
    [SerializeField] private bool atualizarDinamicamente = true;

    private RectTransform _rt;
    private Rect          _ultimaSafeArea = Rect.zero;
    private Vector2Int    _ultimaResolucao;

    // ─── Ciclo de vida ───────────────────────────────────────────────────────
    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Aplicar();
        if (atualizarDinamicamente)
            UIResponsividadeManager.OnLayoutAtualizado += Aplicar;
    }

    private void OnDestroy()
    {
        UIResponsividadeManager.OnLayoutAtualizado -= Aplicar;
    }

    private void Update()
    {
        if (!atualizarDinamicamente) return;

        // Deteccao adicional para rotacao de tela em mobile
        Rect safeAtual = ObterSafeArea();
        if (safeAtual != _ultimaSafeArea)
            Aplicar();
    }

    // ─── Logica ──────────────────────────────────────────────────────────────

    public void Aplicar()
    {
        if (_rt == null) return;

        Rect safe = ObterSafeArea();
        _ultimaSafeArea = safe;

        // Converte pixels para coordenadas normalizadas (0-1)
        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rt.anchorMin        = anchorMin;
        _rt.anchorMax        = anchorMax;
        _rt.offsetMin        = Vector2.zero;
        _rt.offsetMax        = Vector2.zero;
    }

    private Rect ObterSafeArea()
    {
#if UNITY_EDITOR
        if (simularSafeAreaNoEditor)
        {
            return new Rect(
                safeAreaSimulada.x * Screen.width,
                safeAreaSimulada.y * Screen.height,
                (safeAreaSimulada.z - safeAreaSimulada.x) * Screen.width,
                (safeAreaSimulada.w - safeAreaSimulada.y) * Screen.height
            );
        }
#endif
        return Screen.safeArea;
    }

#if UNITY_EDITOR
    // ─── Gizmos de editor ────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (_rt == null || !simularSafeAreaNoEditor) return;

        // Visualiza a safe area simulada no editor
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Apenas uma indicacao visual: nao representa pixels exatos no editor
        Gizmos.DrawWireCube(transform.position, new Vector3(
            _rt.rect.width  * (safeAreaSimulada.z - safeAreaSimulada.x),
            _rt.rect.height * (safeAreaSimulada.w - safeAreaSimulada.y),
            0f
        ));
    }
#endif
}
