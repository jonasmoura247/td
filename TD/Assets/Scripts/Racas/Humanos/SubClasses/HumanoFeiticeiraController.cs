using UnityEngine;

/// <summary>
/// Controller da feiticeira humana. Unidade de combate mago.
/// Fica em repouso sem inimigos; ataca com fireball a cada 5s no inimigo mais próximo dentro do range.
/// </summary>
public class HumanoFeiticeiraController : HumanoController
{
    [Header("Configuração de Ataque")]
    [SerializeField] private float rangeDeAtaque = 30f;
    [SerializeField] private float intervaloAtaque = 5f;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private AudioClip somFireball;

    private float proximoAtaque;

    protected override void Start()
    {
        vidaMaxima = 100;
        danoBase = 1;
        faccao = Faccao.Jogador;
        base.Start();
        if (fireballPrefab == null)
        {
            fireballPrefab = Resources.Load<GameObject>("Fireball");
            if (fireballPrefab == null)
                Debug.LogWarning("[Feiticeira] fireballPrefab não atribuído! Atribua o prefab Fireball no Inspector ou coloque em Assets/Resources/");
        }
        if (somFireball == null)
        {
            somFireball = Resources.Load<AudioClip>("Sons/Humanos/Feiticeiro/fireball");
            if (somFireball == null)
                Debug.LogWarning("[Feiticeira] somFireball não atribuído! Atribua no Inspector ou coloque fireball.mp3 em Assets/Resources/Sons/Humanos/Feiticeiro/");
        }
    }

    private void Update()
    {
        if (!estaVivo) return;

        UnidadeController inimigoProximo = EncontrarInimigoMaisProximo();

        if (inimigoProximo == null)
        {
            movimento?.PararMovimento();
            return;
        }

        float distancia = Vector2.Distance(transform.position, inimigoProximo.transform.position);

        if (distancia <= rangeDeAtaque)
        {
            movimento?.PararMovimento();
            Atacar(inimigoProximo.transform);
        }
        else
        {
            movimento?.ContinuarMovimento();
        }
    }

    private UnidadeController EncontrarInimigoMaisProximo()
    {
        var todasUnidades = FindObjectsByType<UnidadeController>(FindObjectsSortMode.None);
        UnidadeController maisProximo = null;
        float menorDistancia = float.MaxValue;

        foreach (var unidade in todasUnidades)
        {
            if (unidade == null || !unidade.EstaVivo) continue;
            if (unidade.faccao == faccao) continue;

            float dist = Vector2.Distance(transform.position, unidade.transform.position);
            if (dist <= rangeDeAtaque && dist < menorDistancia)
            {
                menorDistancia = dist;
                maisProximo = unidade;
            }
        }

        return maisProximo;
    }

    private void Atacar(Transform alvo)
    {
        if (Time.time < proximoAtaque) return;
        if (fireballPrefab == null || alvo == null) return;

        proximoAtaque = Time.time + intervaloAtaque;

        GameObject fireball = Instantiate(
            fireballPrefab,
            transform.position,
            Quaternion.identity
        );

        FireballProjetil projeto = fireball.GetComponent<FireballProjetil>();
        if (projeto != null)
        {
            projeto.Inicializar(alvo, GetDanoAtaque(), gameObject.name);
        }

        if (somFireball != null)
        {
            AudioSource.PlayClipAtPoint(somFireball, transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, rangeDeAtaque);
    }
}
