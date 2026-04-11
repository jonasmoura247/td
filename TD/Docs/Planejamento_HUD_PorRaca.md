# Planejamento — HUD por Raça

## Contexto

O jogo possui 4 raças jogáveis (Goblin, Humano, Elfo, Anão).  
Cada raça deve ter sua própria HUD de torres e de relíquias ao entrar em partida.  
A raça ativa é lida de `PerfilJogadorAtivo.Raca` (já existe e está populado no início da cena).

---

## Como vai funcionar

A abordagem é a mais simples possível:

1. Na cena **Mapa_1**, criar **4 GameObjects de HUD de torres** (um por raça) e **4 de relíquias**, todos desativados por padrão.
2. Um script novo `GerenciadorHUDRaca` lê `PerfilJogadorAtivo.Raca` no `Start()` e ativa **apenas o par correto** (torres + relíquias da raça).
3. Cada HUD já vem pré-configurado no Inspector com os prefabs e dados da sua raça — nenhuma mudança de lógica nos scripts existentes.

Nenhum script existente (`ControladorHUDConstrucaoTorres`, `ControladorHUDColocacaoUnidades`) precisa ser alterado.

---

## Estrutura de HierArquia na Cena (Mapa_1)

```
Canvas (HUD)
├── HUD_Compartilhado          ← ouro, HP castelo, timer (igual para todas as raças)
│
├── HudTorresGoblin            ← desativado por padrão
│   ├── ControladorHUDConstrucaoTorres   [slots configurados com torres goblin]
│   └── ControladorHUDColocacaoUnidades  [slots configurados com unidades goblin]
│
├── HudTorresHumanos           ← desativado por padrão
│   ├── ControladorHUDConstrucaoTorres   [slots configurados com torres humanas]
│   └── ControladorHUDColocacaoUnidades  [slots configurados com unidades humanas]
│
├── HudTorresElfos             ← desativado por padrão
│   ├── ControladorHUDConstrucaoTorres   [slots configurados com torres élfica]
│   └── ControladorHUDColocacaoUnidades  [slots configurados com unidades élfica]
│
├── HudTorresAnoes             ← desativado por padrão
│   ├── ControladorHUDConstrucaoTorres   [slots configurados com torres anãs]
│   └── ControladorHUDColocacaoUnidades  [slots configurados com unidades anãs]
│
├── ReliquiasGoblinHud         ← desativado por padrão
│   └── SalaoReliquiaGoblinUI  [já existe, só mover para cá]
│
├── ReliquiasHumanoHud         ← desativado por padrão
│   └── SalaoReliquiaHumanoUI  [duplicar o script goblin, renomear]
│
├── ReliquiasElfoHud           ← desativado por padrão
│   └── SalaoReliquiaElfoUI
│
└── ReliquiasAnaoHud           ← desativado por padrão
    └── SalaoReliquiaAnaoUI
```

---

## Tarefas

### Etapa 1 — Script GerenciadorHUDRaca

**Criar:** `Assets/Scripts/HUD/GerenciadorHUDRaca.cs`

Responsabilidade: no `Start()`, ler a raça do perfil ativo e ativar o par de GameObjects correto.

```csharp
// Estrutura esperada do script:
public class GerenciadorHUDRaca : MonoBehaviour
{
    [Header("HUDs de Torres por Raça")]
    [SerializeField] private GameObject hudTorresGoblin;
    [SerializeField] private GameObject hudTorresHumano;
    [SerializeField] private GameObject hudTorresElfo;
    [SerializeField] private GameObject hudTorresAnao;

    [Header("HUDs de Relíquias por Raça")]
    [SerializeField] private GameObject hudReliquiasGoblin;
    [SerializeField] private GameObject hudReliquiasHumano;
    [SerializeField] private GameObject hudReliquiasElfo;
    [SerializeField] private GameObject hudReliquiasAnao;

    private void Start()
    {
        // Desativa todos antes de ativar o correto
        // Lê PerfilJogadorAtivo.Raca
        // Ativa o par (torres + relíquias) correspondente
    }
}
```

**Regras:**
- Todos os HUDs começam desativados no prefab/cena (inactive).
- O `GerenciadorHUDRaca` ativa apenas 1 HUD de torres + 1 HUD de relíquias.
- Se `PerfilJogadorAtivo` não tiver perfil ativo, ativar Goblin como fallback e logar warning.

