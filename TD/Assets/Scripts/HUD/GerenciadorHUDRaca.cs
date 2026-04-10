using UnityEngine;

/// <summary>
/// Ativa a HUD de relíquias e de torres correspondente à raça do perfil ativo.
///
/// Estrutura esperada na cena: os oito GameObjects abaixo são filhos diretos do Canvas.
///   Relíquias: ReliquiasGoblinHud, ReliquiasHumanoHud, ReliquiasElfoHud, ReliquiasAnaoHud
///   Torres   : HudTorresGoblin,    HudTorresHumanos,   HudTorresElfos,   HudTorresAnoes
///
/// O script percorre os filhos do Canvas (funciona mesmo com objetos inativos).
/// torresUsarSempreGoblin: use enquanto as torres das outras raças não estiverem prontas.
/// </summary>
public class GerenciadorHUDRaca : MonoBehaviour
{
    [Header("Torres")]
    [Tooltip("Marque para usar sempre o HUD Goblin nas torres (enquanto os outros não estão prontos).")]
    [SerializeField] private bool torresUsarSempreGoblin = true;

    // Nomes exatos dos GameObjects na cena
    private static readonly string[] NomesReliquias =
    {
        "ReliquiasGoblinHud",
        "ReliquiasHumanoHud",
        "ReliquiasElfoHud",
        "ReliquiasAnaoHud"
    };

    private static readonly string[] NomesTorres =
    {
        "HudTorresGoblin",
        "HudTorresHumanos",
        "HudTorresElfos",
        "HudTorresAnoes"
    };

    private void Start()
    {
        TipoRaca raca = PerfilJogadorAtivo.TemPerfilAtivo
            ? PerfilJogadorAtivo.Raca
            : TipoRaca.Goblin;

        if (!PerfilJogadorAtivo.TemPerfilAtivo)
            Debug.LogWarning("[GerenciadorHUDRaca] Nenhum perfil ativo. Usando Goblin como fallback.");

        Transform canvas = BuscarCanvas();
        if (canvas == null) return;

        AtivarHuds(canvas, NomesReliquias, raca);

        TipoRaca racaTorres = torresUsarSempreGoblin ? TipoRaca.Goblin : raca;
        AtivarHuds(canvas, NomesTorres, racaTorres);
    }

    /// <summary>
    /// Procura o Canvas raiz da cena. Usa o pai do próprio GameObject se ele for Canvas,
    /// caso contrário busca pelo nome "Canvas".
    /// </summary>
    private Transform BuscarCanvas()
    {
        // Sobe na hierarquia até encontrar o Canvas
        Transform t = transform;
        while (t != null)
        {
            if (t.GetComponent<Canvas>() != null)
                return t;
            t = t.parent;
        }

        // Fallback: busca por nome (Canvas está sempre ativo)
        GameObject go = GameObject.Find("Canvas");
        if (go != null)
            return go.transform;

        Debug.LogError("[GerenciadorHUDRaca] Canvas não encontrado na cena.");
        return null;
    }

    /// <summary>
    /// Percorre todos os filhos diretos do Canvas (incluindo inativos) e ativa
    /// apenas o filho cujo nome corresponde à raça informada.
    /// </summary>
    private static void AtivarHuds(Transform canvas, string[] nomes, TipoRaca raca)
    {
        string sufixoRaca = raca.ToString(); // "Goblin", "Humano", "Elfo", "Anao"

        for (int i = 0; i < canvas.childCount; i++)
        {
            Transform filho = canvas.GetChild(i);
            string nome = filho.name;

            // Verifica se este filho é um dos HUDs que gerenciamos
            bool ehHudGerenciado = false;
            foreach (string n in nomes)
            {
                if (nome == n) { ehHudGerenciado = true; break; }
            }
            if (!ehHudGerenciado) continue;

            // Ativa apenas o da raça correta
            bool deveEstarAtivo = nome.Contains(sufixoRaca);
            filho.gameObject.SetActive(deveEstarAtivo);
        }
    }
}
