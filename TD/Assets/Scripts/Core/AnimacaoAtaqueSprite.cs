using UnityEngine;

/// <summary>
/// Toca sprites de ataque sincronizados com o intervaloAtaque da unidade.
/// tempoPorFrame = intervaloAtaque / totalFrames, então a animação completa
/// exatamente em um ciclo de ataque independente da velocidade da criatura.
/// Usa LateUpdate com DefaultExecutionOrder(10) para sobrescrever
/// AnimacaoSpritePorVelocidade2D (order 0) na mesma frame.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(10)]
public class AnimacaoAtaqueSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] spritesAtaque;

    private UnidadeController _unidade;
    private float _tempoAnimacao = -1f;
    private float _tempoPorFrame;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        _unidade = GetComponent<UnidadeController>();
    }

    private void Start()
    {
        RecalcularTempoPorFrame();
    }

    private void OnEnable()
    {
        if (_unidade != null)
            _unidade.OnAtaqueExecutado += AoAtacar;
    }

    private void OnDisable()
    {
        if (_unidade != null)
            _unidade.OnAtaqueExecutado -= AoAtacar;
        _tempoAnimacao = -1f;
    }

    private void RecalcularTempoPorFrame()
    {
        if (_unidade == null || spritesAtaque == null || spritesAtaque.Length == 0)
        {
            _tempoPorFrame = 0.05f;
            return;
        }
        _tempoPorFrame = _unidade.GetIntervaloAtaque() / spritesAtaque.Length;
    }

    private void AoAtacar()
    {
        if (spritesAtaque == null || spritesAtaque.Length == 0) return;
        RecalcularTempoPorFrame();
        _tempoAnimacao = 0f;
    }

    private void LateUpdate()
    {
        if (_tempoAnimacao < 0f) return;

        _tempoAnimacao += Time.deltaTime;
        int frame = Mathf.FloorToInt(_tempoAnimacao / _tempoPorFrame);

        if (frame >= spritesAtaque.Length)
        {
            _tempoAnimacao = -1f;
            return;
        }

        if (spriteRenderer != null && spritesAtaque[frame] != null)
            spriteRenderer.sprite = spritesAtaque[frame];
    }
}
