# TEAM_HANDOFF — Rift Hunters AR

Fecha: 2026-10-13. Proyecto: `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR`. Unity 6000.3.16f1. Rama art-integration. Se conserva toda la base funcional para los compañeros; no se desarrolló gameplay durante este diagnóstico.

## Evidencia y alcance

IMPLEMENTADO distingue código existente de aspecto final. PROBADO EN ANDROID: SÍ significa funcionalidad reportada explícitamente por el usuario, no observación directa del agente. NO significa ausencia de confirmación específica, no fallo demostrado. Pruebas de Editor y compilación no equivalen a pruebas AR físicas. El reporte actual confirma cámara, planos, placement, portal, spawn, enemigos, apuntado, disparo, daño, score, vida, munición y oleadas. No afirma explícitamente Victory/Game Over/Retry/RELOAD ni cuatro impactos exactos.

Las rutas de ARCHIVOS son absolutas. Para elementos no implementados se cita el punto de extensión/carpeta existente, no un archivo inexistente. Código propio: PortalSpawnPoint, PlasmaWeapon, EnemyHealth, EnemyMovement, ScoreManager, PlayerHealth, EnemySpawner, ARPrototypeController y PrototypeBuilder. TutorialInfo/InputSystem_Actions/SampleScene son base de plantilla, no features nuevas del juego.

## Git y conservación

Origin fetch/push verificado exactamente https://github.com/Nazareth75/RiftHuntersAR.git. Fetch exitoso: origin/main y origin/ar-system; origin/art-integration no existía. Primer push falló porque Git Credential Manager no pudo ejecutarse (MSYS NtCreateDirectoryObject 0xC0000022); consulta directa no encontró credencial utilizable sin inicio interactivo. No se hizo push exitoso, merge, reset, force ni modificación de ar-system/main. Autenticación/push pendiente en terminal normal del usuario. Destino autorizado: origin/art-integration únicamente.

Cambios locales excluidos: ProjectSettings/UnityConnectSettings.asset activa m_Enabled de 0 a 1 (Unity Services; no requisito para AR local ni credencial privada literal, pero configuración opcional ajena al alcance; dejar sin commit). ProjectSettings/ProjectSettings.asset solo reordena las mismas referencias preloadedAssets; también se preserva sin incluir. No revertirlos.

No hay Library/Temp/Logs/Builds/obj/UserSettings/.vs/.idea, APK, SDK ni keystores rastreados. Cuatro preferencias de Assets/XR/UserSimulationSettings antes rastreadas pese al ignore se retiraron SOLO del índice en commit 8aa3181; siguen físicamente en el disco. No se borró gameplay ni ningún archivo local. Loaders y settings funcionales ARCore siguen versionados.

## PERSONA 1 — AR

### AR Foundation

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Packages\manifest.json`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Packages\packages-lock.json`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`

CÓMO FUNCIONA: AR Foundation 6.3.5; managers de sesión, cámara, planos, raycast y anclas.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Persona 1 conserva y amplía esta base; validar pausas/reanudación y pérdida de tracking.

### ARCore

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Packages\manifest.json`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Packages\packages-lock.json`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Settings\RiftXRSettings.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\XR\Loaders\ARCoreLoader.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\XR\Settings\ARCoreSettings.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\XR\Resources\ARCoreRuntimeSettings.asset`

CÓMO FUNCIONA: Proveedor ARCore 6.3.5 Android requerido; depth opcional, loader al inicio.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Comprobar compatibilidad en más teléfonos y reinstalación desde checkout limpio.

### XR Origin

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\Editor\PrototypeBuilder.cs`

CÓMO FUNCIONA: XROrigin, Camera Offset, referencia a AR Camera y CameraYOffset 0.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: No recrearlo; mantener referencias y escala en metros.

### AR Session

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: ARSession + ARInputManager; SessionTracking habilita placement/combate y tiempos de oleadas.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Manejar interrupciones y mensajes de permiso/compatibilidad más completos.

### Cámara AR

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Settings\Mobile_Renderer.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\Editor\PrototypeBuilder.cs`

