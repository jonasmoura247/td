using UnityEngine;

/// <summary>
/// Projétil de fireball que se move em direção ao alvo e aplica dano a cada inimigo que acertar.
/// </summary>
public class FireballProjetil : MonoBehaviour
{
    [SerializeField] private float velocidade = 12f;
    private const int DanoFireballPadrao = 10;

    private Transform alvo;
    private string origemNome;
    private int danoAplicar = DanoFireballPadrao;

    public void Inicializar(Transform novoAlvo, int novoDano, string nomeOrigem = null)
    {
        alvo = novoAlvo;
        origemNome = nomeOrigem;
        danoAplicar = novoDano > 0 ? novoDano : DanoFireballPadrao;
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
            IDanificavel danificavel = alvo.GetComponentInParent<IDanificavel>();

            if (danificavel != null && danificavel.EstaVivo)
            {
                ServicoAplicacaoDano.Aplicar(
                    danificavel,
                    alvo.position,
                    danoAplicar,
                    PedidoDanoFlutuante.Criar(danoAplicar, TipoDanoCombate.Fogo),
                    dispararProcsRunaBestiario: true);
            }

            Destroy(gameObject);
        }
    }
}
