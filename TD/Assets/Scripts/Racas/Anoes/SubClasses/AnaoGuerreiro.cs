using UnityEngine;

public class AnaoGuerreiro : AnoesController
{
    [SerializeField] private float multiplicadorDanoGuerreiro = 1.15f;
    [Header("Configuracao Arqueiro")]
    [SerializeField] private float alcanceAtaque = 25f;
    [SerializeField] private float intervaloAtaque = 2f;
    [SerializeField] private int danoFlecha = 2;
    [SerializeField] private GameObject prefabFlecha;
    [SerializeField] private Transform pontoDisparo;

    private float proximoAtaque;

    protected override void Start()
    {
        alcanceAtaque = 25f;
        intervaloAtaque = 2f;
        danoFlecha = 2;
        base.Start();
        CarregarPrefabFlechaSeNecessario();
    }

    private void Update()
    {
        if (!estaVivo)
            return;

        UnidadeController inimigoProximo = EncontrarInimigoMaisProximo(alcanceAtaque);
        if (!EhAlvoInimigoValido(inimigoProximo))
        {
            movimento?.ContinuarMovimento();
            return;
        }

        float distancia = Vector2.Distance(transform.position, inimigoProximo.transform.position);
        if (distancia <= alcanceAtaque)
        {
            movimento?.PararMovimento();
            Atacar(inimigoProximo.transform);
            return;
        }

        movimento?.ContinuarMovimento();
    }

    private void Atacar(Transform alvo)
    {
        if (Time.time < proximoAtaque)
            return;

        if (alvo == null)
            return;

        CarregarPrefabFlechaSeNecessario();
        if (prefabFlecha == null)
            return;

        proximoAtaque = Time.time + intervaloAtaque;

        Vector3 origem = pontoDisparo != null ? pontoDisparo.position : transform.position;
        GameObject flecha = Instantiate(prefabFlecha, origem, Quaternion.identity);

        LancaProjetil projetil = flecha.GetComponent<LancaProjetil>();
        if (projetil != null)
            projetil.Inicializar(alvo, danoFlecha, gameObject.name);
    }

    private void CarregarPrefabFlechaSeNecessario()
    {
        GameObject flecha = Resources.Load<GameObject>("Projetis/Flecha");
        if (flecha != null)
            prefabFlecha = flecha;
    }

    public override int GetDanoFinal()
    {
        return Mathf.RoundToInt(base.GetDanoFinal() * multiplicadorDanoGuerreiro);
    }
}
