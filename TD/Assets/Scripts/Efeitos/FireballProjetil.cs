using UnityEngine;

/// <summary>
/// Projétil de fireball que se move em direção ao alvo e aplica 10 de dano a cada inimigo que acertar.
/// </summary>
public class FireballProjetil : MonoBehaviour
{
    [SerializeField] private float velocidade = 12f;
    private const int DanoFireball = 10;

    private Transform alvo;
    private string origemNome;

    public void Inicializar(Transform novoAlvo, int novoDano, string nomeOrigem = null)
    {
        alvo = novoAlvo;
        origemNome = nomeOrigem;
    }

    private void Update()
    {
        if (alvo == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direcao = (alvo.position - transform.position).normalized;

        transform.position += (Vector3)(direcao * velocidade * Time.deltaTime);

        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo - 90f);

        float distancia = Vector2.Distance(transform.position, alvo.position);

        if (distancia <= 0.3f)
        {
            IDanificavel danificavel = alvo.GetComponent<IDanificavel>();

            if (danificavel != null)
            {
                danificavel.ReceberDano(DanoFireball);
            }

            Destroy(gameObject);
        }
    }
}
