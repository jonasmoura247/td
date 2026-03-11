using UnityEngine;
using System;

/// <summary>
/// Singleton que gerencia o ouro do jogador. Use para adicionar/gastar ouro e exibir na HUD.
/// </summary>
public class OuroManager : MonoBehaviour
{
    public static OuroManager Instance { get; private set; }

    [Header("Configuração")]
    [SerializeField] private int ouroInicial = 0;

    private int ouroAtual;

    /// <summary>Disparado quando o valor do ouro muda (valor atual).</summary>
    public event Action<int> OnOuroAlterado;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ouroAtual = ouroInicial;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Retorna a quantidade atual de ouro.</summary>
    public int GetOuro() => ouroAtual;

    /// <summary>Adiciona ouro. Retorna a quantidade efetivamente adicionada.</summary>
    public int AdicionarOuro(int quantidade)
    {
        if (quantidade <= 0) return 0;
        ouroAtual += quantidade;
        OnOuroAlterado?.Invoke(ouroAtual);
        return quantidade;
    }

    /// <summary>Gasta ouro se houver saldo. Retorna true se conseguiu gastar.</summary>
    public bool GastarOuro(int quantidade)
    {
        if (quantidade <= 0 || ouroAtual < quantidade)
            return false;
        ouroAtual -= quantidade;
        OnOuroAlterado?.Invoke(ouroAtual);
        return true;
    }

    /// <summary>Define o ouro diretamente (ex.: ao carregar save).</summary>
    public void SetOuro(int valor)
    {
        ouroAtual = Mathf.Max(0, valor);
        OnOuroAlterado?.Invoke(ouroAtual);
    }
}
