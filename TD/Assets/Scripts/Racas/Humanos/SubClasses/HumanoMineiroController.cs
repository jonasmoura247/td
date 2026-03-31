using UnityEngine;

public class HumanoMineiroController : MineiroAliadoController
{
    [Header("Atributos Raciais Humano")]
    [SerializeField] protected float multiplicadorDanoRacial = 1f;

    public float tempoInvulnerabilidade = 0f;
}
