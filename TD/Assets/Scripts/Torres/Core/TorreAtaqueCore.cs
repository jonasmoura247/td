using UnityEngine;

/// <summary>
/// Cooldown e spawn de projétil (mesmo fluxo que GoblinLanceiro: LancaProjetil em pontoDisparo).
/// </summary>
public class TorreAtaqueCore
{
    private Transform _pontoDisparo;
    private TorreAtributosCore _atributos;
    private TorreAnimacaoDisparoBridge _bridge;
    private string _nomeOrigemLog;

    private float _proximoAtaquePermitidoEm;
    private Transform _alvoPendente;
    private bool _esperandoEventoAnimacao;

    public bool EsperandoAnimacao => _esperandoEventoAnimacao;

    public void Configurar(
        Transform pontoDisparo,
        TorreAtributosCore atributos,
        TorreAnimacaoDisparoBridge bridge,
        string nomeOrigemLog)
    {
        _pontoDisparo = pontoDisparo;
        _atributos = atributos;
        _bridge = bridge;
        _nomeOrigemLog = nomeOrigemLog;
    }

    /// <summary>Inicia ataque se o cooldown permitir; pode disparar já ou delegar à animação.</summary>
    public void TentarAtacar(Transform alvo)
    {
        if (alvo == null || _pontoDisparo == null || _atributos == null)
            return;

        float agora = Time.time;
        if (agora < _proximoAtaquePermitidoEm)
            return;

        GameObject prefab = _atributos.ObterPrefabProjetil();
        if (prefab == null)
            return;

        _alvoPendente = alvo;
        _proximoAtaquePermitidoEm = agora + _atributos.ObterIntervaloEfetivo();

        if (_bridge != null && _bridge.TentarIniciarAtaquePorAnimacao())
        {
            _esperandoEventoAnimacao = true;
            return;
        }

        _esperandoEventoAnimacao = false;
        ExecutarDisparoProjetil();
        _alvoPendente = null;
    }

    /// <summary>Chamado pelo Animation Event (ou pela bridge).</summary>
    public void ExecutarDisparoPendentePorAnimacao()
    {
        if (!_esperandoEventoAnimacao)
            return;
        _esperandoEventoAnimacao = false;
        ExecutarDisparoProjetil();
        _alvoPendente = null;
    }

    public void CancelarPendenteAnimacao()
    {
        _esperandoEventoAnimacao = false;
        _alvoPendente = null;
    }

    private void ExecutarDisparoProjetil()
    {
        Transform alvo = _alvoPendente;
        if (alvo == null || _pontoDisparo == null)
            return;

        IDanificavel d = alvo.GetComponentInParent<IDanificavel>();
        if (d != null && !d.EstaVivo)
            return;

        GameObject prefab = _atributos.ObterPrefabProjetil();
        if (prefab == null)
            return;

        GameObject inst = Object.Instantiate(prefab, _pontoDisparo.position, Quaternion.identity);
        var lanca = inst.GetComponent<LancaProjetil>();
        if (lanca != null)
        {
            lanca.velocidade = _atributos.ObterVelocidadeProjetil();
            lanca.Inicializar(
                alvo,
                _atributos.ObterDanoEfetivo(),
                _nomeOrigemLog,
                _atributos.ObterDefinicaoDot());
        }
    }
}
