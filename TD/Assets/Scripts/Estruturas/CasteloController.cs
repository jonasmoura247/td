using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasteloController : EstruturasController
{
    [Header("Detecção")]
    [Tooltip("Raio de detecção de inimigos ao redor do castelo.")]
    [SerializeField] private float raioDeteccao = 4f;

    [Tooltip("Intervalo entre cada varredura por inimigos próximos.")]
    [SerializeField] private float intervaloScan = 0.15f;

    private readonly Dictionary<UnidadeController, float> _ultimoAtaque
        = new Dictionary<UnidadeController, float>();

    private readonly Collider2D[] _buffer = new Collider2D[32];

    protected override void Start()
    {
        base.Start();
        StartCoroutine(LoopAtaque());
    }

    private IEnumerator LoopAtaque()
    {
        while (!destruida)
        {
            yield return new WaitForSeconds(intervaloScan);
            ScanearEAtacar();
        }
    }

    private void ScanearEAtacar()
    {
        if (destruida) return;

        LayerMask maskInimigo = LayerMask.GetMask("Inimigo");
        var filtro = new ContactFilter2D();
        filtro.SetLayerMask(maskInimigo);
        int count = Physics2D.OverlapCircle(transform.position, raioDeteccao, filtro, _buffer);

        float agora = Time.time;

        for (int i = 0; i < count; i++)
        {
            if (_buffer[i] == null) continue;

            UnidadeController unidade = _buffer[i].GetComponentInParent<UnidadeController>();
            if (unidade == null || unidade.faccao == faccao || !unidade.EstaVivo)
                continue;

            if (_ultimoAtaque.TryGetValue(unidade, out float ultimo) &&
                agora - ultimo < unidade.GetIntervaloAtaque())
                continue;

            _ultimoAtaque[unidade] = agora;

            int danoBase = unidade.GetDanoAtaque();
            int dano = BestiarioAtaqueInimigo.CalcularDanoAoCastelo(unidade, danoBase, null, out bool critico);
            var pedido = PedidoDanoFlutuante.Criar(dano, TipoDanoCombate.Fisico, critico);
            ServicoAplicacaoDano.Aplicar(this, transform.position, dano, pedido, dispararProcsRunaBestiario: false);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, raioDeteccao);
    }
}
