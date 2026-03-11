using UnityEngine;
using UnityEngine.Events;

public class LootColetavel : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string nomeLoot = "Loot";
    [SerializeField] private int quantidade = 1;
    [SerializeField] private float tempoNoChaoSegundos = 40f;

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

        Debug.Log($"[Loot] Coletado: {quantidade}x {nomeLoot}");

        Sprite spriteItem = null;
        var sr = GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null)
            spriteItem = sr.sprite;

        if (spriteItem == null)
            Debug.LogWarning($"[Loot] {nomeLoot} nao tem SpriteRenderer com sprite. O item sera adicionado sem icone.");

        if (LootManager.Instance == null)
        {
            Debug.LogError("[Loot] LootManager nao encontrado! Crie um GameObject com o script Loot Manager (ex: SistemaInventario) na cena.");
            coletado = false;
            return;
        }

        LootManager.Instance.AdicionarItem(nomeLoot, spriteItem, quantidade);

        Destroy(gameObject);
    }
}