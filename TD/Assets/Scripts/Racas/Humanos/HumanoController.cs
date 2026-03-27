using UnityEngine;

public class HumanoController : UnidadeController
{
    [Header("Atributos Raciais Humano")]
    [SerializeField] protected float multiplicadorDanoRacial = 1f;

    [Header("Configura��es de Recuo")]
    public float tempoInvulnerabilidade = 0f;
    //private float proximoTempoDano = 0f;

    protected override void Start()
    {
        base.Start();
    }
}
