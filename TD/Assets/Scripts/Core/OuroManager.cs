using UnityEngine;
using System;

/// <summary>
/// Singleton Unity que expõe OuroLogica ao runtime do jogo.
/// Toda regra de negócio vive em <see cref="OuroLogica"/> — testável sem MonoBehaviour.
/// </summary>
public class OuroManager : MonoBehaviour
{
    public static OuroManager Instance { get; private set; }

    [Header("Configuração")]
    [SerializeField] private int ouroInicial = 0;

    /// <summary>Lógica pura de ouro. Acessível para testes de integração.</summary>
    public OuroLogica Logica { get; private set; }

    /// <summary>Disparado quando o valor do ouro muda (valor atual).</summary>
    public event Action<int> OnOuroAlterado
    {
        add    => Logica.OnOuroAlterado += value;
        remove => Logica.OnOuroAlterado -= value;
    }

    /// <summary>Proxy estatico: disparado ao adicionar ouro (delta, total).</summary>
    public static event Action<int, int> OnOuroAdicionadoStatic;
    /// <summary>Proxy estatico: disparado ao gastar ouro (delta).</summary>
    public static event Action<int> OnOuroGastoStatic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Logica = new OuroLogica(ouroInicial);
            Logica.OnOuroAdicionado += (delta, total) => OnOuroAdicionadoStatic?.Invoke(delta, total);
            Logica.OnOuroGasto      += delta           => OnOuroGastoStatic?.Invoke(delta);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Retorna a quantidade atual de ouro.</summary>
    public int GetOuro() => Logica.GetOuro();

    /// <summary>Adiciona ouro. Retorna a quantidade efetivamente adicionada.</summary>
    public int AdicionarOuro(int quantidade) => Logica.AdicionarOuro(quantidade);

    /// <summary>Gasta ouro se houver saldo. Retorna true se conseguiu gastar.</summary>
    public bool GastarOuro(int quantidade) => Logica.GastarOuro(quantidade);

    /// <summary>Define o ouro diretamente (ex.: ao carregar save).</summary>
    public void SetOuro(int valor) => Logica.SetOuro(valor);
}
