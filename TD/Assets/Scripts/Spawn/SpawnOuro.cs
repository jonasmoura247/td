using UnityEngine;

public class SpawnOuro : MonoBehaviour
{
    [Header("Configura??o do Spawn de Ouro")]
    public GameObject prefabMinerio;
    public int quantidadeParaGerar = 10;
    public float intervaloSpawn = 5f;

    [Header("Range de Spawn (relativo ? base)")]
    [Tooltip("Dist?ncia m?nima da base para o min?rio nascer")]
    [SerializeField] private float distanciaMinimaBase = 10f;
    [Tooltip("Dist?ncia m?xima da base para o min?rio nascer")]
    [SerializeField] private float distanciaMaximaBase = 50f;

    [Header("Fallback - ?rea fixa (se base n?o existir)")]
    [SerializeField] private Vector2 areaMin = new Vector2(-50, -25);
    [SerializeField] private Vector2 areaMax = new Vector2(50, 25);

    private int quantidadeGerada = 0;
    private float temporizador = 0f;
    private Transform baseTransform;

    private void Start()
    {
        GameObject baseObj = GameObject.FindWithTag("Castelo");
        if (baseObj != null)
            baseTransform = baseObj.transform;

        SpawnarMinerio();
    }

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
        Vector2 posicaoAleatoria;

        if (baseTransform != null)
        {
            Vector2 centroBase = baseTransform.position;
            Vector2 direcao = Random.insideUnitCircle;
            if (direcao.sqrMagnitude < 0.01f)
                direcao = Vector2.up;
            direcao.Normalize();
            float distancia = Random.Range(distanciaMinimaBase, distanciaMaximaBase);
            posicaoAleatoria = centroBase + direcao * distancia;
        }
        else
        {
            posicaoAleatoria = new Vector2(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y)
            );
        }

        Instantiate(prefabMinerio, posicaoAleatoria, Quaternion.identity);
        quantidadeGerada++;
    }
}