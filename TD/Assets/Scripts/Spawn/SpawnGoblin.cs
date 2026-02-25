using UnityEngine;

public class SpawnGoblin : MonoBehaviour
{
    public GameObject goblinPrefab; // Agora vai ser o PREFAB, não o da cena
    public float spawnRate = 7f;
    public Transform rotasPai;

    private float nextSpawnTime;

    void Start()
    {
        // Verifica se o prefab está atribuído
        if (goblinPrefab == null)
        {
            Debug.LogError(" Prefab do goblin não está atribuído no SpawnGoblin!");
            return;
        }

        Debug.Log($"Prefab configurado: {goblinPrefab.name}");

        // Diagnóstico do prefab
        var rb = goblinPrefab.GetComponent<Rigidbody2D>();
        Debug.Log($"Rigidbody2D no prefab: {(rb != null ? "OK" : "NÃO TEM")}");
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            nextSpawnTime = Time.time + spawnRate;

            int index = Random.Range(0, rotasPai.childCount);
            Transform rotaEscolhida = rotasPai.GetChild(index);

            // Agora instancia do PREFAB, não do GameObject da cena
            GameObject goblin = Instantiate(goblinPrefab);

            // Opcional: dar um nome único para cada goblin
            goblin.name = $"Goblin_{System.DateTime.Now.Ticks}";

            MovimentoUnidade movimento = goblin.GetComponent<MovimentoUnidade>();
            if (movimento != null)
            {
                movimento.DefinirRota(rotaEscolhida);
                Debug.Log($" Goblin spawnado na rota {index}");
            }
        }
    }
}