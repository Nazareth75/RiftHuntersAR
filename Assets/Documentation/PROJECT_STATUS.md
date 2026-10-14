# RIFT HUNTERS AR - PROJECT STATUS

Actualizado: 2026-10-13. MVP técnico creado; build Android en curso, validación física pendiente.

## Proyecto y Git
Raíz: C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR
Unity 6000.3.16f1 (a56f230f6470), URP. Rama art-integration. Sin remoto ni push.
Force Text y Visible Meta Files. Proyecto original y SampleScene conservados.

## Paquetes resueltos
- AR Foundation 6.3.5.
- Google ARCore XR Plugin 6.3.5.
- XR Plug-in Management 4.5.4 (dependencia).
- XR Core Utilities 2.6.0 (dependencia).
- URP 17.3.0, Input System 1.19.0, uGUI 2.0.0.
- No se añadió OpenXR ni un proveedor VR. Legacy Input Helpers es dependencia transitiva de XR Management.

El primer intento de descarga de AR Foundation falló por ENOTFOUND/ECONNRESET. La resolución posterior recuperó el paquete oficial y Unity pudo compilar y ejecutar la herramienta de Editor. No se cambiaron versiones para ocultar el error.
Fuentes oficiales y API instalada:
https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.3/manual/project-setup/install-arfoundation.html
https://docs.unity3d.com/Packages/com.unity.xr.arcore@6.3/manual/project-configuration-arcore.html

## Escena
Assets/Scenes/Gameplay/ARGame.unity, creada y serializada por Unity.
AR Session + ARInputManager; XR Origin; Camera Offset; AR Camera con ARCameraManager, ARCameraBackground y TrackedPoseDriver del Input System; ARPlaneManager horizontal, ARRaycastManager y ARAnchorManager. La colocación usa AttachAnchor de la API 6.3 antes de instanciar el portal.
ARGame es primera escena habilitada; SampleScene se conserva. El build del prototipo incluye solamente ARGame.

## Scripts
- Scripts/AR/PortalSpawnPoint: referencia explícita al SpawnPoint.
- Scripts/Enemies/EnemyHealth: 100 HP, daño y muerte con evento único.
- Scripts/Combat/PlasmaWeapon: raycast central, 25 daño, rango 20 m; disparo deshabilitado sin tracking.
- Scripts/Game/ScoreManager: +100 por eliminación, sin persistencia.
- Scripts/Integration/ARPrototypeController: Input System, exclusión de toques sobre UI, colocación única, spawn tras 1 segundo, UI y puntuación.
- Scripts/Integration/Editor/PrototypeBuilder: generación inicial sin sobrescribir ARGame, configuración Android, smoke test y build. Menús bajo Rift Hunters.

## Prefabs y arte
- Prefabs/Portal/PortalPlaceholder: raíz técnica, Visual con cilindro y SpawnPoint.
- Prefabs/Enemies/EnemyBasicPlaceholder: raíz EnemyHealth, Visual con cápsula, HitCollider separado y AimPoint.
- Prefabs/Portal/PlanePlaceholder: mesh AR temporal sin collider físico.
- Art/Materials: PortalPlaceholder, EnemyPlaceholder y PlanePlaceholder (URP Unlit).
Todo es PENDIENTE DE DISEÑO; ningún recurso artístico final creado. Cambiar Visual preservando raíz, scripts, collider y SpawnPoint.

## Android configurado
- Build Target Android.
- Identificador provisional: com.rifthunters.ar.
- Producto: RIFT HUNTERS AR.
- Portrait, ARM64, IL2CPP, OpenGLES3, SDK mínimo 25.
- API 25 verificada en ARCoreBuildProcessor de 6.3.5 para Unity 6000.3 con GLES3.
- ARCore loader asignado a Android, Initialize XR on Startup habilitado.
- ARCore requerido y Depth opcional: no usamos Depth.
- Mobile URP pipeline en Graphics/Quality y ARBackgroundRendererFeature en Mobile_Renderer.
- Cámara/permisos: verificar manifest del APK generado; ARCore administra el acceso, autorización física aún pendiente.
- Objetivo 30 FPS, geometría simple, materiales Unlit, sin luces ni partículas del juego.

