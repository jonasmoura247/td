using UnityEngine;
using UnityEngine.Events;

public class LootColetavel : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string nomeLoot = "Loot";
    [SerializeField] private int quantidade = 1;
    [SerializeField] private float tempoNoChaoSegundos = 40f;
    [Tooltip("Se ativo, ao coletar adiciona 'quantidade' ao OuroManager.")]
    [SerializeField] private bool adicionarComoOuro = false;

    [Header("Eventos")]
    [SerializeField] private UnityEvent aoColetar;

    private bool coletado;

    private void Start()
    {
        if (tempoNoChaoSegundos > 0f)
            Destroy(gameObject, tempoNoChaoSegundos);
    }

    public void Configurar(string novoNomeLoot, int novaQuantidade, float novoTempoNoChaoSegundos)
    {
        if (!string.IsNullOrWhiteSpace(novoNomeLoot))
            nomeLoot = novoNomeLoot;

        quantidade = Mathf.Max(1, novaQuantidade);
        tempoNoChaoSegundos = Mathf.Max(0f, novoTempoNoChaoSegundos);
    }

    public void Coletar()
    {
        if (coletado)
            return;

        coletado = true;

        if (adicionarComoOuro && OuroManager.Instance != null)
            OuroManager.Instance.AdicionarOuro(quantidade);

        Debug.Log($"[Loot] Coletado: {quantidade}x {nomeLoot}");

        aoColetar?.Invoke();

        Destroy(gameObject);
    }
}