using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Aplica perfis de layout diferentes conforme o aspect ratio da tela.
///
/// Como usar:
///   1. Adicione ao GameObject raiz da UI (filho do Canvas).
///   2. Configure os "Perfis" no Inspector — cada perfil e um conjunto de
///      ajustes aplicados para um intervalo de aspect ratio.
///   3. O adapter detecta automaticamente o aspect ratio atual e aplica
///      o perfil correto ao iniciar e quando a resolucao muda.
///
/// Isso permite, por exemplo:
///   - Ultrawide (21:9): aumentar a largura minima dos paineis laterais
///   - Portrait (9:16):  reorganizar o HUD inferior para ficar empilhado
///   - 4:3 classico:     reduzir tamanho dos elementos para caber na tela
/// </summary>
public class AspectRatioUIAdapter : MonoBehaviour
{
    [Serializable]
    public class ConfiguracaoPorPerfil
    {
        [Tooltip("Perfil de aspect ratio que ativa esta configuracao.")]
        public PerfilAspectRatio perfil = PerfilAspectRatio.Widescreen;

        [Tooltip("GameObjects a ativar neste perfil (desativados nos outros).")]
        public List<GameObject> objetosAtivos = new List<GameObject>();

        [Tooltip("GameObjects a desativar neste perfil.")]
        public List<GameObject> objetosInativos = new List<GameObject>();

        [Tooltip("RectTransforms a redimensionar neste perfil.")]
        public List<AjusteRectTransform> ajustesRect = new List<AjusteRectTransform>();
    }

    [Serializable]
    public class AjusteRectTransform
    {
        public RectTransform alvo;
        public Vector2 anchorMin = new Vector2(0.5f, 0f);
        public Vector2 anchorMax = new Vector2(0.5f, 0f);
        public Vector2 pivot     = new Vector2(0.5f, 0f);
        public Vector2 sizeDelta = new Vector2(440f, 96f);
        public Vector2 anchoredPosition = Vector2.zero;
        [Tooltip("Se falso, nao aplica o sizeDelta (mantem o tamanho existente).")]
        public bool aplicarSizeDelta = true;
        [Tooltip("Se falso, nao aplica o anchoredPosition.")]
        public bool aplicarPosicao   = true;
    }

    [Header("Configuracoes por perfil")]
    [SerializeField] private List<ConfiguracaoPorPerfil> configuracoes = new List<ConfiguracaoPorPerfil>();

    [Header("Perfil atual (leitura)")]
    [SerializeField, HideInInspector] private PerfilAspectRatio _perfilAtual;

    private PerfilAspectRatio _perfilAplicado = (PerfilAspectRatio)(-1);

    // ─── Ciclo de vida ───────────────────────────────────────────────────────
    private void Start()
    {
        AplicarPerfilAtual();
        UIResponsividadeManager.OnLayoutAtualizado += AplicarPerfilAtual;
    }

    private void OnDestroy()
    {
        UIResponsividadeManager.OnLayoutAtualizado -= AplicarPerfilAtual;
    }

    // ─── Logica ──────────────────────────────────────────────────────────────

    public void AplicarPerfilAtual()
    {
        float aspect = (float)Screen.width / Mathf.Max(Screen.height, 1);
        PerfilAspectRatio perfilNovo = UIResponsividadeManager.CalcularPerfil(aspect);

        _perfilAtual = perfilNovo;

        // Evita reaplicar o mesmo perfil sem necessidade
        if (perfilNovo == _perfilAplicado)
            return;

        _perfilAplicado = perfilNovo;
        AplicarConfiguracaoPara(perfilNovo);
    }

    private void AplicarConfiguracaoPara(PerfilAspectRatio perfil)
    {
        foreach (ConfiguracaoPorPerfil cfg in configuracoes)
        {
            bool estePerfilAtivo = cfg.perfil == perfil;

            foreach (GameObject go in cfg.objetosAtivos)
                if (go != null) go.SetActive(estePerfilAtivo);

            foreach (GameObject go in cfg.objetosInativos)
                if (go != null) go.SetActive(!estePerfilAtivo);

            if (estePerfilAtivo)
            {
                foreach (AjusteRectTransform ajuste in cfg.ajustesRect)
                    AplicarAjusteRect(ajuste);
            }
        }
    }

    private static void AplicarAjusteRect(AjusteRectTransform ajuste)
    {
        if (ajuste.alvo == null) return;

        ajuste.alvo.anchorMin = ajuste.anchorMin;
        ajuste.alvo.anchorMax = ajuste.anchorMax;
        ajuste.alvo.pivot     = ajuste.pivot;

        if (ajuste.aplicarSizeDelta)
            ajuste.alvo.sizeDelta = ajuste.sizeDelta;

        if (ajuste.aplicarPosicao)
            ajuste.alvo.anchoredPosition = ajuste.anchoredPosition;
    }
}
