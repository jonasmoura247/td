using UnityEngine;

public class DropLootAoMorrer : MonoBehaviour, IMorteListener
{
    [System.Serializable]
    public class Drop
    {
        public GameObject prefabLoot;
        [Range(0f, 1f)] public float chance = 1f;
        public int quantidade = 1;
        public string nomeLoot = "Loot";
    }

    [Header("Drops")]
    [SerializeField] private Drop[] drops;

    [Header("Spawn")]
    [SerializeField] private float raioOffset = 0.25f;
    [SerializeField] private float tempoNoChaoSegundos = 40f;

    private bool dropou;

    public void OnMorte(UnidadeController unidade)
    {
        if (dropou)
            return;

        dropou = true;

        if (drops == null || drops.Length == 0)
            return;

        foreach (var drop in drops)
        {
            if (drop == null || drop.prefabLoot == null)
                continue;

            if (Random.value > Mathf.Clamp01(drop.chance))
                continue;

            Vector2 offset = Random.insideUnitCircle * raioOffset;
            GameObject loot = Instantiate(drop.prefabLoot, transform.position + (Vector3)offset, Quaternion.identity);

            LootColetavel pickup = loot.GetComponent<LootColetavel>();
            if (pickup != null)
                pickup.Configurar(drop.nomeLoot, drop.quantidade, tempoNoChaoSegundos);

            Debug.Log($"[Loot] Dropado por {unidade.name}: {drop.quantidade}x {drop.nomeLoot}");    
        }
    }
}
