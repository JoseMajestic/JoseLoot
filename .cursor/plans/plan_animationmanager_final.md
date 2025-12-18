# Plan Final para AnimationManager

## Objetivo
Crear un `AnimationManager` que gestione las animaciones de jugador y enemigo por estado (Idle, Attack, Defense, KO), con clips, controllers y objetos con SpriteRenderer. Integrarlo en `CombatManager` para que las animaciones se ejecuten en el orden correcto con duración fija de 2 segundos.

## Archivos a crear/modificar
- `Assets/Scripts/AnimationManager.cs` (nuevo)
- `Assets/Scripts/CombatManager.cs` (modificar: separar textos y agregar animaciones)
- `Assets/Scripts/BattleManager.cs` (modificar: pasar índice de enemigo)

## Estructura de datos

### AnimationStateConfig
```csharp
[System.Serializable]
public class AnimationStateConfig
{
    public AnimationClip clip;
    public RuntimeAnimatorController controller;
    public GameObject spriteRendererObject; // Objeto con SpriteRenderer
}
```

### EnemyAnimationSet
```csharp
[System.Serializable]
public class EnemyAnimationSet
{
    public AnimationStateConfig idle;
    public AnimationStateConfig attack;
    public AnimationStateConfig defense;
    public AnimationStateConfig ko;
    public Sprite enemySprite; // Sprite único para este enemigo (usado en todos los estados)
}
```

### AnimationManager
- **Jugador:**
  - `AnimationStateConfig playerIdle`
  - `AnimationStateConfig playerAttack`
  - `AnimationStateConfig playerDefense`
  - `AnimationStateConfig playerKO`
  - `Sprite playerSprite` (opcional, fijo para todo el combate)

- **Enemigos:**
  - `EnemyAnimationSet[] enemyAnimationSets` (alineado con array de enemigos del BattleManager)

## API del AnimationManager

### Métodos públicos:
- `void InitializeForCombat(int enemyIndex)` - Inicializa animaciones para un combate específico
- `IEnumerator PlayPlayerAnimation(AnimationState state)` - Reproduce animación del jugador (2 segundos)
- `IEnumerator PlayEnemyAnimation(AnimationState state)` - Reproduce animación del enemigo (2 segundos)
- `void SetPlayerIdle()` - Establece estado Idle del jugador
- `void SetEnemyIdle()` - Establece estado Idle del enemigo

### Enum AnimationState:
- `Idle`
- `Attack`
- `Defense`
- `KO`

## Flujo de integración en CombatManager

### Modificar ProcessNormalAttack:

**Cuando el jugador ataca (isPlayer = true):**
1. Mostrar texto del ataque (solo nombre): `FormatText(combatTexts.playerAttack, attack.attackName)`
2. `yield return animationManager.PlayPlayerAnimation(AnimationState.Attack)`
3. Calcular daño
4. Mostrar texto de daño: `FormatText(combatTexts.enemyReceivesDamage, currentEnemy.enemyName, damage)`
5. `yield return animationManager.PlayEnemyAnimation(AnimationState.Defense)`
6. Actualizar HP del enemigo

**Cuando el enemigo ataca (isPlayer = false):**
1. Mostrar texto del ataque (solo nombre): `FormatText(combatTexts.enemyAttack, currentEnemy.enemyName, attack.attackName)`
2. `yield return animationManager.PlayEnemyAnimation(AnimationState.Attack)`
3. Calcular daño
4. Mostrar texto de daño: `FormatText(combatTexts.playerReceivesDamage, damage)`
5. `yield return animationManager.PlayPlayerAnimation(AnimationState.Defense)`
6. Actualizar HP del jugador

### Modificar ProcessBasicAttack:
Mismo flujo pero sin nombre de ataque (solo "Ataque básico")

### Modificar StartCombat:
- Llamar a `animationManager.InitializeForCombat(enemyIndex)` pasando el índice del enemigo
- Establecer estado inicial Idle para ambos: `SetPlayerIdle()`, `SetEnemyIdle()`

### Modificar OnPlayerVictory/OnPlayerDefeat:
- `yield return animationManager.PlayEnemyAnimation(AnimationState.KO)` o `PlayPlayerAnimation(AnimationState.KO)`

## Implementación de PlayAnimation

1. Obtener la configuración del estado solicitado
2. Si hay SpriteRenderer, asignar el sprite (jugador o enemigo según corresponda)
3. Si hay AnimatorController, asignarlo al Animator del GameObject
4. Si hay AnimationClip, reproducirlo en el Animator
5. Esperar 2 segundos fijos: `yield return new WaitForSeconds(2f)`
6. Volver automáticamente a Idle cuando termine

## Detalles técnicos

- **Duración:** Todas las animaciones duran exactamente 2 segundos (no usar clip.length)
- **SpriteRenderer:** Buscar componente en el GameObject asignado o en sus hijos
- **Animator:** Buscar componente en el GameObject asignado o en sus hijos, crear si no existe
- **Sprite del jugador:** Asignar una vez en `InitializeForCombat` y mantener fijo
- **Sprite del enemigo:** Asignar desde el array según el índice del enemigo en `InitializeForCombat`
- **Índice de enemigo:** Modificar `BattleManager.OnBattleButtonClicked()` para pasar el índice a `CombatManager.StartCombat(enemy, enemyIndex)`

## Validaciones

- Si falta clip/controller/spriteRenderer, log warning y continuar sin animación (esperar 2 segundos igualmente)
- Si falta configuración de enemigo para un índice, log error y usar Idle por defecto
- Asegurar que Idle sea el estado por defecto cuando no hay animación activa
- Verificar que el array de enemyAnimationSets tenga la misma longitud que enemyButtons del BattleManager

## Textos nuevos en CombatTexts

Necesitarás agregar estos textos (o modificar los existentes):
- `playerAttack` - Solo nombre del ataque del jugador
- `enemyReceivesDamage` - Daño recibido por el enemigo
- `enemyAttack` - Solo nombre del ataque del enemigo  
- `playerReceivesDamage` - Daño recibido por el jugador