CÓMO FUNCIONA: ARCameraManager, ARCameraBackground y TrackedPoseDriver Input System con pose HandheldARInputDevice; ARBackgroundRendererFeature URP.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Pruebas de orientación, permiso cámara y vuelta desde segundo plano.

### Detección de planos

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Portal\PlanePlaceholder.prefab`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: ARPlaneManager solicita planos horizontales; mesh temporal visible hasta colocar portal, luego oculta renderers manteniendo tracking.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Afinar feedback de superficie, políticas de seguimiento y plano apropiado.

### Raycast AR

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`

CÓMO FUNCIONA: ARRaycastManager.Raycast con PlaneWithinPolygon sobre posición táctil; verifica TrackingState.Tracking.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: No confundir con Physics.Raycast del disparo; ampliar UX de colocación.

### Placement y exclusión de UI

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: Lee primaryTouch.wasPressedThisFrame; EventSystem.RaycastAll descarta toques UI y evita colocar con SHOOT.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Reposicionamiento voluntario no implementado.

### Anchors

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`

CÓMO FUNCIONA: AttachAnchor sobre plano elegido; portal hijo del ancla. Enemigos hijos del mismo ancla al salir del portal.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Colocación reportada funcional; estabilidad prolongada/relocalización no confirmadas por prueba específica.

### Portal placement y SpawnPoint

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\AR\PortalSpawnPoint.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Portal\PortalPlaceholder.prefab`

CÓMO FUNCIONA: Un portal por partida; PortalSpawnPoint expone Transform serializado independiente de Visual.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Arte del portal corresponde a Persona 4; lógica AR posterior corresponde a Persona 1.

### Android relacionado con AR

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ProjectSettings\ProjectSettings.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Settings\RiftXRSettings.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\XR\Settings\ARCoreSettings.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Settings\Mobile_Renderer.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Settings\Mobile_RPAsset.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ProjectSettings\GraphicsSettings.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ProjectSettings\QualitySettings.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Packages\manifest.json`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Packages\packages-lock.json`

CÓMO FUNCIONA: IL2CPP ARM64, GLES3, min API25, target36, portrait, com.rifthunters.ar y pipeline Mobile_RPAsset.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: SDK local no se comparte por Git: cada compañero necesita Android Build Support y SDK/JDK/NDK válidos.

### Simulación de AR en editor PC

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Settings\RiftXRSettings.asset`

CÓMO FUNCIONA: Hay assets XR de simulación del paquete, pero RiftXRSettings solo registra Android; no hay entorno/sesión de PC validada.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Persona 1 decide si configura simulación; Play Windows puede mostrar avisos XR y pantalla negra. No cambiar aquí.

## PERSONA 2 — COMBATE/ENEMIGOS

### PlasmaWeapon y Shoot

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Combat\PlasmaWeapon.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`

CÓMO FUNCIONA: Shoot llama Fire con tracking real; botón registra/desregistra listener en OnEnable/OnDisable. Bloqueo por frame y cooldown 0,2 s.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Prueba específica de un toque=un disparo y balance en teléfonos adicionales.

### Raycast de disparo

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Combat\PlasmaWeapon.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Enemies\EnemyBasicPlaceholder.prefab`

CÓMO FUNCIONA: Ray desde centro del viewport, rango 20 m, Physics.Raycast único, ignora triggers; GetComponentInParent<EnemyHealth>.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: LayerMask específico/obstáculos del juego si Persona 2 lo requiere.

### Daño y cuatro impactos

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Combat\PlasmaWeapon.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyHealth.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Enemies\EnemyBasicPlaceholder.prefab`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\Editor\PrototypeBuilder.cs`

CÓMO FUNCIONA: 25 damage contra 100 HP; pruebas físicas de raycast en Editor dan 75/50/25/0. Logs por impacto; texto HP.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Daño Android reportado funcional; cuatro impactos exactos en el teléfono no se afirmaron explícitamente en el último reporte. Confirmar caso original de disparo letal.

### EnemyHealth y muerte

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyHealth.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Enemies\EnemyBasicPlaceholder.prefab`

