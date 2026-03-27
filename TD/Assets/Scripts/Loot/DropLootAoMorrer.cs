using UnityEngine;

public class DropLootAoMorrer : MonoBehaviour, IMorteListener
{
    [System.Serializable]
    public class Drop
    {
        public bool habilitado = true;
        public GameObject prefabLoot;
        [Range(0f, 1f)] public float chance = 1f;
        public int quantidade = 1;
        public string nomeLoot = "Loot";
    }

    [System.Serializable]
    public class DropReliquia
    {
        public bool habilitado = true;
        public ReliquiaDef reliquia;
        public string idReliquiaManual = "";
        [Range(0f, 1f)] public float chance = 1f;
        public bool mostrarAvisoSeJaPossuir = true;
    }

    [Header("Drops de item no chao")]
    [SerializeField] private Drop[] drops;

    [Header("Reliquia (save atual)")]
    [SerializeField] private DropReliquia dropReliquia = new DropReliquia();

    [Header("Mensagens toast (reliquia)")]
    [SerializeField] private string mensagemNovaReliquia = "Nova reliquia obtida! Verifique seu Salao de Reliquias.";
    [SerializeField] private string mensagemJaPossuiReliquia = "Voce ja possui esta reliquia.";

    [Header("Spawn item")]
    [SerializeField] private float raioOffset = 0.25f;
    [SerializeField] private float tempoNoChaoSegundos = 40f;

    private bool dropou;

    public void OnMorte(UnidadeController unidade)
    {
        if (dropou)
            return;
        dropou = true;

        if (drops != null && drops.Length > 0)
        {
            foreach (var drop in drops)
            {
                if (drop == null || !drop.habilitado || drop.prefabLoot == null)
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

        if (!dropReliquia.habilitado || ReliquiasManager.Instance == null)
            return;

        string id = null;
        if (dropReliquia.reliquia != null && !string.IsNullOrEmpty(dropReliquia.reliquia.idReliquia))
            id = dropReliquia.reliquia.idReliquia;
        else if (!string.IsNullOrEmpty(dropReliquia.idReliquiaManual))
            id = dropReliquia.idReliquiaManual;

        if (string.IsNullOrEmpty(id))
            return;

        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            return;

        if (Random.value > Mathf.Clamp01(dropReliquia.chance))
            return;

        if (ReliquiasManager.Instance.PossuiReliquia(id))
        {
            if (dropReliquia.mostrarAvisoSeJaPossuir && NotificacaoToastUI.Instance != null)
                NotificacaoToastUI.Instance.Mostrar(mensagemJaPossuiReliquia);
            return;
        }

        if (ReliquiasManager.Instance.AdicionarReliquiaPossuida(id))
        {
            if (NotificacaoToastUI.Instance != null)
                NotificacaoToastUI.Instance.Mostrar(mensagemNovaReliquia);
        }
    }
}
