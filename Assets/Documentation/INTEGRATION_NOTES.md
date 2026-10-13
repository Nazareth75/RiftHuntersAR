# INTEGRATION NOTES

## Base y límites
Proyecto existente en juegoHunter/RiftHuntersAR; Unity 6000.3.16f1. No mover el proyecto, cambiar Unity, eliminar Library ni recrear assets. Alcance de esta entrega: Fase 1; sin gameplay ni arte final.

## Git
Repositorio local creado dentro de la raíz real del proyecto. Commit base: chore: initialize Rift Hunters AR Unity project. Rama de trabajo: art-integration. Sin remoto y sin push. .gitignore excluye cachés, builds, IDE y claves de firma; conserva Assets, Packages, ProjectSettings y .meta.
No usar reset --hard, clean -fd o push --force. No cambiar credenciales ni configuración global.
Antes de conectar el repositorio del grupo: inspeccionar ramas, historial, estructura, ProjectVersion, Packages y ProjectSettings; explicar la estrategia al usuario antes de integrar. No asumir que el historial local es el del grupo.

## Responsabilidades propuestas
- Persona 1: Scripts/AR y colocación.
- Persona 2: Scripts/Combat y Scripts/Enemies.
- Persona 3: Scripts/Game y Scripts/UI.
- Persona 4: Scripts/Integration, Art, Effects, prefabs y pruebas de integración.
Estas carpetas no implican apropiación de archivos de compañeros. Coordinar nombres y contratos antes de integrar.

## Contratos mínimos previstos para Fase 2
Aún no implementados: placement comunica la pose colocada; portal expone SpawnPoint; EnemyHealth recibe daño y emite muerte una sola vez; EnemyMovement recibe referencia explícita del objetivo; PlasmaShooter usa raycast central de cámara y botón UI; puntuación escucha muerte sin depender del modelo visual.
Usar namespace RiftHuntersAR. Evitar dependencias circulares y búsquedas globales repetidas. Input System ya activo: no asumir UnityEngine.Input legado.
Un arma, un enemigo, una prueba técnica. No boss, otras armas, tienda o sistemas adicionales todavía.

## Assets y escenas
Conservar todos los .meta y GUID. Mover assets futuros desde Unity para preservar referencias. Carpetas vacías versionadas mediante sus .meta; Unity las reconoce al importar.
No modificar SampleScene ni escenas de compañeros para la prueba. Crear escena específica bajo Assets/Scenes/Tests en Fase 2.
Mantener el modelo visual como hijo intercambiable del prefab; colliders y lógica en raíz o hijos técnicos documentados. PENDIENTE DE DISEÑO en todo recurso provisional.
Las fuentes .ase/.aseprite posteriores son UI; exportaciones PNG para integrar. Enemigos principales 3D.

## Paquetes y ajustes compartidos
AR Foundation 6.3 y ARCore 6.3 compatibles según documentación oficial; candidatos 6.3.5/6.3.5. No instalados en esta entrega. Confirmar resolución real en Package Manager y versionar manifest junto con packages-lock.
XR Plug-in Management, habilitación Android, validación y renderer URP AR pendientes. No editar YAML de ajustes mientras el Editor pueda sobrescribirlo; preferir configuración desde Unity.
Usar IL2CPP/ARM64 ya configurados. Comprobar SDK mínimo mediante Project Validation. Revisar permiso de cámara, soporte del dispositivo y retorno de suspensión.

## Validación para integrar
Actualizar PROJECT_STATUS después de cada bloque. Registrar escena, controles, errores de Console, compilación y prueba real Android por separado. La inspección de archivos no demuestra ejecución ni tracking AR.
Unity permanece abierto: esperar importación automática o Assets > Refresh. No ejecutar un segundo Editor batch sobre la misma carpeta abierta.
Evitar luces, partículas y texturas pesadas; pooling solo cuando el flujo funcional lo justifique.
