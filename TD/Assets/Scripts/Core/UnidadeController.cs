using UnityEngine;

public class UnidadeController : MonoBehaviour
{
    [Header("Status Base")]
    public int vidaMaxima = 3;
    protected int vidaAtual;

    public int danoBase = 1;

    protected bool estaVivo = true;

    protected virtual void Start()
    {
        vidaAtual = vidaMaxima;
    }

    public virtual void ReceberDano(int quantidade)
    {
        if (!estaVivo) return;

        vidaAtual -= quantidade;

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    protected virtual void Morrer()
    {
        estaVivo = false;
        Destroy(gameObject);
    }

    public int GetVidaAtual()
    {
        return vidaAtual;
    }
}