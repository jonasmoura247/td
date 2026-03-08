using UnityEngine;

public class HumanoController : UnidadeController
{
    [Header("Atributos Raciais Humano")]
    [SerializeField] protected float multiplicadorDanoRacial = 1f;

    [Header("Configurações de Recuo")]
    public float tempoInvulnerabilidade = 0f;
    //private float proximoTempoDano = 0f;

    protected override void Start()
    {
        vidaMaxima = 10;
        danoBase = 1;
        base.Start();
    }
}
