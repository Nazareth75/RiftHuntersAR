# Fase 2: integración visual — Rift Hunters AR

Fecha: 14 de octubre de 2026. Unity 6000.3.16f1, Android, URP 17.3.0.

## Rama y protección

- Rama de trabajo: `integracion-visual`, creada desde `diseno-visual` en `32e897591b6345c2a125ce48a2faaf71a667055c`.
- Remoto: `https://github.com/Nazareth75/RiftHuntersAR.git`.
- Destino exclusivo del push: `origin/integracion-visual`.
- No se hizo merge, reset destructivo, force push ni modificación de main, art-integration o diseno-visual.
- Los cambios locales previos de ProjectSettings/UnityConnectSettings.asset y ProjectSettings/ProjectSettings.asset quedan fuera de los commits de integración.
- No se construyó ni reemplazó ninguna APK. Los dos APK anteriores son de la fase funcional.

## Inspección y fuentes

Fuentes 2D: `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\modelos 2d\RiftHunters_UI_Separado`.

Fuentes 3D: `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\modelos 3d\Paquete3D\ArtExports`.

Se encontraron 36 PNG finales y ocho FBX finales. Los .blend, renders, ZIP y scripts de producción continúan fuera de Assets. Las fuentes externas no se editaron ni borraron. Las 44 copias importadas tienen SHA256 idéntico a sus fuentes; véase `ArtRenders/VisualIntegration/Assets_SHA256.csv`.

La estructura existente tenía Art/Materials, carpetas de prefabs para enemigos, armas, boss y portal, ARGame, scripts separados de AR/Combat/Enemies/Game/Integration y HUD provisional creado parcialmente en runtime. Se reutilizó esa organización, agregando subcarpetas Approved para distinguir las copias finales, Scripts/UI/Visual para los adaptadores y Prefabs/UI para el HUD. No se reestructuraron los sistemas del equipo ni se reemplazó TEAM_HANDOFF.md.

## 3D y prefabs

Importados en `Assets/Art/Models/Approved/`:

| FBX | Altura real importada | Estado / prefab |
|---|---:|---|
| EnemyBasic_Final.fbx | 0.60 m | Visual del prefab funcional Enemies/EnemyBasicPlaceholder.prefab |
| EnemyFast_Final.fbx | 0.52 m | Enemies/EnemyFast_Visual.prefab; comportamiento pendiente Persona 2 |
| EnemyTank_Final.fbx | 0.95 m | Enemies/EnemyTank_Visual.prefab; comportamiento pendiente Persona 2 |
| RiftGuardian_Final.fbx | 1.85 m | Boss/RiftGuardian_Visual.prefab; lógica pendiente Persona 2/3 |
| Portal_Final.fbx | 1.25 m | Visual del prefab funcional Portal/PortalPlaceholder.prefab |
| PlasmaBlaster_Final.fbx | 0.30 m | Weapons/PlasmaBlaster_Visual.prefab y representación cosmética bajo AR Camera |
| ElectricCannon_Final.fbx | 0.36 m | Weapons/ElectricCannon_Visual.prefab; lógica pendiente Persona 2 |
| EnergyLauncher_Final.fbx | 0.42 m | Weapons/EnergyLauncher_Visual.prefab; lógica pendiente Persona 2 |

Rutas de prefabs relativas a `Assets/Prefabs/`. Se mantuvieron las alturas aprobadas con escala 1, importación bakeAxisConversion y un giro de 180° en el contenedor visual para convertir el frente importado -Z en +Z de Unity. El modelo se centró en X/Z y se desplazó hasta bounds.min.y = 0 para apoyar en el suelo. No se modificó la geometría del FBX. El viewmodel de plasma lleva una escala exclusivamente visual 0.30 y posición local (0.095, -0.17, 0.33) bajo la cámara; no modifica el raycast central.

Basic conserva EnemyHealth, EnemyMovement, sus valores, AimPoint y su único CapsuleCollider funcional. La cápsula antigua queda desactivada dentro de Visual. El modelo es más ancho que el collider original: ajustar el área de impacto si el equipo lo desea es trabajo posterior de Persona 2; no se amplió durante esta fase.

