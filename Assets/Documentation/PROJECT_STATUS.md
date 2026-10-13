# RIFT HUNTERS AR - PROJECT STATUS

Actualizado: 2026-10-13. Alcance completado: Fase 1 (preparación local).

## Proyecto inspeccionado
- Raíz real: C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR
- juegoHunter es la carpeta contenedora. El proyecto no fue movido ni recreado.
- Unity: 6000.3.16f1, revisión a56f230f6470. Plantilla Universal 3D / URP.
- Serialización: Force Text. Version Control: Visible Meta Files.
- Unity permanece abierto; no se cerró ni se eliminó Library.

## Paquetes instalados relevantes
| Paquete | Versión en manifest |
| --- | --- |
| Universal RP | 17.3.0 |
| Input System | 1.19.0 |
| uGUI | 2.0.0 |
| Test Framework | 1.6.0 |
| AI Navigation | 2.0.12 |
| Timeline | 1.8.12 |
| Visual Scripting | 1.9.11 |

AR Foundation, ARCore y XR Plug-in Management no figuran en manifest.
manifest.json y packages-lock.json no se modificaron.

## Compatibilidad AR verificada
La documentación oficial de AR Foundation 6.3 indica compatibilidad con Unity 6000.0 o superior y proveedor Android ARCore 6.3. Las páginas consultadas corresponden a 6.3.5 para ambos paquetes. Candidatos: AR Foundation 6.3.5 y ARCore 6.3.5; confirmar disponibilidad en Package Manager antes de instalar. No usar prereleases ni mezclar líneas de versión.

Fuentes consultadas el 2026-10-13:
- https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.3/manual/project-setup/install-arfoundation.html
- https://docs.unity3d.com/Packages/com.unity.xr.arcore@6.3/manual/project-configuration-arcore.html

Instalar un paquete no habilita por sí solo el proveedor ARCore. La prueba real sigue pendiente.

## Escenas, scripts y prefabs
- Existente: Assets/Scenes/SampleScene.unity, incluida en EditorBuildSettings.
- Scripts existentes: Readme y ReadmeEditor de la plantilla; ningún script de gameplay añadido.
- Prefabs de juego: ninguno todavía.
- Carpetas preparadas según el plan del equipo, con .meta estables para Git.
- Pendientes: MainMenu, ARGame y ArtIntegrationTest; no se crearon escenas vacías.
- Materiales y assets URP existentes conservados en Assets/Settings.

## Android observado
- Scripting backend: IL2CPP (Android: 1).
- Target architectures: ARM64 (2).
- Minimum SDK: 25; Target SDK: automático (0).
- Application identifier actual: com.UnityTechnologies.com.unity.template.urpblank.
- Input: Input System (activeInputHandler: 1).
- SDK/NDK/JDK y módulo Android: reportados por el usuario; no revalidados en esta fase.
- Plataforma activa del Editor: no verificada desde la interfaz.

## Qué funciona y qué falta
Funciona: estructura de plantilla conservada, repositorio local, exclusiones Unity, rama art-integration y documentación.
Falta: instalar/configurar AR, escena técnica, portal, enemigo, disparo, puntuación y APK probado.
No hay gameplay probado ni APK generado. No se afirma compilación sin verificar en Unity.

## Controles previstos (no implementados)
Escanear piso/mesa, tocar superficie para colocar portal, mover físicamente el teléfono para apuntar con mira central y pulsar SHOOT. Una sola arma Plasma y un EnemyBasicPlaceholder inicialmente.

## Arte
PENDIENTE DE DISEÑO: portal, criaturas, armas, HUD, iconos, botones, logo y demás arte final. Usar solo placeholders temporales. Los futuros modelos 3D reemplazarán el hijo visual del prefab conservando lógica y referencias.

## Instrucciones de prueba actuales
1. Volver al Editor y esperar la importación automática; si no actualiza, Assets > Refresh.
2. Comprobar las nuevas carpetas en Project y abrir Window > General > Console.
3. Confirmar que no aparecen errores nuevos. Esta verificación del Editor está pendiente.
4. En Git: git status --short --branch; git log --oneline -5.

## Preparación AR para Fase 2
1. Window > Package Management > Package Manager; Unity Registry; seleccionar AR Foundation, versión estable 6.3.5 si disponible; Install. Repetir con Google ARCore XR Plugin 6.3.5. Si no aparece, verificar Versions; no sustituir por una versión aleatoria.
2. File > Build Profiles > Android; Switch Platform (añadir perfil Android si todavía no existe).
3. Edit > Project Settings > XR Plug-in Management; instalar XR Plug-in Management si se solicita; pestaña Android; activar ARCore.
4. XR Plug-in Management > Project Validation > Android: revisar los errores y aplicar Fix individualmente; el validador define el SDK mínimo de esta combinación.
5. Player > Android > Other Settings: confirmar IL2CPP y ARM64; elegir el identificador del equipo antes del APK. ARCore admite OpenGLES y Vulkan; verificar la API elegida en el teléfono real.
6. Configurar el renderer URP móvil para fondo AR siguiendo la documentación de la versión instalada.
7. Crear prueba en Scenes/Tests: AR Session, XR Origin, AR Camera con Camera Manager/Background, Plane Manager y Raycast Manager. Revisar API instalada antes de escribir scripts.
8. Probar en Android compatible con ARCore: permiso de cámara, tracking, detección horizontal, colocación, disparo y puntuación. Registrar modelo del teléfono y resultado.

## Problemas conocidos
- Ruta del proyecto un nivel debajo de la ruta indicada inicialmente.
- Identificador Android todavía de plantilla; AR aún no instalado ni validado.
- Git puede mostrar avisos LF/CRLF por la configuración existente. No se cambió configuración global.
- La prueba de superficies y tracking exige un dispositivo compatible; no se validó hardware.
