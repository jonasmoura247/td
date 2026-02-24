using UnityEngine;

public class SpawnGoblin : MonoBehaviour
{
    public GameObject goblinEnemy;
    public float spawnRate = 7f;

    public Transform rotasPai; // arraste o objeto "Rotas" aqui

    private float nextSpawnTime;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            nextSpawnTime = Time.time + spawnRate;

            // Escolhe rota aleatória
            int index = Random.Range(0, rotasPai.childCount);
            Transform rotaEscolhida = rotasPai.GetChild(index);

            // Instancia goblin
            GameObject goblin = Instantiate(goblinEnemy);

            // Define rota
            goblin.GetComponent<MovNpcGoblin>().DefinirRota(rotaEscolhida);
        }
    }
}