Portal conserva root, PortalSpawnPoint y la referencia al hijo SpawnPoint en (0,0,0). El disco antiguo queda desactivado. Las animaciones y la orientación hacia cámara actúan sobre Visual; el root técnico, el anchor y SpawnPoint no dependen de la geometría.

Guardian conserva los objetos separados WeakPoint_Left, WeakPoint_Core y WeakPoint_Right, así como veinte grupos FloatingFragment. Su prefab no tiene IA, fases, vida o ataques nuevos. No está añadido a las oleadas existentes.

## Materiales

Ubicación: `Assets/Art/Materials/`.

| Material | Shader / finalidad |
|---|---|
| M_Enemy_MetalDark | Universal Render Pipeline/Lit; grafito |
| M_Enemy_MetalLight | URP/Lit; placas grises |
| M_Enemy_Energy | URP/Lit; emisión naranja moderada |
| M_Enemy_Energy_Red | URP/Lit; variante roja Guardian |
| M_Tech_MetalDark | URP/Lit; tecnología grafito |
| M_Tech_MetalLight | URP/Lit; tecnología gris |
| M_Player_Energy | URP/Lit; emisión azul/cian |
| M_Rift_Metal | URP/Lit; fragmentos del portal |
| M_Rift_Energy | URP/Lit; emisión azul, doble cara para la espiral |
| M_AR_Hologram | RiftHunters/PlaneHologram; cian semitransparente con rejilla |

Los materiales energéticos activan _EMISSION y RealtimeEmissive para evitar que el validador URP descarte el keyword. Los materiales se comparten por familia. Una luz direccional sin sombras y ambiente plano hacen visibles los modelos en AR; no hay luces por partícula ni dependencia de Bloom.

## 2D y configuración

Los 36 PNG están en `Assets/Art/UI/Approved/`: boss_health_bar, btn_ajustes, btn_disparar, btn_disparar_pressed, btn_disparar_disabled, btn_iniciar, btn_recargar, btn_recargar_pressed, btn_reintentar, btn_salir, crosshair_normal, crosshair_hit, fondo_menu_principal, heart_full, heart_empty, heart_damaged, icon_advertencia, icon_curacion, icon_electrica, icon_enemy, icon_energia_pesada, icon_pausa, icon_plasma, icon_portal, logo_rift_hunters_ar, msg_colocar_portal, msg_enemigo_eliminado, msg_escanear, msg_prepara_jefe, panel_ammo, panel_enemy_counter, panel_score, panel_wave, weakpoint_core, weakpoint_left y weakpoint_right.

Sprite, alpha de entrada, alphaIsTransparency, FullRect, Bilinear, sin mipmaps, compresión Uncompressed y máximo 1024. Los cuatro msg_* usan un único sub-sprite con los 32 píxeles inferiores excluidos para ocultar el nombre de archivo impreso en el PNG fuente. Solo se recorta el rectángulo importado; los PNG siguen idénticos.

Limitación visible de los originales: varios PNG son pequeños y contienen fondos/bordes rectangulares semitransparentes. No se eliminó su fondo ni se regeneraron. Se conserva el diseño aprobado; una exportación 2D de mayor resolución y transparencia más limpia sería una mejora futura de Persona 4.

## HUD y eventos

Prefab: `Assets/Prefabs/UI/GameplayHUD.prefab`, instancia conectada en ARGame.

- Canvas Overlay, referencia 1080x1920, Scale With Screen Size, match 0.5, anchors por borde y SafeAreaLayout.
- Mira normal y feedback hit de 0.16 s usando PlasmaWeapon.Feedback.
- Disparo: sprites normal/pressed/disabled; invoca el MISMO onClick del botón funcional existente. No aplica damage ni consume ammo por su cuenta.
- Reload: invoca el botón RELOAD creado por ARPrototypeController, conserva la condición WAVE.
- Vida: tres sprites full/empty/damaged obtenidos del PlayerHealth real, sin cambiar el modelo de 100 HP. Flash corto al recibir daño.
- Score, ammo, wave y enemy counter: texto Unity dinámico sobre sus PNG; fuentes ScoreManager, PlasmaWeapon y ARPrototypeController/EnemySpawner.
- Mensajes: escaneo, tocar para colocar y enemigo eliminado; estados existentes y Feedback. Los textos temporales countdown/intermission son presentación.
- Game Over/Victory: panel común, btn_reintentar conectado al RETRY existente, botón MENU PRINCIPAL carga MainMenu. No existe PNG btn_menu_principal en el paquete: ese botón usa Image y texto Unity simple.
- BossHUD queda oculto en runtime hasta integración real. BossHUDView.Present(healthFraction,leftActive,coreActive,rightActive) pinta la barra y los tres iconos. Persona 2/3 debe proporcionar datos reales y activar el panel. La captura 07 utiliza valores de revisión, no un boss funcional.
- icon_enemy está disponible además del gráfico de enemigo ya incluido en el panel. Los iconos de armas, pausa, curación, portal, advertencia y msg_prepara_jefe quedan disponibles para las mecánicas futuras; no se inventaron botones que ejecuten esas mecánicas.

