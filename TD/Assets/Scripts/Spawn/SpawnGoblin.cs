
//Vamos manter assim por enquanto, pq conseguimos controlar pelo wavemanager,
//mas futuramente podemos querer spawnar goblins em outros momentos, como por exemplo, quando o jogador entrar em uma área específica do mapa.

//using UnityEngine;

//public class SpawnGoblin : MonoBehaviour
//{
//    public GameObject[] goblinsPrefabs;
//    public float spawnRate = 5f;
//    public Transform rotasPai;

//    private float nextSpawnTime;

//    void Update()
//    {
//        if (Time.time >= nextSpawnTime)
//        {
//            nextSpawnTime = Time.time + spawnRate;

//            if (rotasPai.childCount == 0) return;

//            int indexRota = Random.Range(0, rotasPai.childCount);
//            Transform rotaEscolhida = rotasPai.GetChild(indexRota);

//            if (rotaEscolhida.childCount == 0) return;

//            Transform primeiroPonto = rotaEscolhida.GetChild(0);

//            int indexGoblin = Random.Range(0, goblinsPrefabs.Length);
//            GameObject prefabEscolhido = goblinsPrefabs[indexGoblin];

//            GameObject goblin = Instantiate(prefabEscolhido, primeiroPonto.position, Quaternion.identity);

//            MovimentoUnidade movimento = goblin.GetComponent<MovimentoUnidade>();

//            if (movimento != null)
//            {
//                movimento.DefinirRota(rotaEscolhida);
//            }
//        }
//    }
//}