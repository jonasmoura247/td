using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class SetaNavegacaoMapa : MonoBehaviour
{
    [SerializeField] private string nomeCenaDestino;
    [SerializeField] private float duracaoTintClique = 0.15f;

    public System.Action<string> CarregarCena;

    private SpriteRenderer _sr;
    private static readonly Color CorTintClique = new Color(0.5f, 0.5f, 0.5f, 1f);

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        CarregarCena = SceneManager.LoadScene;
    }

    private void OnMouseDown()
    {
        if (!Application.isPlaying) return;
        if (string.IsNullOrEmpty(nomeCenaDestino))
        {
            Debug.LogWarning("[SetaNavegacaoMapa] nomeCenaDestino não configurado.");
            return;
        }
        Time.timeScale = 1f;
        StartCoroutine(EfeitoCliqueECarregar());
    }

    private IEnumerator EfeitoCliqueECarregar()
    {
        if (_sr != null)
            _sr.color = CorTintClique;

        yield return new WaitForSecondsRealtime(duracaoTintClique);

        CarregarCena?.Invoke(nomeCenaDestino);
    }
}