## Flujo implementado
Escanear -> detectar plano horizontal -> tocar área sin UI -> un portal -> esperar 1 s -> un enemigo estático -> apuntar con teléfono -> SHOOT -> cuatro impactos -> enemigo eliminado -> SCORE: 100.
Estados temporales SCAN ENVIRONMENT, TAP TO PLACE PORTAL, SPAWNING, READY, TARGET ELIMINATED y AR NOT SUPPORTED.
Mira centrada y botón inferior; feedback HIT/MISS. Al perder tracking el disparo se deshabilita.
No hay movimiento, vida del jugador, oleadas, otras armas ni boss: requieren estabilidad del MVP en teléfono primero.

## Pruebas realizadas
- Unity compiló y ejecutó PrototypeBuilder.Create: SUCCESS.
- Smoke test de EnemyHealth/ScoreManager en Editor: 3 impactos dejan 25 HP y score 0; cuarto impacto produce una muerte y score 100; impactos/Die posteriores no duplican score. PASS.
- Este smoke test no prueba cámara, tracking, colocación ni disparo táctil en hardware.
- ADB ejecutado: lista de dispositivos vacía. No se instaló ni probó AR físicamente.
- Arranque de ARGame en Play Mode: UI visible y cero errores rojos; seis advertencias por ausencia de proveedor AR de Editor. No es una prueba de tracking.
- El primer build falló por ausencia de SDK cmdline-tools/latest. SDK local preparado bajo Builds/Toolchain/AndroidSDK, con plataformas/build-tools/platform-tools/CMake existentes copiados y Command-line Tools 16 oficial descargado.
- ZIP 16 verificado contra SHA1 32787c10f55911fd109848906b1275723e79f659 del repositorio oficial Google. sdkmanager ejecutado con JDK17 devuelve 16.0.
- Reintento posterior encontró CMake 3.22.1 faltante en la copia local; se copió la versión existente del SDK de Unity. Nuevo build en curso, resultado/APK pendientes.

## Cómo probar en teléfono
1. Dispositivo Android compatible con ARCore; conectar y habilitar depuración USB, aceptar autorización en el teléfono.
2. Instalar Builds/Android/RiftHuntersAR_Prototype.apk cuando exista y esté validado.
3. Abrir y aceptar cámara. Escanear mesa/piso con luz y textura; esperar planos temporales.
4. Tocar plano fuera del botón. Verificar un solo portal y un enemigo tras 1 segundo.
5. Mover teléfono hasta alinear la mira con cápsula; cuatro impactos deben eliminarla y dar exactamente 100 puntos.
6. Probar fallo de puntería, toques sobre SHOOT antes de placement, pérdida/recuperación de tracking y suspensión/reanudación.
7. Registrar modelo, Android, resultados y errores en adb logcat. Reiniciar aplicación para una nueva prueba: no existe botón RESTART aún.

## Limitaciones conocidas
No afirmar AR operativo en dispositivo hasta prueba física. Actualmente no hay teléfono autorizado detectado.
Primer build IL2CPP y cambio de plataforma pueden tardar varios minutos por importación de paquetes/shaders.
Se recuperó el control de Unity mediante Computer Use y se cerró el aviso modal de SDK. La herramienta de Editor registra diagnósticos propios en Logs/RiftBuild.log.
No se modificó configuración global de Git. Builds, cachés y logs ignorados.

## Herramientas Android locales
El SDK original instalado en Unity carece de cmdline-tools moderno. El prototipo usa Builds/Toolchain/AndroidSDK (ignorado por Git), sin modificar Program Files. Unity selecciona este SDK para el build. Restaurar el SDK original mediante la API pública falló porque valida la ruta incompleta; la preferencia External Tools puede quedar apuntando al SDK local. Revisar esta preferencia antes de compilar otro proyecto y reparar el módulo SDK de Unity cuando sea conveniente. No se aceptaron licencias nuevas ni se usaron credenciales administrativas.
Fuentes: https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/android/getting-started/sdksetup/supported-dependency-versions y https://dl.google.com/android/repository/repository2-3.xml.
