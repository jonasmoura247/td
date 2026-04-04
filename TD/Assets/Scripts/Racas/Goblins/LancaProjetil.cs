using UnityEngine;

public class LancaProjetil : MonoBehaviour
{
    public float velocidade = 8f;

    [Tooltip("0 = sem limite. Evita projéteis presos se o alvo fugir ou for destruído de forma anómala.")]
    public float tempoVidaMaximo = 0f;

    [Tooltip("Se ativo, regista no console cada impacto com dano.")]
    public bool registrarImpactoNoConsole = false;

    private Transform alvo;
    private int dano;
    private string origemNome;
    private float tempoCriacao;

    public void Inicializar(Transform novoAlvo, int novoDano, string nomeOrigem = null)
    {
        alvo = novoAlvo;
        dano = novoDano;
        origemNome = nomeOrigem;
        tempoCriacao = Time.time;
    }

    private void Update()
    {
        if (tempoVidaMaximo > 0f && Time.time - tempoCriacao >= tempoVidaMaximo)
        {
            Destroy(gameObject);
            return;
        }

        if (alvo == null)
        {
            Destroy(gameObject);
            return;
        }

        IDanificavel vidaAlvo = alvo.GetComponent<IDanificavel>();
        if (vidaAlvo != null && !vidaAlvo.EstaVivo)
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
            if (vidaAlvo != null && vidaAlvo.EstaVivo)
            {
                vidaAlvo.ReceberDano(dano);
                if (registrarImpactoNoConsole)
                {
                    string origem = string.IsNullOrEmpty(origemNome) ? "Projetil" : origemNome;
                    Debug.Log($"[LancaProjetil] {origem} causou {dano} de dano em {alvo.name}.");
                }
            }

            Destroy(gameObject);
        }
    }
}