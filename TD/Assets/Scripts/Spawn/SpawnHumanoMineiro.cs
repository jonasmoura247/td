using UnityEngine;

public class SpawnHumanoMineiro : MonoBehaviour
{
    public GameObject humanoMineiroPrefab;
    public float spawnRate = 7f;
    public Transform rotasPai;

    private float nextSpawnTime;

    void Start()
    {
        GameObject humano = Instantiate(
            humanoMineiroPrefab,
            Vector2.zero,
            Quaternion.identity
        );

        humano.name = "HumanoMineiro";
    }

    void Update()
    {
    }
}