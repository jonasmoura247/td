using System.Collections;
using TMPro;
using UnityEngine;

public class NotificacaoToastUI : MonoBehaviour
{
    public static NotificacaoToastUI Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private float duracaoVisivel = 3.5f;
    [SerializeField] private float duracaoFade = 0.35f;

    private Coroutine rotina;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            if (texto != null)
                texto.gameObject.SetActive(false);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Mostrar(string mensagem)
    {
        if (texto == null || canvasGroup == null)
            return;
        texto.text = mensagem;
        if (rotina != null)
            StopCoroutine(rotina);
        rotina = StartCoroutine(AnimarToast());
    }

    private IEnumerator AnimarToast()
    {
        texto.gameObject.SetActive(true);
        float t = 0f;
        while (t < duracaoFade)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / duracaoFade);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(duracaoVisivel);
        t = 0f;
        while (t < duracaoFade)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t / duracaoFade);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        texto.gameObject.SetActive(false);
        rotina = null;
    }
}