El Canvas antiguo permanece activo para que sus objetos, referencias y eventos sigan funcionando. VisualHUD deshabilita únicamente Canvas y GraphicRaycaster del HUD antiguo desde la escena guardada y lo mantiene así en Start, después de Awake. No se ve la cruz blanca, SHOOT rectangular ni los textos debug anteriores.

## Plano AR

Se cambió únicamente el sharedMaterial de `Assets/Prefabs/Portal/PlanePlaceholder.prefab`: del verde sólido provisional al material M_AR_Hologram, cian con alpha base 0.045 y líneas discretas. Shader en `Assets/Effects/RiftPlaneHologram.shader`, sin luces ni texturas pesadas.

ARPlaneManager, ARRaycastManager, anchors y placement permanecen intactos. ARPrototypeController ya ocultaba los renderers de los planos al colocar el portal; se reutiliza ese comportamiento. No se cambió código de Persona 1.

## VFX y animación

Después de validar la integración base se crearon Particle Systems en `Assets/Effects/Prefabs/`:

| Prefab | Conexión | Límite |
|---|---|---:|
| VFX_Portal | Portal/Visual, loop azul/cian | 24, emisión 5/s |
| VFX_PlasmaShot | Feedback de disparo aceptado, muzzle cosmético | 8 |
| VFX_PlasmaHit | Impacto no letal mediante consulta visual; muerte mediante Died | 12 |
| VFX_EnemyDamage | Caída de HP del Basic | 6 |
| VFX_EnemyDeath | EnemyHealth.Died del Basic | 18 |
| VFX_Electric | Preparado para Persona 2 | 16 |
| VFX_EnergyExplosion | Preparado para Persona 2 | 30 |
| VFX_BossSpawn | Preparado para Persona 2/3 | 28 |
| VFX_WeakPointHit | Preparado para Persona 2/3 | 10 |

Material compartido URP/Particles/Unlit y textura radial pequeña generada en Unity, situados en Assets/Effects/Materials. Los efectos no aplican damage, no cambian score ni ammo. Sin VFX Graph, luces, colisiones de partículas o partículas gigantes. EffectLifetime elimina los bursts tras 0.3–0.85 s; el loop vive con el portal. Android aún debe perfilarse en fase 3.

VisualMotion anima una oscilación lenta de fragmentos (0.012 m / 4°) y pulsación moderada de emisión mediante MaterialPropertyBlock. Portal gira hacia la cámara solamente en su Visual. Guardian tiene fragmentos flotantes y pulsación preparados. No se realizó rigging, animación de caminar/atacar ni mecánica de retroceso nueva. El disparo utiliza VFX de boca como feedback visual.

## Menú y audio

`Assets/Scenes/MainMenu/MainMenu.unity`: logo, fondo aprobado en banda panorámica preservando proporción, INICIAR → ARGame, AJUSTES deshabilitado/preparado, SALIR → Application.Quit. Cámara sencilla de fondo, InputSystemUIInputModule y EventSystem. EditorBuildSettings incluye MainMenu primero y ARGame después. Eso prepara el orden futuro de escenas; no ejecuta un build.

No se encontraron sonidos funcionales en este paquete. No se generó audio. Pendientes: portal/placement, plasma/disparo/impacto/recarga, daño/muerte, UI, armas adicionales y jefe.

## Scripts y alcance exacto

