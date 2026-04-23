using UnityEngine;

[DisallowMultipleComponent]
public class AnimacaoSpritePorVelocidade2D : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rigidbody2DRef;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteParado;
    [Tooltip("Frames de corrida olhando para a direita. Esquerda usa flip X automático.")]
    [SerializeField] private Sprite[] spritesAndar;

    [Header("Comportamento")]
    [SerializeField] private float velocidadeMinima = 0.2f;
    [SerializeField] private float limiteDirecaoX = 0.08f;
    [SerializeField] private float tempoPorFrame = 0.1f;

    private bool parado = true;
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

        float speed = rigidbody2DRef.linearVelocity.magnitude;

        if (speed < velocidadeMinima)
        {
            if (!parado)
            {
                parado = true;
                indiceFrame = 0;
                acumuladorTempo = 0f;
            }
            if (spriteParado != null)
                spriteRenderer.sprite = spriteParado;
            return;
        }

        if (parado)
        {
            parado = false;
            indiceFrame = 0;
            acumuladorTempo = 0f;
        }

        float vx = rigidbody2DRef.linearVelocity.x;
        if (vx > limiteDirecaoX)
            spriteRenderer.flipX = false;
        else if (vx < -limiteDirecaoX)
            spriteRenderer.flipX = true;

        if (spritesAndar == null || spritesAndar.Length == 0)
            return;

        acumuladorTempo += Time.deltaTime;
        while (acumuladorTempo >= tempoPorFrame && tempoPorFrame > 0f)
        {
            acumuladorTempo -= tempoPorFrame;
            indiceFrame = (indiceFrame + 1) % spritesAndar.Length;
        }

        Sprite frame = spritesAndar[indiceFrame];
        if (frame != null)
            spriteRenderer.sprite = frame;
    }
}
