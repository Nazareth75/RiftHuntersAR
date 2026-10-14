# INTEGRATION NOTES

Actualizado: 2026-10-13. Unity 6000.3.16f1. Rama art-integration. Sin remoto.

## Integración con equipo
Antes de añadir el repositorio del grupo inspeccionar ramas/historial, versión Unity, estructura, Packages y ProjectSettings y explicar estrategia al usuario. Conservar GUID/meta. No mover proyecto ni apropiarse de escenas de compañeros. No reset --hard, clean -fd, push --force ni push.
ARGame es escena nueva; SampleScene queda conservada. Revisar cambios de Graphics/Quality y Mobile_Renderer al integrar porque son compartidos: ahora seleccionan la pipeline móvil y el fondo AR.

## Responsabilidades sugeridas
AR: PortalSpawnPoint y colocación actualmente coordinada por ARPrototypeController.
Combate: PlasmaWeapon y EnemyHealth.
Game/UI: ScoreManager y HUD temporal; el coordinador se podrá separar cuando el equipo integre sus sistemas.
Integración: PrototypeBuilder, prefabs y materiales placeholder.
No se añadieron sistemas avanzados: validar MVP en Android antes de movimiento/oleadas/armas/boss.

## Contratos actuales
EnemyHealth.TakeDamage(float), EnemyHealth.Die(), evento Died(EnemyHealth) emitido una sola vez.
ScoreManager.AddEnemyScore() y Changed(int). Sin singleton ni guardado.
PortalSpawnPoint.SpawnPoint: Transform serializado, independiente de Visual.
PlasmaWeapon.Shoot(), Ready y Feedback(string); cámara explícita. GetComponentInParent<EnemyHealth> permite collider técnico hijo.
ARPrototypeController excluye toques UI usando EventSystem.RaycastAll (evita el estado retrasado de IsPointerOverGameObject en Input System).
Un portal bajo un ARAnchor creado por ARAnchorManager.AttachAnchor, un enemigo, spawn diferido; la lógica no depende del nombre de la cápsula o del cilindro.

## Sustitución de arte
PENDIENTE DE DISEÑO: portal, enemigo, armas, mira, botón y HUD.
Reemplazar hijos Visual manteniendo raíz y componentes técnicos. Ajustar collider/AimPoint/SpawnPoint a escala en metros del asset nuevo. Enemy placeholder mide aproximadamente 0.6 m y portal 0.5 m de diámetro.
El collider no vive en el modelo; materiales Unlit temporales. UI Text/Images estándar uGUI, sustituible por prefabs artísticos conservando referencias del controlador.

## Configuración
AR Foundation/ARCore 6.3.5, XR Management 4.5.4, Core Utils 2.6.0. Versionar manifest y lock juntos. Dependencias oficiales transitivas conservadas, sin OpenXR.
ARCore Android requerido, profundidad opcional. IL2CPP/ARM64, GLES3, API25, portrait, com.rifthunters.ar.
ARBackgroundRendererFeature en Mobile_Renderer y pipeline Mobile_RPAsset en Graphics/Quality.
XRGeneralSettings Android inicializa el loader al iniciar. La cámara usa TrackedPoseDriver del Input System, no ARSessionOrigin obsoleto.

## Herramienta del Editor
Menú Rift Hunters: Create AR Prototype, Test Four Hits And Score, Build Android Prototype.
Create rehúsa sobrescribir ARGame y rehúsa continuar con escenas abiertas sucias. No ejecutarlo sobre escenas de compañeros.
Para automatización local, el Editor consume comandos create/test/android/build desde Temp/RiftPrototypeCommand.txt y escribe Temp/RiftPrototypeResult.txt. Temp ignorado; no incluir comandos en commits. La vigilancia no corre durante Play Mode, importación o compilación.
Build produce APK Development ARM64 en Builds/Android; no firmar con credenciales privadas en esta fase.
No lanzar un segundo Editor para generar assets sobre un proyecto abierto; la herramienta se ejecuta dentro del Editor existente.

## Verificación
Smoke test de cuatro impactos y score 100 pasó en Editor, incluida protección contra doble muerte. No prueba AR físico.
ADB no detectó teléfono. La estabilidad del MVP no está certificada aún: probar tracking, placement, cuatro impactos, score, pausa/reanudación y permiso de cámara.
Registrar resultado del build y pruebas reales en PROJECT_STATUS. No confundir compilación con tracking AR.

## SDK local y artefactos de build
Builds/Toolchain/AndroidSDK contiene una copia local de herramientas: Command-line Tools 16, build-tools/platform-tools 36.0.0, plataforma Android 36 y CMake 3.22.1. NDK y JDK siguen siendo los de Unity. No versionar SDK/ZIP/APK; se reconstruyen fuera de Git. El SDK original de Unity está incompleto y su restauración vía API valida y rechaza esa ruta: revisar External Tools si se trabaja en otro proyecto.
Ignorar Resources/PerformanceTestRun*.json y preferencias temporales XR, generadas automáticamente por paquetes durante builds. Mantener ajustes y loaders ARCore versionados.

## APK y empaquetado verificado
APK de desarrollo generado y firmado: Builds/Android/RiftHuntersAR_Prototype.apk (49,016,124 bytes), ARM64, com.rifthunters.ar, min25/target36. No validado físicamente.
Salidas Gradle se acortaron a Builds/g para evitar rutas mayores a 260 caracteres. PrototypeBuilder detecta ese modo y copia el APK actual tanto a la salida esperada de Unity como al destino final, validando la estructura ARM64. Si Unity informa solamente el fallo de copia de salida, la herramienta repara esa copia; no oculta errores de compilación o de Gradle.
Tools/BuildAndroidPrototype.ps1 es auxiliar de empaquetado de datos generados previamente por Unity. Usa caché estándar de Gradle y una copia de la clave debug existente, sin contraseñas privadas ni claves de producción guardadas. El agente encontró una caída de AAPT2 en su terminal; el Gradle dentro de Unity sí produjo un APK válido. Las modificaciones generadas bajo Library/Builds se ignoran y pueden necesitar reaplicarse tras regeneración; no mover el proyecto para corregir rutas.
No añadir características avanzadas hasta validar tracking, ancla, cuatro impactos y score en Android real.

Build final confirmado en Unity: Succeeded, 0 errores, 1 advertencia. La prueba de salud/score se repitió con el Editor normal y pasó. Conservar el cambio preexistente no relacionado en UnityConnectSettings sin incluirlo en el commit técnico.
