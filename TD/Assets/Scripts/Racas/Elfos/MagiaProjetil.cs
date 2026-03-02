using UnityEngine;

public class MagiaProjetil : MonoBehaviour
{
    public float velocidade = 8f;

    private Transform alvo;
    private int dano;

    public void Inicializar(Transform novoAlvo, int novoDano)
    {
        alvo = novoAlvo;
        dano = novoDano;
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

        if (distancia <= 0.1f)
        {
            IDanificavel danificavel = alvo.GetComponent<IDanificavel>();

            if (danificavel != null)
            {
                danificavel.ReceberDano(dano);
            }

            Destroy(gameObject);
        }
    }
}