CÓMO FUNCIONA: Awake inicializa maxHealth; TakeDamage reduce HP; Die emite Died una vez, desactiva y destruye en Play.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Animación/despawn visual a coordinar con Persona 4 sin duplicar daño.

### Movimiento horizontal

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyMovement.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Enemies\EnemyBasicPlaceholder.prefab`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\EnemySpawner.cs`

CÓMO FUNCIONA: Velocidad 0,25 m/s hacia cámara manteniendo altura; limita paso y se detiene a ~0,65 m; pausa sin tracking.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Código probado en Editor; reporte Android enumera enemigos pero no confirma movimiento por separado. Sin evitar obstáculos ni separación entre enemigos.

### Ataques enemigos

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyMovement.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\PlayerHealth.cs`

CÓMO FUNCIONA: Daño Basic 34, cooldown 2 s, no ataca jugador muerto y Running=false al Game Over.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Prueba Android individual de ataque/cooldown y balance pendiente de confirmar.

### Munición

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Combat\PlasmaWeapon.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: MagazineSize 30; -1 por disparo aceptado, también MISS; con 0 no dispara.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Respetar guardias al añadir armas; sin reservas/pickups aún.

### Recarga

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Combat\PlasmaWeapon.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: Reload llena a 30; botón temporal durante WAVE; reposición entre oleadas.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: El reporte no confirma explícitamente RELOAD físico; pruebas de lógica pasaron.

### Colliders / hit detection

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Enemies\EnemyBasicPlaceholder.prefab`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Combat\PlasmaWeapon.cs`

CÓMO FUNCIONA: Un CapsuleCollider técnico en HitCollider, separado de Visual; AimPoint hijo separado. Cápsula visual sin collider.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Persona 4 ajusta dimensiones al nuevo arte con Persona 2; no generar colliders duplicados en modelo importado.

### Spawn de enemigos

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\EnemySpawner.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\AR\PortalSpawnPoint.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Enemies\EnemyBasicPlaceholder.prefab`

CÓMO FUNCIONA: EnemySpawner instancia prefab en SpawnPoint y registra vivos en HashSet; AliveCount, EnemyKilled, Clear y StopMovement.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Instanciación directa; pooling y patrones de spawn corresponden a Persona 2/3.

### Configuración BASIC

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Enemies\EnemyBasicPlaceholder.prefab`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyHealth.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyMovement.cs`

CÓMO FUNCIONA: 100 HP, speed 0,25, damage 34, attackDistance 0,65, cooldown 2 serializados.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Balance final y validación específica de cuatro impactos.

### Configuración FAST/TANK

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyHealth.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyMovement.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\EnemySpawner.cs`

CÓMO FUNCIONA: Componentes reutilizables permiten variar maxHealth/speed/damage, pero no hay prefabs Fast/Tank ni assets de configuración.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Persona 2 crea variantes: referencia solicitada Fast 50 HP y Tank 250 HP. No existe implementación de estos tipos.

### Otras armas y Boss lógico

IMPLEMENTADO: NO

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Combat\PlasmaWeapon.cs`

CÓMO FUNCIONA: Solo Plasma; sin Electric, Launcher, weakpoints ni fases Boss.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Persona 2 implementa cuando equipo lo decida; Persona 4 solo modelos/VFX.

## PERSONA 3 — GAMEPLAY/UI LÓGICA

### ScoreManager

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\ScoreManager.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\EnemySpawner.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: EnemyKilled suma +100; Changed actualiza HUD; ResetScore en Retry. LÓGICA FUNCIONAL.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Sin persistencia ni multiplicadores.

