# Guia do sistema de torres (Tower Defense)

Este documento descreve a implementação do sistema escalável de torres, a **Torre Goblin Lanceira** inicial e como trabalhar no Unity.

## Onde estão os ficheiros

### Scripts (Core / Manager / Controller / Dados)

| Área | Caminho |
|------|---------|
| Dados | `Assets/Scripts/Torres/Data/TorreDadosSO.cs` |
| Core | `Assets/Scripts/Torres/Core/` — `TorreBaseCore`, `TorreAtributosCore`, `TorreSistemaModificadoresCore`, `TorreAlvejamentoCore`, `TorreAtaqueCore`, `TorreAnimacaoDisparoBridge`, `IAlvoDeTorre` |
| Controller | `Assets/Scripts/Torres/Controller/TorreController.cs`, `TorreGoblinLanceiraController.cs` |
| Manager | `Assets/Scripts/Torres/Manager/TorresManager.cs` |

### Alterações em código existente

| Ficheiro | Alteração |
|----------|-----------|
| `Assets/Scripts/Core/MovimentoUnidade.cs` | `CalcularDistanciaRestanteNaRota()`, `IndiceWaypointAtual`, `TotalPontosNaRota` — prioridade de alvo das torres. |
| `Assets/Scripts/Racas/Goblins/LancaProjetil.cs` | Alvo morto/nulo, log opcional, `tempoVidaMaximo` opcional. |

### Prefabs e dados

| Recurso | Caminho |
|---------|---------|
| Prefab da torre | `Assets/Prefabs/Torres/TorreGoblinLanceira.prefab` |
| ScriptableObject de stats | `Assets/Prefabs/Torres/TorreGoblinLanceiraDados.asset` |
| Prefab torre fogo (mesma lógica lanceira) | `Assets/Prefabs/Torres/TorreGoblinFogo.prefab` + `TorreGoblinFogoDados.asset` |
| Prefab torre veneno (mesma lógica lanceira) | `Assets/Prefabs/Torres/TorreGoblinVeneno.prefab` + `TorreGoblinVenenoDados.asset` |
| Sprite usado (torre goblin) | `Assets/Sprites/Torres/TorreGoblinBesta.png` (nome do asset; substitui se tiveres arte “lanceira” dedicada) |
| Sprites fogo / veneno | `TorreGoblinFogo.png`, `TorreGoblinVeneno.png` |
| Projétil reutilizado | `Assets/Prefabs/Characters/Goblins/LancaProjetil.prefab` |

### Cena

- `Mapa_1`: objeto raiz **Torres** com `TorresManager` + instância de **TorreGoblinLanceira** (filho), posicionada para teste.

---

## O que foi reaproveitado do GoblinLanceiro

- **Mesmo prefab de projétil** `LancaProjetil` e a mesma chamada `Inicializar(Transform, int dano, string origem)`.
- **Mesmo conceito de `PontoDisparo`**: filho `Transform` com offset local alinhado ao do prefab `GoblinLanceiro` (~`2.75`, `0.75`, `0`).
- **Disparo imediato** quando não há Animator / trigger configurado — igual ao comportamento atual do `GoblinLanceiro` (o projeto ainda não tinha clips `.anim` / Animator para o lanceiro).

---

## Configurar a torre no Inspector

1. Abre o prefab `TorreGoblinLanceira`.
2. No componente **Torre Goblin Lanceira Controller**:
   - **Dados**: arrasta `TorreGoblinLanceiraDados.asset`.
   - **Ponto Disparo**: deve apontar para o filho `PontoDisparo` (já referenciado por defeito no prefab).
   - **Animator Opcional**: deixa vazio enquanto não houver animação; quando tiveres Animator, arrasta aqui.
   - **Referência Objetivo Trajeto Fallback**: opcional; se vazio, a torre tenta encontrar o objeto com tag **Castelo** para inimigos sem `MovimentoUnidade`.
   - **Procurar Castelo Por Tag**: ativo por defeito.

---

## Configurar o ScriptableObject `TorreGoblinLanceiraDados`

- **Raca**: `Goblin` (alinhado a `TipoRaca` do projeto).
- **Custo construção**: `0` (configurável).
- **Vida / dano / alcance / intervalo**: ajusta ao balanceamento.
- **Prefab Projetil**: `LancaProjetil` (já referenciado no asset criado).
- **Velocidade Projetil**: coerente com o prefab (ex.: `60`).
- **Layer Mask Alvos**: camada **Inimigo**; se estiver vazio no asset, o código usa automaticamente a máscara da layer `Inimigo`.
- **Nome Trigger Ataque**: vazio = disparo imediato; se preencheres (ex. `Atacar`), precisas de Animator com esse trigger e **Animation Event** no clip (ver abaixo).
- **Mostrar Gizmos**: desenha alcance e, em Play, linha ao alvo ao selecionar a torre.

---

## Animator e timing do disparo

1. Adiciona um **Animator** ao root da torre (ou ao filho do sprite).
2. Cria um parâmetro **Trigger** com o mesmo nome que colocaste em **Nome Trigger Ataque** nos dados.
3. No clip de ataque, no frame em que a lança deve sair, adiciona um **Animation Event** que chama a função **`AnimacaoEventoDisparo`** no objeto que tem o **TorreController** (o root da torre com o script).
4. Se o trigger estiver definido mas **não** houver evento, o projétil **não** será disparado pela animação (o cooldown já terá sido consumido — evita deixar triggers sem evento).

