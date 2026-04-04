using UnityEngine;

/// <summary>
/// Animação por velocidade 2D: parado, horizontal (Leste + flip ou Oeste dedicado), Norte (Y+) e Sul (Y-).
/// Deixe arrays vazios no prefab até ter sprites; direções sem frames usam fallback horizontal se existir.
/// </summary>
[DisallowMultipleComponent]
public class AnimacaoSpritePorVelocidade2D : MonoBehaviour
{
    private enum ModoAnim
    {
        Parado,
        Horizontal,
        Norte,
        Sul
    }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rigidbody2DRef;

    [Header("Sprites — parado")]
    [SerializeField] private Sprite spriteParado;

    [Header("Sprites — horizontal (olhando à direita / leste)")]
    [Tooltip("Frames de corrida com personagem de frente para a direita. Esquerda: flip X ou use Oeste.")]
    [SerializeField] private Sprite[] spritesAndar;

    [Tooltip("Opcional: corrida para oeste sem espelhar. Se vazio, usa Leste + Flip X.")]
    [SerializeField] private Sprite[] spritesAndarOeste;

    [Header("Sprites — vertical (top-down: Y+ = Norte, Y- = Sul)")]
    [SerializeField] private Sprite[] spritesAndarNorte;
    [SerializeField] private Sprite[] spritesAndarSul;

    [Header("Direção")]
    [Tooltip("Se desligado, só usa horizontal + flip (comportamento simples).")]
    [SerializeField] private bool usarDirecoesNorteSul = true;

    [Tooltip("Norte/Sul quando |Vy| >= |Vx| * este valor. 1 = empate favorece vertical; menor (ex: 0,7) exige menos componente Y.")]
    [SerializeField] private float multiplicadorDominanciaVertical = 1f;

    [Header("Comportamento")]
    [SerializeField] private float velocidadeMinima = 0.15f;
    [SerializeField] private float limiteDirecaoX = 0.05f;
    [SerializeField] private float tempoPorFrame = 0.12f;

    private ModoAnim modoAtual = ModoAnim.Parado;
    private int indiceFrame;
    private float acumuladorTempo;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (rigidbody2DRef == null)
            rigidbody2DRef = GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null || rigidbody2DRef == null)
            return;

        if (!TemAlgumFrameDeCorrida())
            return;

        Vector2 v = rigidbody2DRef.linearVelocity;
        float speed = v.magnitude;

        if (speed < velocidadeMinima)
        {
            if (spriteParado != null)
                spriteRenderer.sprite = spriteParado;
            if (modoAtual != ModoAnim.Parado)
            {
                modoAtual = ModoAnim.Parado;
                indiceFrame = 0;
                acumuladorTempo = 0f;
            }
            return;
        }

        ModoAnim desejado = CalcularModo(v);
        if (desejado != modoAtual)
        {
            modoAtual = desejado;
            indiceFrame = 0;
            acumuladorTempo = 0f;
        }

        bool usarOesteSemEspelho = desejado == ModoAnim.Horizontal
            && v.x < -limiteDirecaoX
            && spritesAndarOeste != null
            && spritesAndarOeste.Length > 0;

        if (usarOesteSemEspelho)
            spriteRenderer.flipX = false;
        else if (desejado == ModoAnim.Horizontal)
        {
            if (v.x > limiteDirecaoX)
                spriteRenderer.flipX = false;
            else if (v.x < -limiteDirecaoX)
                spriteRenderer.flipX = true;
        }
        else
            spriteRenderer.flipX = false;

        Sprite[] clip = ObterClipAndando(desejado, v, usarOesteSemEspelho);
        if (clip == null || clip.Length == 0)
            return;

        acumuladorTempo += Time.deltaTime;
        while (acumuladorTempo >= tempoPorFrame && tempoPorFrame > 0f)
        {
            acumuladorTempo -= tempoPorFrame;
            indiceFrame = (indiceFrame + 1) % clip.Length;
        }

        Sprite frame = clip[indiceFrame];
        if (frame != null)
            spriteRenderer.sprite = frame;
    }

    private bool TemAlgumFrameDeCorrida()
    {
        return TemArray(spritesAndar) || TemArray(spritesAndarOeste) || TemArray(spritesAndarNorte) || TemArray(spritesAndarSul);
    }

    private static bool TemArray(Sprite[] a) => a != null && a.Length > 0;

    private ModoAnim CalcularModo(Vector2 v)
    {
        if (!usarDirecoesNorteSul)
            return ModoAnim.Horizontal;

        float ax = Mathf.Abs(v.x);
        float ay = Mathf.Abs(v.y);
        if (ay >= ax * Mathf.Max(0.01f, multiplicadorDominanciaVertical))
            return v.y >= 0f ? ModoAnim.Norte : ModoAnim.Sul;

        return ModoAnim.Horizontal;
    }

    private Sprite[] ObterClipAndando(ModoAnim modo, Vector2 v, bool usarOesteSemEspelho)
    {
        switch (modo)
        {
            case ModoAnim.Norte:
                if (TemArray(spritesAndarNorte))
                    return spritesAndarNorte;
                break;
            case ModoAnim.Sul:
                if (TemArray(spritesAndarSul))
                    return spritesAndarSul;
                break;
        }

        if (usarOesteSemEspelho)
            return spritesAndarOeste;

        if (TemArray(spritesAndar))
            return spritesAndar;

        if (TemArray(spritesAndarOeste))
            return spritesAndarOeste;

        if (TemArray(spritesAndarNorte))
            return spritesAndarNorte;

        if (TemArray(spritesAndarSul))
            return spritesAndarSul;

        return null;
    }
}