### PlayerHealth

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\PlayerHealth.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyMovement.cs`

CÓMO FUNCIONA: 100 HP, TakeDamage, Die con evento único, Retry restaura. LÓGICA FUNCIONAL.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: No hay invulnerabilidad/armadura; balance y GameOver específico pendientes de confirmar en Android.

### Oleadas

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\EnemySpawner.cs`

CÓMO FUNCIONA: 3/5/7 Basic, spawn cada 2 s de tracking; espera AliveCount=0, pausa 3 s y siguiente oleada. LÓGICA FUNCIONAL.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Parámetros hardcoded dentro del coordinador; Persona 3 puede separar WaveManager/configuración sin reimplementar spawn.

### Estados de juego

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: Strings SCANNING, PORTAL_PLACED, COUNTDOWN, WAVE, WAVE_COMPLETE, NEXT_WAVE, VICTORY, GAME_OVER; coroutine coordina flujo. LÓGICA FUNCIONAL.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: No hay GameManager/FSM independiente; PORTAL_PLACED transitorio. Recorrido completo Victory/Retry no confirmado explícitamente.

### Victory

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: Tras limpiar Wave 3, bloquea arma, muestra VICTORY y RETRY.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Prueba física de final completo por confirmar; pantalla final es UI PLACEHOLDER TEMPORAL.

### Game Over

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\PlayerHealth.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\EnemySpawner.cs`

CÓMO FUNCIONA: Evento Died pone GAME_OVER, detiene coroutine y movimiento, bloquea arma y muestra RETRY.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Prueba física de derrota por confirmar; pantalla final temporal.

### HUD temporal

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: uGUI Text/Image/Button: algunos serializados en escena y otros creados en Awake con MakeLabel/MakeButton. UI PLACEHOLDER TEMPORAL.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Separar presentación/eventos con Persona 3 antes de sustituir UI creada dinámicamente; Persona 4 diseña gráficos.

### Contador de enemigos

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\EnemySpawner.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: AliveCount muestra enemigos vivos actuales, no enemigos futuros pendientes del spawn de la oleada. LÓGICA FUNCIONAL + texto temporal.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Confirmar contador físico y definir presentación final con Persona 3.

### Vida en HUD

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\PlayerHealth.cs`

CÓMO FUNCIONA: LIFE ceil(HP/(100/3))/3 y HP; flash rojo breve al daño. Lógica funcional, aspecto temporal textual.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Reemplazar texto por corazones gráficos; no es arte final.

### Score en HUD

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\ScoreManager.cs`

CÓMO FUNCIONA: Texto SCORE actualizado por evento Changed. Lógica funcional, arte temporal.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Gráficos finales y safe area.

### Wave display

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`

CÓMO FUNCIONA: Estado/WAVE n | ENEMIES count y feedback WAVE COMPLETE; Text temporal.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: UI final, localización y transiciones con Persona 3.

### Retry

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\PlayerHealth.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\EnemySpawner.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Game\ScoreManager.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Combat\PlasmaWeapon.cs`

CÓMO FUNCIONA: Limpia vivos sin puntuar, HP100, score0, ammo30, conserva portal y countdown Wave1.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Prueba física de repetición por confirmar; botón temporal.

### Main Menu

IMPLEMENTADO: NO

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`

CÓMO FUNCIONA: Existe carpeta Assets/Scenes/MainMenu, sin escena de menú ni navegación implementada.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Persona 3 hace navegación/estados; Persona 4 diseño y prefab visual.

## PERSONA 4 — ARTE/INTEGRACIÓN

### Estructura Art

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\Models`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\Textures`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\Materials`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\UI`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\UI_Source`

CÓMO FUNCIONA: Carpetas con metas; Models/Textures/UI/UI_Source sin assets finales.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Modelos, texturas, PNG/UI aprobados; conservar fuentes editables y GUID.

### Prefabs visuales

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Enemies\EnemyBasicPlaceholder.prefab`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Portal\PortalPlaceholder.prefab`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Portal\PlanePlaceholder.prefab`

