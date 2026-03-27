using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReliquiasPartidaOrchestrator : MonoBehaviour
{
    [SerializeField] private ReliquiaDatabase database;
    [SerializeField] private Canvas canvasOverlay;
    [SerializeField] private int sortOrderCanvasOverlay = 500;
    [SerializeField] private bool criarCanvasSeAusente = true;

    private readonly List<GameObject> instanciasEfeitos = new List<GameObject>();
    private readonly List<IReliquiaEfeito> efeitosAtivos = new List<IReliquiaEfeito>();

    private void Start()
    {
        if (database == null)
            return;

        if (ReliquiasManager.Instance == null)
            return;

        Canvas canvas = ResolverCanvas();
        Camera cam = Camera.main;

        var contexto = new ReliquiaContext
        {
            CameraPrincipal = cam,
            CanvasOverlay = canvas
        };

        IReadOnlyList<string> equipadas = ReliquiasManager.Instance.GetReliquiasEquipadas();

        for (int i = 0; i < equipadas.Count; i++)
        {
            ReliquiaDef def = database.ObterPorId(equipadas[i]);
            if (def == null || def.prefabEfeitoPartida == null)
                continue;

            GameObject prefabGo = def.prefabEfeitoPartida as GameObject;
            if (prefabGo == null)
                continue;

            GameObject instancia = Instantiate(prefabGo, transform);
            instanciasEfeitos.Add(instancia);

            IReliquiaEfeito efeito = instancia.GetComponent<IReliquiaEfeito>();
            if (efeito == null)
            {
                efeito = instancia.GetComponentInChildren<IReliquiaEfeito>(true);
            }

            if (efeito == null)
            {
                Destroy(instancia);
                instanciasEfeitos.RemoveAt(instanciasEfeitos.Count - 1);
                continue;
            }

            efeito.Iniciar(contexto);
            efeitosAtivos.Add(efeito);
        }
    }

    private Canvas ResolverCanvas()
    {
        if (canvasOverlay != null)
            return canvasOverlay;

        Canvas[] todos = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < todos.Length; i++)
        {
            if (todos[i].renderMode == RenderMode.ScreenSpaceOverlay && todos[i].sortingOrder >= sortOrderCanvasOverlay)
                return todos[i];
        }

        for (int i = 0; i < todos.Length; i++)
        {
            if (todos[i].renderMode == RenderMode.ScreenSpaceOverlay)
                return todos[i];
        }

        if (!criarCanvasSeAusente)
            return null;

        GameObject go = new GameObject("CanvasReliquiasOverlay");
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = sortOrderCanvasOverlay;
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    private void OnDestroy()
    {
        for (int i = efeitosAtivos.Count - 1; i >= 0; i--)
        {
            if (efeitosAtivos[i] != null)
                efeitosAtivos[i].Encerrar();
        }

        efeitosAtivos.Clear();

        for (int i = 0; i < instanciasEfeitos.Count; i++)
        {
            if (instanciasEfeitos[i] != null)
                Destroy(instanciasEfeitos[i]);
        }

        instanciasEfeitos.Clear();
    }
}
