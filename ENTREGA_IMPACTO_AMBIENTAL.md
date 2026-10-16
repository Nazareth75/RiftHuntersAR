# Entrega: impacto ambiental

Abrir con Unity **6000.3.16f1**, con Android Build Support, SDK/NDK y OpenJDK de Unity Hub. Unity instalará las versiones fijadas en `Packages/manifest.json` y `Packages/packages-lock.json`.

Escena inicial: `Assets/Scenes/MainMenu/MainMenu.unity`. Juego: `Assets/Scenes/Gameplay/ARGame.unity`. Las escenas de revisión no son escenas de entrega.

Los modelos, materiales, sprites separados de resultados, fuentes TextMeshPro, sonidos y catálogos usados por el juego están en `Assets`. Las fuentes de reciclaje editables están en `ArtSource/Recycling`; Blender no es necesario para abrir o ejecutar el juego.

## Pruebas en Editor

- `Rift Hunters > Reciclaje AR > 2 Probar flujo real completo`: combate, contaminación, clasificación, ECO, recuperación de waves, jefe, recuperación final, Victoria y Retry.
- Los comandos y resultados de pruebas automáticas viven en `Temp` y no forman parte del repositorio.
- ARCore físico requiere un teléfono compatible; las pruebas de escritorio simulan la superficie para comprobar la lógica y la presentación.

## Android

Package `com.rifthunters.ar`; Portrait; IL2CPP; ARM64; GLES3; API mínima 25; objetivo 36; cámara y ARCore requeridos.

`Rift Hunters > Build Android Prototype` genera `Builds/Android/RiftHuntersAR_ImpactoAmbiental.apk`. No se versionan APK, Library, Temp, Logs, Builds ni credenciales de firma.

Si Windows excede la longitud de rutas de Gradle, `Tools/BuildAndroidPrototype.ps1` empaqueta los datos recién generados por Unity usando la estrategia corta existente. Nunca reutilizar datos Android antiguos después de cambiar scripts o assets.