CÓMO FUNCIONA: Visual intercambiable: cápsula Basic y cilindro portal; plano mesh temporal. Prefabs Weapons/Boss/Pickups aún vacíos.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: EnemyBasic_Final primero, luego Portal_Final, Fast/Tank/armas/Boss visuales.

### Materials

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\Materials\EnemyPlaceholder.mat`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\Materials\PortalPlaceholder.mat`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\Materials\PlanePlaceholder.mat`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Settings\Mobile_RPAsset.asset`

CÓMO FUNCIONA: Tres materiales URP Unlit temporales; enemigo naranja, portal azul, plano verde.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Paleta compartida grafito/metal/cian/rojo; pocas variantes y emisión ligera.

### Effects / VFX

IMPLEMENTADO: NO

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Effects`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`

CÓMO FUNCIONA: Carpeta preparada; X/flash textual del controlador no son VFX definitivos.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Particle System ligero para hit/death/portal; presupuesto de overdraw y pruebas.

### Audio

IMPLEMENTADO: NO

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Audio`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`

CÓMO FUNCIONA: Carpeta preparada; AudioListener en cámara, sin clips/AudioSource de juego.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: SFX aprobados/licenciados, mezcla y volúmenes móviles.

### UI visual / crosshair / botones

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\ARPrototypeController.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\UI`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Art\UI_Source`

CÓMO FUNCIONA: Mira +, botones rectangulares y texto temporal. No hay Crosshair_Final, ShootButton_Final ni HUD_Final.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: PNG transparentes, estados de botones, corazones, iconos, logo/menu; integración acordada con Persona 3.

### Placeholders reemplazables

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Enemies\EnemyBasicPlaceholder.prefab`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Prefabs\Portal\PortalPlaceholder.prefab`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\AR\PortalSpawnPoint.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyHealth.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Enemies\EnemyMovement.cs`

CÓMO FUNCIONA: Lógica en raíz; collider y AimPoint separados de Visual. SpawnPoint separado del portal visual.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: No reestructurar raíz; sustituir únicamente contenido Visual y acordar escala/collider.

### Documentación

IMPLEMENTADO: SÍ

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Documentation\PROJECT_STATUS.md`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Documentation\INTEGRATION_NOTES.md`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\PLAYTEST_GAMEPLAY.md`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\PROJECT_STATUS.md`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Documentation\TEAM_HANDOFF.md`

CÓMO FUNCIONA: Documentos de fases, build y contratos; algunos describen historial del MVP y no la versión actual.

PROBADO EN ANDROID: NO

QUÉ LE FALTA: Este handoff es referencia actual; registrar assets y pruebas visuales nuevas.

### Testing Android / build

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\Editor\PrototypeBuilder.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Tools\BuildAndroidPrototype.ps1`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\PLAYTEST_GAMEPLAY.md`

CÓMO FUNCIONA: APK Playtest construido, firma v2 verificada; tests de prefab/raycast/vida/movimiento en Editor. Usuario reporta cámara/planos/placement/portal/spawn/enemigos/apuntado/disparo/daño/score/vida/ammo/oleadas funcionando físicamente.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Registrar teléfono/FPS/temperatura/visibilidad; confirmar Victory, Game Over, Retry y recarga de forma específica. Nada de ello se asume.

### Integración y optimización

IMPLEMENTADO: PARCIAL

ARCHIVOS:
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scripts\Integration\Editor\PrototypeBuilder.cs`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Scenes\Gameplay\ARGame.unity`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Settings\Mobile_Renderer.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Assets\Settings\Mobile_RPAsset.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ProjectSettings\GraphicsSettings.asset`
- `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ProjectSettings\QualitySettings.asset`

CÓMO FUNCIONA: URP móvil, AR background, target 30 FPS, lógica desacoplada de hijo Visual. Helper genera placeholders, pruebas y APK; comando Create rechaza sobrescribir escena.

PROBADO EN ANDROID: SÍ

QUÉ LE FALTA: Perfilado de arte real, materiales compartidos, geometría/overdraw/atlas. SDK/toolchain locales no portables; cada PC configura Android Build Support.

## Contrato de sustitución visual (sin modificar lógica)

Jerarquía REAL actual:

```text
EnemyBasicPlaceholder [EnemyHealth + EnemyMovement en raíz]
├── HitCollider [CapsuleCollider técnico]
├── AimPoint
└── Visual
    └── TemporaryCapsule [Renderer, sin Collider]

