# Plan para AnimationManager - Actualizado

## Objetivo
Crear un `AnimationManager` que gestione las animaciones de jugador y enemigo por estado (Idle, Attack, Defense, KO), con clips, controllers y objetos con SpriteRenderer. Integrarlo en `CombatManager` para que las animaciones se ejecuten en el orden correcto: texto ataque → animación ataque → idle → texto daño → animación defensa → idle.

## Archivos a crear/modificar
- `Assets/Scripts/AnimationManager.cs` (nuevo)
- `Assets/Scripts/CombatManager.cs` (modificar: integrar animaciones en flujo de combate)
- `Assets/Scripts/BattleManager.cs` (modificar: pasar índice de enemigo al iniciar combate)

## Estructura de datos

### AnimationStateConfig
- `AnimationClip clip`
- `RuntimeAnimatorController controller`
- `GameObject spriteRendererObject` (objeto con SpriteRenderer)
- `Sprite sprite` (opcional, para cambiar sprite del SpriteRenderer)

### EnemyAnimationSet
- Array de `AnimationStateConfig` para los 4 estados (Idle, Attack, Defense, KO)
- `Sprite enemySprite` (sprite específico de este enemigo)

### AnimationManager
- **Jugador:**
  - `AnimationStateConfig playerIdle`
  - `AnimationStateConfig playerAttack`
  - `AnimationStateConfig playerDefense`
  - `AnimationStateConfig playerKO`
  - `Sprite playerSprite` (opcional, único para todo el combate)

- **Enemigos:**
  - `EnemyAnimationSet[] enemyAnimationSets` (alineado con array de enemigos del BattleManager)
  
- **Referencias:**
  - `Canvas animationsCanvas` (opcional, para buscar objetos dentro del canvas de animaciones)

## API del AnimationManager

### Métodos públicos:
- `void InitializeForCombat(int enemyIndex)` - Inicializa animaciones para un combate específico (carga sprite del enemigo)
- `IEnumerator PlayPlayerAnimation(AnimationState state)` - Reproduce animación del jugador y espera a que termine
- `IEnumerator PlayEnemyAnimation(AnimationState state)` - Reproduce animación del enemigo y espera a que termine
- `void SetPlayerState(AnimationState state)` - Cambia estado del jugador inmediatamente (sin esperar)
- `void SetEnemyState(AnimationState state)` - Cambia estado del enemigo inmediatamente (sin esperar)

### Enum AnimationState:
- `Idle`
- `Attack`
- `Defense`
- `KO`

## Flujo de integración en CombatManager

### Modificar ProcessNormalAttack y ProcessBasicAttack:

**Cuando el jugador ataca:**
1. Mostrar texto del ataque (solo nombre, sin daño)
2. `yield return animationManager.PlayPlayerAnimation(AnimationState.Attack)`
3. Calcular daño
4. Mostrar texto de daño recibido por enemigo
5. `yield return animationManager.PlayEnemyAnimation(AnimationState.Defense)`

**Cuando el enemigo ataca:**
1. Mostrar texto del ataque (solo nombre, sin daño)
2. `yield return animationManager.PlayEnemyAnimation(AnimationState.Attack)`
3. Calcular daño
4. Mostrar texto de daño recibido por jugador
5. `yield return animationManager.PlayPlayerAnimation(AnimationState.Defense)`

### Modificar StartCombat:
- Llamar a `animationManager.InitializeForCombat(enemyIndex)` pasando el índice del enemigo seleccionado
- Establecer estado inicial Idle para ambos

### Modificar OnPlayerVictory/OnPlayerDefeat:
- Llamar a `PlayPlayerAnimation(AnimationState.KO)` o `PlayEnemyAnimation(AnimationState.KO)` según corresponda

## Implementación de PlayAnimation

1. Obtener la configuración del estado solicitado
2. Si hay SpriteRenderer, asignar el sprite si está configurado
3. Si hay AnimatorController, asignarlo al Animator del GameObject
4. Si hay AnimationClip, reproducirlo en el Animator
5. Esperar a que la animación termine (usar duración del clip o evento de Animator)
6. Volver automáticamente a Idle cuando termine

## Detalles técnicos

- **Detección de fin de animación:** Usar `AnimationClip.length` o eventos de Animator
- **SpriteRenderer:** Buscar componente en el GameObject asignado o en sus hijos
- **Animator:** Buscar componente en el GameObject asignado o en sus hijos, crear si no existe
- **Canvas de animaciones:** Si se asigna, buscar objetos dentro de ese canvas; si no, buscar en toda la escena
- **Índice de enemigo:** Modificar `BattleManager.OnBattleButtonClicked()` para pasar el índice del enemigo a `CombatManager.StartCombat(enemy, enemyIndex)`

## Validaciones

- Si falta clip/controller/spriteRenderer, log warning y continuar sin animación
- Si falta configuración de enemigo para un índice, usar configuración por defecto o log error
- Asegurar que Idle sea el estado por defecto cuando no hay animación activa
- Verificar que el array de enemyAnimationSets tenga la misma longitud que enemyButtons del BattleManager



