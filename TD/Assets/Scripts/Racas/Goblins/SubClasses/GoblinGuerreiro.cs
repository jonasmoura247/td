using UnityEngine;

public class GoblinGuerreiro : GoblinController
{
    [Header("Atributos do Guerreiro Goblin")]
    [SerializeField] private float multiplicadorDanoGuerreiro = 1f;

    protected override void Start()
    {
        base.Start();
    }

    public override int GetDanoFinal()
    {
        return Mathf.RoundToInt(
            base.GetDanoFinal() * multiplicadorDanoGuerreiro
        );
    }

    protected override void Morrer()
    {
        Debug.Log($"{name} (Guerreiro) morreu!");
        base.Morrer();
    }
}