PortalPlaceholder [PortalSpawnPoint en raíz]
├── SpawnPoint
└── Visual
    └── TemporaryPortalDisc [Renderer, sin Collider]
```

No existe hijo Logic separado, y no es necesario crearlo: la lógica está en la raíz y ya está desacoplada del arte. Conservar raíz, componentes, referencias serializadas, GUID/meta, HitCollider/AimPoint/SpawnPoint. Prefab importado de arte debajo de Visual; deshabilitar solo el render placeholder cuando el nuevo asset esté validado. No mover scripts al modelo FBX, no autogenerar collider del importador. Escala actual Basic ~0,6 m de alto; portal ~0,5 m de diámetro. Nuevo tamaño se acuerda antes de modificar collider. Los enemigos miran +Z con LookRotation; pivote de arte en pies/suelo. Esta compatibilidad está comprobada por inspección, no se hizo sustitución de arte hoy.

El HUD es menos desacoplado: vida/ammo/RETRY/RELOAD se crean en Awake dentro del controlador. Persona 4 prepara sprites/prefabs gráficos; Persona 3 expone presentación y eventos para integrarlos. No reescribir lógica ni añadir GameManager por cuenta de Persona 4.

## Herramientas de diseño encontradas

Inventario por registro de desinstalación de equipo/usuario, nombres de ejecutables en Program Files/Program Files (x86)/Local Programs y búsqueda en Escritorio/Descargas/Documentos. No equivale a búsqueda exhaustiva de todos los discos; portable en otro disco puede no aparecer.

| Herramienta | Resultado observado |
|---|---|
| LibreSprite | SÍ, portable: C:\\Users\\HP\\Desktop\\libresprite\\libresprite.exe. Metadata ProductVersion 1.1.0.0; carpeta contiene ZIP development. No se lanzó ni instaló hoy. |
| Blender | No localizado en los lugares revisados. |
| Aseprite | No localizado. |
| GIMP | No localizado. |
| Krita | No localizado. |
| Paint.NET | No localizado. |
| Blockbench/MeshLab/SketchUp/Maya/3ds Max/MagicaVoxel | No localizado en búsquedas realizadas. |
| Unity 6 / URP | Disponible, proyecto 6000.3.16f1, URP17.3.0. |
| Shader Graph | Disponible como dependencia instalada 17.3.0 (packages-lock). Sin shaders gráficos propios finales. |
| Particle System | Módulo built-in presente en manifest; utilizable sin instalar paquete. Sin efectos finales actuales. |
| ProBuilder | No está en manifest/lock; no se instala. |
| VFX Graph | No está en manifest/lock; no se instala. |

Unity ya permite primitives, Transform, jerarquías, MeshRenderer, materiales URP, Animator/Animation y Particle System. Eso sirve para una primera silueta angular sin instalación; no ofrece el flujo completo de modelado orgánico/UV/rigging de Blender. Shader Graph puede construir emisión/pulsos sencillos. Con GLES3 actual, priorizar Particle System y comprobar compatibilidad real antes de considerar VFX Graph/otro backend.

## Herramientas recomendadas y pipeline

3D final: Blender recomendado para modelado low-poly y, si hace falta, rigging/exportación FBX. NO localizado ni instalado; se requerirá autorización futura para instalarlo. Ruta de contingencia inmediata: ensamblar geometría angular con primitivas Unity, convertir a mesh/asset visual cuando se apruebe la silueta; no asumir ProBuilder disponible. No se modela nada en este diagnóstico.

2D: LibreSprite ya disponible para iconos, mira, botones y exportación PNG transparente. No es herramienta 3D. Para HUD suave usar PNG de referencia a resolución suficiente; no convertir automáticamente toda la dirección visual a pixel art.

Pipeline 3D: usuario + ChatGPT aprueban vista frontal/lateral/trasera, silueta, tamaño y detalle de EnemyBasic → fuente .blend fuera del runtime → modelo low-poly, UV sencilla si procede, 2–3 materiales compartidos → FBX (OBJ solo para mesh estática) → Assets/Art/Models con metas conservadas → materiales URP y prefab visual → hijo Visual del Basic actual → tests de escala, aim/collider sin duplicados, visibilidad AR y rendimiento Android → commit de un asset. No importar .blend como dependencia obligatoria para todos los compañeros; entregar FBX.

Pipeline 2D: diseño aprobado → PNG/.ase editable en fuente → LibreSprite (exportar PNG RGBA transparente; transparencia se solicita en el diseño, no se asume del JPG/concepto) → Assets/Art/UI_Source fuente y Assets/Art/UI PNG → Unity Texture Type Sprite (2D and UI), alpha, atlas si procede → prefab gráfico coordinado con Persona 3 → test táctil portrait/safe area → commit por conjunto pequeño.

Paleta OFICIAL: tecnología/jugador cian y azul eléctrico, enemigos naranja/rojo, estructuras grafito/negro/gris metálico, portal azul/cian dimensional. Low-poly sci-fi oscuro/futurista/angular, adulto, sin infantilización ni hiperrealismo. No se inventa otra dirección artística.

Presupuesto inicial PROPUESTO, no medido ni aprobado: Basic 1–3k triángulos, 2–3 materiales compartidos, texturas 512–1024 cuando necesarias, luces/emisión simples sin depender de bloom. Medir con hasta siete vivos; conservar objetivo 30 FPS y revisar overdraw de portal/partículas. Ajustar al teléfono real en vez de prometer cifras.

## Primer asset a diseñar

SOLO EnemyBasic_Final ahora. Preparar hoja de turnaround frontal/lateral/trasera del mismo enemigo de la Biblia visual, silueta clara, zonas de roca/grafito y núcleo rojo/naranja, tamaño en metros y versión sin fondo. Definir si la primera entrega será estática o animada; si animada, pose y partes móviles. Una imagen conceptual no es todavía un modelo 3D ni un FBX listo para Unity.

Orden después de Basic: Portal_Final → Crosshair_Final → ShootButton_Final → HUD_Final → EnemyFast/EnemyTank → armas → RiftGuardian visual. Se pueden diseñar referencias de esas piezas, pero no se implementan diez assets a la vez ni sus mecánicas. Para Boss visual reservar zonas nombradas WeakPoint_Left/Right/Core sin programar comportamiento ahora.

## Límite de responsabilidad vigente

Persona 1 continúa AR; Persona 2 combate/enemigos; Persona 3 GameManager/oleadas/lógica UI. Nosotros Persona 4: pipeline, arte, materiales, prefabs visuales, VFX/audio, integración y testing visual Android. Cualquier cambio estrictamente necesario de lógica para integrar arte se explica antes y se coordina. No se continúa gameplay tras este diagnóstico.

## Fuentes oficiales del pipeline

- [Unity 6.3 — Model Import Settings](https://docs.unity.com/en-us/engine/6000.3/manual/assets-and-media/asset-types/models/importing/fbximporter-model): escala 1 unidad = 1 metro y ajustes de importación.
- [Unity — Model formats](https://docs.unity.com/en-us/engine/6000.0/manual/assets-and-media/asset-types/models/creating-dccassets/3d-formats): FBX/OBJ y fuentes DCC.
- [LibreSprite — repositorio oficial](https://github.com/LibreSprite/LibreSprite): editor de sprites/animación 2D.
- [Unity — Particle effects](https://docs.unity.com/en-us/engine/6000.6/manual/visual-effects/particle-systems): sistemas de partículas. Versión de documentación distinta; disponibilidad aquí comprobada localmente en manifest.