Sem Animator ou com trigger vazio, o fluxo é o mesmo que o **GoblinLanceiro**: projétil no mesmo instante em que o ataque é aceite.

---

## Configurar o projétil `LancaProjetil`

- No prefab, podes definir **`tempoVidaMaximo`** (>0) para destruir lanças perdidas após X segundos.
- **`registrarImpactoNoConsole`**: só ativa para debug pontual.

---

## Posição na cena e hierarquia

- Em **Mapa_1**, a hierarquia sugerida é: **Torres** (com `TorresManager`) → filho **TorreGoblinLanceira** (instância do prefab).
- Ajusta a **posição local** da torre sob **Torres** se quiseres cobrir melhor o percurso dos inimigos (valores atuais são apenas para teste).

---

## Testar alcance

1. Entra em Play com **Mapa_1**.
2. Seleciona a torre na hierarquia.
3. Com **Mostrar Gizmos** ativo nos dados, vês a esfera de alcance (wireframe) e o **PontoDisparo** (amarelo).

---

## Testar prioridade de alvo

Critério implementado:

1. Entre inimigos no alcance (layer **Inimigo**, `UnidadeController` vivo, facção inimiga), escolhe-se o que tem **menor** `CalcularDistanciaRestanteNaRota()` (definido em `MovimentoUnidade`).
2. Em empate, ganha o **mais próximo da torre** (menor distância euclidiana).

Para testar: spawna duas ondas ou coloca inimigos manualmente a diferentes distâncias ao longo da mesma rota e observa qual recebe o projétil primeiro.

---

## Testar animação e disparo

- **Sem Animator**: inicia Play; quando um inimigo entra no alcance, devem aparecer instâncias de `LancaProjetil` a mover-se para o alvo.
- **Com Animator**: confirma no **Animation** window que o evento chama `AnimacaoEventoDisparo` no GameObject certo.

---

## `TorresManager`

- **Registo**: cada `TorreController` regista-se no `Start` (usa `TorresManager.Instancia` ou procura um na cena uma vez).
- **Instanciar em runtime** (futura UI de construção):

```csharp
TorresManager.Instancia?.InstanciarTorre(prefabTorreGoblinLanceira, posicao, Quaternion.identity);
```

O prefab já deve ter dados e referências configurados; o registo ocorre no `Start` da torre.

---

## Escalabilidade

### Nova torre (outra raça ou tipo)

1. Duplica `TorreGoblinLanceiraDados` (ou cria novo **Create > TD > Torre Dados**).
2. Define `raca`, stats, prefab de projétil e layer mask.
3. Duplica o prefab da torre ou usa `TorreController` genérico com arte nova; mantém filho **PontoDisparo** onde o projétil nasce.
4. Opcional: controller fino específico (como `TorreGoblinLanceiraController`) só para validação ou lógica extra.

### Reutilizar outra unidade inimiga como referência visual

- Copia hierarquia / offsets de **PontoDisparo** e sprites do prefab da unidade; **não** adiciones `GoblinLanceiro`/`UnidadeController` na torre (facção e movimento são de unidade, não de estrutura).

### Novo tipo de projétil

1. Cria novo script (podes seguir o padrão de movimento + `IDanificavel` de `LancaProjetil`).
2. Referencia o novo prefab em **Prefab Projetil** no `TorreDadosSO` (ou estende `TorreAtaqueCore` no futuro para tipos múltiplos).
3. Para **object pool**, o ponto natural é substituir `Instantiate`/`Destroy` dentro de `TorreAtaqueCore` por serviços registados no `TorresManager`.

### Buffs e debuffs

- Usa `TorreSistemaModificadoresCore` na torre (`ModificadorTorreEntrada` com `TipoStatTorre` e valores aditivo/multiplicador).
- Chama métodos de recálculo quando implementares alteração de vida máxima em runtime (`TorreBaseCore.DefinirVidaMaxima` já existe).

### Inimigos especiais sem `MovimentoUnidade`

- Implementa `IAlvoDeTorre` num componente no mesmo objeto (ou pai) que o `UnidadeController`, definindo prioridade e transform de acerto.

---

## Resolução de problemas

- **Torre não atira**: confirma **Prefab Projetil**, **Ponto Disparo**, layer **Inimigo** nos inimigos, e alcance suficiente.
- **Prioridade estranha**: confirma que os inimigos têm `MovimentoUnidade` e rota definida pelo `WaveManager`; caso contrário usa-se o fallback pela distância ao **Castelo** (tag).
- **Cena não abre após merge manual**: abre o Mapa_1 no Unity para o serializador corrigir referências; verifica o **Console** por erros de YAML.

---

## Referência rápida — fluxo de ataque

1. `TorreAlvejamentoCore` faz `OverlapCircleNonAlloc` na posição da torre.
2. Filtra `UnidadeController` inimigos vivos; obtém prioridade via rota ou `IAlvoDeTorre`.
3. `TorreAtaqueCore` aplica cooldown e, se aplicável, `TorreAnimacaoDisparoBridge` dispara o trigger.
4. Projétil `LancaProjetil` segue o `Transform` do alvo e aplica dano via `IDanificavel`.
