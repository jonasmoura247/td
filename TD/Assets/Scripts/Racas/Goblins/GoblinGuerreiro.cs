using UnityEngine;

public class GoblinGuerreiro : GoblinController
{
    [Header("Atributos do Guerreiro Goblin")]
    [SerializeField] private float multiplicadorDanoGuerreiro = 1.2f;

    //[Header("Efeitos Visuais")]
    //[SerializeField] private GameObject efeitoSanguePrefab;
    //[SerializeField] private Color corGuerreiro = new Color(0.8f, 0.2f, 0.2f);

    protected override void Start()
    {
        // Configurações específicas do guerreiro
        vidaMaxima = 4;
        danoBase = 1;

        base.Start();
       // AplicarVisualGuerreiro();
    }

    //private void AplicarVisualGuerreiro()
    //{
    //    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    //    if (sr != null)
    //    {
    //        sr.color = corGuerreiro;
    //    }
    //}

    public override int GetDanoFinal()
    {
        return Mathf.RoundToInt(base.GetDanoFinal() * multiplicadorDanoGuerreiro);
    }

    protected override void Morrer()
    {
        //if (efeitoSanguePrefab != null && estaVivo)
        //{
        //    Instantiate(efeitoSanguePrefab, transform.position, Quaternion.identity);
        //}

        Debug.Log($"{name} (Guerreiro) morreu!");
        base.Morrer();
    }
}