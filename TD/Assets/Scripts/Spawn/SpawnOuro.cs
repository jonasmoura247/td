using UnityEngine;

public class SpawnOuro : MonoBehaviour
{
    [Header("Configuração do Spawn de Ouro")]
    public GameObject prefabMinerio;
    public int quantidadeParaGerar = 10;
    public float intervaloSpawn = 5f;

    [Header("Área de Spawn")]
    public Vector2 areaMin;
    public Vector2 areaMax;

    private int quantidadeGerada = 0;
    private float temporizador = 0f;

    private void Update()
    {
        if (quantidadeGerada >= quantidadeParaGerar)
            return;

        temporizador += Time.deltaTime;

        if (temporizador >= intervaloSpawn)
        {
            SpawnarMinerio();
            temporizador = 0f;
        }
    }

    private void SpawnarMinerio()
    {
        Vector2 posicaoAleatoria = new Vector2(
            Random.Range(areaMin.x, areaMax.x),
            Random.Range(areaMin.y, areaMax.y)
        );

        Instantiate(prefabMinerio, posicaoAleatoria, Quaternion.identity);
        quantidadeGerada++;
    }
}