using TMPro;
using UnityEngine;

/// <summary>
/// Configuração global do texto de dano flutuante (DoT) por cena.
/// Coloque um objeto com este componente na cena (ex.: Mapa_1) para ajustar fonte, cor e animação no Inspector.
/// </summary>
[DefaultExecutionOrder(-100)]
public class ConfiguracaoTextoDanoFlutuanteMapa : MonoBehaviour
{
    public static ConfiguracaoTextoDanoFlutuanteMapa Instancia { get; private set; }

    public enum ModoCor
    {
        /// <summary>Somente para <see cref="TextoDanoFlutuante.Inicializar(int, Color)"/> legado: usa a cor passada.</summary>
        UsarCorDaDefinicaoDoT,
        /// <summary>Força uma única cor para todo dano flutuante (ignora biblioteca e overrides do pedido).</summary>
        ForcarCorDoMapa,
        /// <summary>Cores por <see cref="TipoDanoCombate"/> via biblioteca (recomendado com <see cref="PedidoDanoFlutuante"/>).</summary>
        UsarBibliotecaTiposDano
    }

    [Header("Texto")]
    [Tooltip("Opcional. Se vazio, usa a fonte padrão do TextoDanoFlutuante (Resources).")]
    [SerializeField] private TMP_FontAsset fonte;
    [SerializeField] private float tamanhoFonte = 56f;
    [Tooltip("Opcional. Se vazio, usa TextoDanoFlutuante.Criar (Canvas gerado em código).")]
    [SerializeField] private GameObject prefabTextoFlutuante;

    [Header("Tipos de dano")]
    [SerializeField] private BibliotecaEstilosTipoDanoSO bibliotecaEstilosTipoDano;

    [Header("Cor")]
    [SerializeField] private ModoCor modoCor = ModoCor.UsarBibliotecaTiposDano;
    [SerializeField] private Color corMapa = Color.white;

    [Header("Animação")]
    [SerializeField] private float duracaoSegundos = 0.85f;
    [SerializeField] private float deslocamentoMundo = 0.45f;

    [Header("Canvas world space")]
    [SerializeField] private float escalaCanvasUniforme = 0.012f;
    [SerializeField] private int sortingOrderCanvas = 2000;

    public TMP_FontAsset Fonte => fonte;
    public float TamanhoFonte => tamanhoFonte;
    public GameObject PrefabTextoFlutuante => prefabTextoFlutuante;
    public BibliotecaEstilosTipoDanoSO BibliotecaEstilosTipoDano => bibliotecaEstilosTipoDano;
    public ModoCor ModoCorTexto => modoCor;
    public Color CorMapa => corMapa;
    public float DuracaoSegundos => duracaoSegundos;
    public float DeslocamentoMundo => deslocamentoMundo;
    public float EscalaCanvasUniforme => escalaCanvasUniforme;
    public int SortingOrderCanvas => sortingOrderCanvas;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Debug.LogWarning("[ConfiguracaoTextoDanoFlutuanteMapa] Já existe uma instância na cena; mantendo a primeira.", this);
            return;
        }

        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this)
            Instancia = null;
    }
}
