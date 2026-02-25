using UnityEngine;

public class EstruturasController : MonoBehaviour
{
    [Header("Configurações de Estrutura")]
    public int vidaMaxima = 15; 
    protected int vidaAtual;
    protected bool destruida = false;

    protected virtual void Start()
    {
        if (vidaMaxima <= 0) vidaMaxima = 1;

        vidaAtual = vidaMaxima;
    }

    public virtual void ReceberDano(int quantidade)
    {
        if (destruida) return;
        vidaAtual -= quantidade;
        if (vidaAtual <= 0) DestruirEstrutura();
    }

    protected virtual void DestruirEstrutura()
    {
        destruida = true;
        Destroy(gameObject);
    }

    public int GetVidaAtual() => vidaAtual;
}