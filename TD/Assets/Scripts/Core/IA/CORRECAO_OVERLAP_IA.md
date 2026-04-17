# Correção: Sobreposição de Unidades em Combate

## Problema

Unidades inimigas (ex: AnaoGuerreiro) ficavam completamente sobrepostas com
unidades do jogador (ex: ElfoAssassino) durante o combate, em vez de parar
a uma distância adequada.

---

## Causa Raiz

O prefab `AnaoGuerreiro` tinha **dois componentes `SistemaIAUnidade`** ao mesmo
tempo. O segundo componente era inválido (`alcanceAtaque = 0`), fazendo a
unidade nunca parar de avançar. O componente duplicado foi removido.

---

## Correções em `SistemaIAUnidade.cs`

### 1. Freio imediato no FixedUpdate
**Onde:** início de `MoverParaAlvo()`

Ao detectar que a distância ao alvo já está dentro do `alcanceAtaque`, a
velocidade é zerada e o método retorna imediatamente. Sem isso, o movimento
continuava até o próximo `Update()` processar a distância, causando overshoot
de até 0.5u quando ambas as unidades se aproximavam ao mesmo tempo.

---

### 2. Repulsão mínima do inimigo
**Onde:** método privado `CalcularRepulsaoDeInimigo()`

Quando a unidade está mais próxima do alvo do que `alcanceAtaque * 0.65`
(distância mínima melee), uma força a empurra de volta. Isso impede que
empurrões de aliados joguem a unidade dentro do inimigo.

---

### 3. Anti-oscilação (ping-pong Atacando↔Perseguindo)
**Onde:** FixedUpdate caso `Atacando` e `MoverParaAlvo()`

A força de separação de aliados, se apontar na direção oposta ao alvo, causava
a unidade sair do alcance de ataque e voltar para o estado `Perseguindo`,
gerando tremor visual. A correção remove o componente "para trás" (em relação
ao alvo) da força de separação — aliados só se empurram lateralmente, nunca
para longe do alvo.

---

### 4. Separação kinematic (estado Combate)
**Onde:** `AplicarSeparacaoKinematica()`

No estado `Combate`, o Rigidbody é Kinematic e a física não resolve sobreposições
automaticamente. Foi adicionada:
- Escala correta da força de separação entre aliados
- Direção determinística baseada no Instance ID para garantir que dois aliados
  sobrepostos sempre se separem em direções opostas
- Repulsão do inimigo aplicada também no modo kinematic

---

### 5. Clamp do destino por slot
**Onde:** `MoverParaAlvo()`, cálculo de `destino`

Cada unidade aponta para um "slot" ao redor do alvo (ângulo baseado no Instance
ID) para não convergirem todas no mesmo ponto. O offset do slot podia colocar
o `destino` fora do `alcanceAtaque`, fazendo a unidade parar ali sem nunca
entrar em `Atacando` — gerando tremor. O fix clamp o `destino` para sempre
ficar dentro do raio `alcanceAtaque * 0.85` a partir do alvo.

---

## Arquivos Modificados

| Arquivo | O que mudou |
|---|---|
| `Assets/Scripts/Core/IA/SistemaIAUnidade.cs` | 5 correções descritas acima |
| `Assets/Prefabs/Characters/Anoes/AnaoGuerreiro.prefab` | Removido componente `SistemaIAUnidade` duplicado |