Único script funcional previo modificado: `Assets/Scripts/Integration/ARPrototypeController.cs`. Se añadieron cuatro getters de solo lectura: Player, CurrentState, CurrentWave y AliveEnemies. Permiten pintar datos que ya existían; no cambian estados, oleadas, conteo, daño, score, ammo o AR.

Nuevos en `Assets/Scripts/UI/Visual/`: VisualHUD, VisualMainMenu, SafeAreaLayout, BossHUDView, VisualMotion, EnemyVisualEffects y EffectLifetime.

Nuevo Editor-only: `Assets/Scripts/Integration/Editor/VisualIntegrationBuilder.cs`. Comandos explícitos de importación, prefabs, HUD, menú, VFX, validación, revisión y play-check mediante Temp/RiftVisualCommand.txt. No tiene comando de build. Las capturas son renders de Unity con los prefabs/canvas reales y datos sintéticos de revisión. Ese código y sus escenas temporales no se incluyen en runtime. Para revisar sin reconstruir assets: comandos validate/review; no ejecutar import/models/hud sobre cambios futuros del equipo sin revisarlos.

Se conservaron sin cambios PlasmaWeapon, EnemyHealth, EnemyMovement, EnemySpawner, ScoreManager, PlayerHealth y scripts AR. Se mantuvieron los GUID de los prefabs funcionales.

## Validación y capturas

- Unity importó y compiló los scripts; validación de shaders URP sin errores.
- Validación de escena, referencias HUD y scripts Missing de todos los prefabs: PASS.
- Basic conserva 100 HP, collider único, AimPoint y EnemyMovement. Portal conserva SpawnPoint. Guardian conserva los tres weak points separados.
- Pruebas existentes de combate: cuatro raycasts 75/50/25/0 HP; muerte única; guardas de frame/cooldown; ammo a cero y reload; muerte/retry del jugador; movimiento horizontal, pausa por tracking y cooldown de ataque: PASS.
- Play Mode real: evento INICIAR carga ARGame, ciclo Awake/Start/LateUpdate del HUD, valores dinámicos iniciales y ocultación del HUD antiguo/raycaster: PASS, errores=0.
- Console después del Play Mode: 0 errores rojos, 6 advertencias de subsistemas AR (Session, Input, Camera, Raycast, Plane, Anchor) no disponibles en Editor. Eso no valida ARCore en teléfono; se debe repetir la prueba física tras aprobar estas imágenes.

Capturas 1080x1920 fuera de runtime, en `ArtRenders/VisualIntegration/`:

1. 01_MainMenu.png
2. 02_Scanning.png
3. 03_Portal.png
4. 04_EnemyBasic.png
5. 05_GameplayHUD.png
6. 06_GameOver.png
7. 07_BossHUD.png

Son renders de revisión del Editor, con escenario de prueba y datos de HUD sintéticos; no son fotografías del teléfono ni evidencias de tracking físico. Las capturas 02–07 lo indican explícitamente. La escala de distancia de revisión y el tamaño del Guardian en la captura se ajustan solo en la escena temporal; los prefabs runtime mantienen las alturas aprobadas.

## Pendientes por persona

- Persona 1: validar placement/anchor/tracking reales con el portal vertical, orientación de Visual, plano cian, oclusión y safe area en dispositivo. La prueba física corresponde a fase 3.
- Persona 2: comportamiento Fast/Tank, boss/fases/ataques/weak points, Electric y Energy Launcher; conectar los VFX preparados y confirmar el collider del Basic frente al modelo más ancho. Mantener la separación Logic/Collider/Visual.
- Persona 3: activar y alimentar BossHUDView con datos reales, conectar msg_prepara_jefe al estado real futuro, sistema de ajustes/pausa si se desarrolla y revisar UI final de Victory/Retry.
- Persona 4: aprobación de estas capturas; decidir rigging de caminar/ataque, mejorar exportación 2D si se desea, audio posterior, medir rendimiento/VFX en Android. NO APK hasta aprobación de las capturas.

## Entrega y límite de fase

El resultado queda para revisión visual. No se generó APK ni se realizó merge. El push y los commits se registran en `ArtRenders/VisualIntegration/Git_Entrega.txt`; las evidencias de validación están en esa misma carpeta. La siguiente fase requiere aprobación explícita del usuario.
