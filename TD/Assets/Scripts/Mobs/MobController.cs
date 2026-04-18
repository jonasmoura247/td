using UnityEngine;

/// <summary>
/// Base de todos os mobs inimigos do mundo (não-Goblins).
/// Garante facção Inimigo, multiplicador de dano configurável e
/// ponto de extensão para comportamentos únicos por criatura.
/// </summary>
public class MobController : UnidadeController
{
    [Header("Configurações do Mob")]
    [SerializeField] protected float multiplicadorDano = 1f;

    protected int bonusDanoFixo = 0;

    protected override void Awake()
    {
        faccao = Faccao.Inimigo;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    public virtual int GetDanoFinal()
    {
        return Mathf.RoundToInt(danoBase * multiplicadorDano) + bonusDanoFixo;
    }

    public override int GetDanoAtaque() => AplicarModificadoresTalentosDano(GetDanoFinal());
}
