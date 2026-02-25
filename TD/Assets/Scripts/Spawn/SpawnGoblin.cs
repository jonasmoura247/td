using UnityEngine;

public class SpawnGoblin : MonoBehaviour
{
    public GameObject goblinPrefab; 
    public float spawnRate = 7f;
    public Transform rotasPai;

    private float nextSpawnTime;

    void Start()
    {
        var rb = goblinPrefab.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            nextSpawnTime = Time.time + spawnRate;

            int index = Random.Range(0, rotasPai.childCount);
            Transform rotaEscolhida = rotasPai.GetChild(index);

            GameObject goblin = Instantiate(goblinPrefab);

            // dar um nome único para cada goblin "goblin_uuid"
            goblin.name = $"Goblin_{System.DateTime.Now.Ticks}";

            MovimentoUnidade movimento = goblin.GetComponent<MovimentoUnidade>();
            if (movimento != null)
            {
                movimento.DefinirRota(rotaEscolhida);
            }
        }
    }
}