using UnityEngine;

/// <summary>
/// Se existir Animator e nome do trigger nos dados, usa SetTrigger e espera evento na animação.
/// Caso contrário, retorna false e o ataque dispara no mesmo frame (como GoblinLanceiro).
/// </summary>
public class TorreAnimacaoDisparoBridge
{
    private Animator _animator;
    private TorreDadosSO _dados;

    public void Configurar(Animator animator, TorreDadosSO dados)
    {
        _animator = animator;
        _dados = dados;
    }

    /// <summary>true = disparo ficou pendente até o Animation Event chamar <see cref="AnimacaoEventoDisparo"/>.</summary>
    public bool TentarIniciarAtaquePorAnimacao()
    {
        if (_animator == null || _dados == null || string.IsNullOrEmpty(_dados.nomeTriggerAtaque))
            return false;

        _animator.SetTrigger(_dados.nomeTriggerAtaque);
        return true;
    }
}
