using UnityEngine;

public class MovNpcGoblin : MonoBehaviour
{
    public float speed = 3f;

    private Transform[] pontos;
    private int pontoAtual = 0;
    private Rigidbody2D rb;

    public void DefinirRota(Transform rota)
    {
        int quantidade = rota.childCount;
        pontos = new Transform[quantidade];

        for (int i = 0; i < quantidade; i++)
        {
            pontos[i] = rota.GetChild(i);
        }

        rb = GetComponent<Rigidbody2D>();
        rb.position = pontos[0].position;
    }

    void FixedUpdate()
    {
        if (pontos == null || pontoAtual >= pontos.Length)
            return;

        Transform destino = pontos[pontoAtual];

        rb.MovePosition(Vector2.MoveTowards(
            rb.position,
            destino.position,
            speed * Time.fixedDeltaTime
        ));

        if (Vector2.Distance(rb.position, destino.position) < 0.05f)
        {
            pontoAtual++;
        }
    }
}