---

### Etapa 2 — Criar os GameObjects na cena

**Na cena Mapa_1:**

- [ ] Criar `HudTorresGoblin` (filho do Canvas)
  - Mover/copiar o `ControladorHUDConstrucaoTorres` existente para dentro dele
  - Mover/copiar o `ControladorHUDColocacaoUnidades` existente para dentro dele
  - Configurar slots com prefabs/dados dos **Goblins**
  - Desativar o GameObject

- [ ] Criar `HudTorresHumanos` (filho do Canvas)
  - Duplicar a estrutura acima
  - Reconfigurar slots com prefabs/dados dos **Humanos**
  - Desativar o GameObject

- [ ] Criar `HudTorresElfos` (filho do Canvas)
  - Duplicar a estrutura acima
  - Reconfigurar slots com prefabs/dados dos **Elfos**
  - Desativar o GameObject

- [ ] Criar `HudTorresAnoes` (filho do Canvas)
  - Duplicar a estrutura acima
  - Reconfigurar slots com prefabs/dados dos **Anões**
  - Desativar o GameObject

---

### Etapa 3 — HUDs de Relíquias

O script `SalaoReliquiaGoblinUI` já funciona e é genérico (lê de `ReliquiasManager`).  
Não é necessário criar scripts separados — basta **duplicar o GameObject** e renomear.

- [ ] Criar `ReliquiasGoblinHud` com `SalaoReliquiaGoblinUI` (já existe)
- [ ] Criar `ReliquiasHumanoHud` com **cópia do mesmo script** (mesma lógica, visual diferente)
- [ ] Criar `ReliquiasElfoHud`
- [ ] Criar `ReliquiasAnaoHud`

> **Nota:** Se as relíquias forem visualmente idênticas entre raças, um único `ReliquiasHud` serve — ative/desative conforme necessário. Só criar 4 separados se o visual ou layout for diferente por raça.

---

### Etapa 4 — Conectar no Inspector

- [ ] Adicionar `GerenciadorHUDRaca` em um GameObject na cena (ex: `HUD_Manager`)
- [ ] Arrastar cada HUD para o campo correspondente no Inspector
- [ ] Verificar que todos os HUDs de torres e relíquias estão **desativados** antes de entrar no Play Mode

---

### Etapa 5 — Teste

- [ ] Entrar em partida com Goblin → só `HudTorresGoblin` e `ReliquiasGoblinHud` aparecem
- [ ] Entrar em partida com Humano → só `HudTorresHumanos` e `ReliquiasHumanoHud` aparecem
- [ ] Entrar em partida com Elfo → só `HudTorresElfos` e `ReliquiasElfoHud` aparecem
- [ ] Entrar em partida com Anão → só `HudTorresAnoes` e `ReliquiasAnaoHud` aparecem
- [ ] Testar que torres e unidades corretas aparecem nos slots de cada HUD

---

## Dependências

| Dependência | Estado |
|---|---|
| `PerfilJogadorAtivo.Raca` | Já existe e é populado antes da cena de jogo |
| `ControladorHUDConstrucaoTorres` | Já existe — não precisa alterar |
| `ControladorHUDColocacaoUnidades` | Já existe — não precisa alterar |
| `SalaoReliquiaGoblinUI` | Já existe — reutilizar para os outros |
| Prefabs de torres por raça | Precisam existir configurados nos `DadosSlotConstrucaoTorre` |
| Prefabs de unidades por raça | Precisam existir configurados nos `DadosSlotConstrucaoUnidade` |

---

## Ordem de Implementação Recomendada

```
1. Criar GerenciadorHUDRaca.cs
2. Montar HudTorresGoblin na cena e testar com Goblin
3. Duplicar para as outras 3 raças e configurar slots
4. Montar os 4 ReliquiasHud (duplicar GameObject)
5. Conectar tudo no GerenciadorHUDRaca via Inspector
6. Rodar testes com cada raça
```

---

## O que NÃO precisa ser feito

- Nenhuma alteração em `ControladorHUDConstrucaoTorres` ou `ControladorHUDColocacaoUnidades`
- Nenhum sistema de carregamento dinâmico de assets
- Nenhum ScriptableObject novo (opcional para o futuro)
- Nenhuma mudança em `WaveManager`, `GameManager` ou sistemas de combate
