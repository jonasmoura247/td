using UnityEngine;

public class LancaProjetil : MonoBehaviour
{
    public float velocidade = 8f;

    private Transform alvo;
    private int dano;
    private string origemNome;

    public void Inicializar(Transform novoAlvo, int novoDano, string nomeOrigem = null)
    {
        alvo = novoAlvo;
        dano = novoDano;
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

        if (distancia <= 0.1f)
        {
            IDanificavel danificavel = alvo.GetComponent<IDanificavel>();

            if (danificavel != null)
            {
                danificavel.ReceberDano(dano);
                string origem = string.IsNullOrEmpty(origemNome) ? "Projetil" : origemNome;
                Debug.Log($"[Castelo] {origem} (Lanceiro) causou {dano} de dano no castelo.");
            }

            Destroy(gameObject);
        }
    }
}