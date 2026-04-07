using UnityEngine;

/// <summary>Relíquia passiva: registra configuração da explosão Veneno+Fogo para o combate.</summary>
public class EfeitoCarvaoEnxofreSalitre : MonoBehaviour, IReliquiaEfeito
{
    [SerializeField] private ConfiguracaoExplosaoCarvaoEnxofreSalitreSO configuracao;

    /// <summary>Configuração do prefab ativo na partida (última relíquia deste tipo iniciada).</summary>
    public static ConfiguracaoExplosaoCarvaoEnxofreSalitreSO ConfiguracaoAtiva { get; private set; }

    public void Iniciar(ReliquiaContext contexto)
    {
        if (configuracao != null)
            ConfiguracaoAtiva = configuracao;
    }

    public void Encerrar()
    {
        if (ConfiguracaoAtiva == configuracao)
            ConfiguracaoAtiva = null;
    }
}
