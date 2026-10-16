# Reciclaje y presentación procedural — revisión local

La mecánica real en ARGame está documentada en [GAMEPLAY_AR.md](GAMEPLAY_AR.md). Las secciones siguientes describen la entrega de assets y su escena de revisión; esa escena continúa siendo exclusivamente una herramienta de desarrollo.

Base: `integracion-equipo`, commit `2a2ed98`. Sin commit nuevo ni push hasta aprobación visual.
Unity 6000.3.16f1; Blender 5.2.2 LTS. No se cambian paquetes ni los cuatro FBX aprobados.

## Archivos editables y exports

Todas las rutas siguientes son relativas a `C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR`.

| Asset | Fuente editable | Export Unity | Tris | Alto |
|---|---|---|---:|---:|
| PlasticBottle_Final | ArtSource/Recycling/PlasticBottle_Final.blend | Assets/Art/Models/Recycling/PlasticBottle_Final.fbx | 344 | 0,275 m |
| MetalCan_Final | ArtSource/Recycling/MetalCan_Final.blend | Assets/Art/Models/Recycling/MetalCan_Final.fbx | 480 | 0,140 m |
| PaperCardboard_Final | ArtSource/Recycling/PaperCardboard_Final.blend | Assets/Art/Models/Recycling/PaperCardboard_Final.fbx | 120 | 0,166 m |
| TrashBag_Final | ArtSource/Recycling/TrashBag_Final.blend | Assets/Art/Models/Recycling/TrashBag_Final.fbx | 184 | 0,360 m |
| PlasticRecycleStation_Final | ArtSource/Recycling/PlasticRecycleStation_Final.blend | Assets/Art/Models/Recycling/PlasticRecycleStation_Final.fbx | 864 | 0,820 m |
| PaperRecycleStation_Final | ArtSource/Recycling/PaperRecycleStation_Final.blend | Assets/Art/Models/Recycling/PaperRecycleStation_Final.fbx | 924 | 0,820 m |
| MetalRecycleStation_Final | ArtSource/Recycling/MetalRecycleStation_Final.blend | Assets/Art/Models/Recycling/MetalRecycleStation_Final.fbx | 1080 | 0,820 m |

Un renderer por modelo y dos materiales por prefab. Los contenedores usan la misma estructura grafito y comparten `Recycling_Graphite.mat`; sus símbolos son geometría, sin fuentes ni texturas. Botella gris clara opaca, no transparencia costosa. Los `.blend` incluyen un estudio de presentación que no se exporta. Transformaciones aplicadas, unidades métricas, pivote en suelo, triangulación, normales recalculadas. `Recycling_Stats.json` contiene las mediciones y el resultado de exportar/reimportar cada FBX en Blender. Unity convierte los ejes y valida de nuevo dimensiones/pivote.

Generador reproducible: `Tools/ArtPipeline/generar_recycling.py`. Ejecutar exclusivamente en un proceso Blender nuevo con `--background --factory-startup --python-exit-code 1 --python Tools/ArtPipeline/generar_recycling.py`. Solo regenera sus propios archivos de Recycling; no ejecutar dentro de una escena personal de Blender. Renders individuales en `ArtRenders/Recycling/`.

## Unity

Los siete prefabs están en `Assets/Prefabs/Recycling/` con los nombres sin `_Final`: PlasticBottle, MetalCan, PaperCardboard, TrashBag y las tres RecycleStation.
Todos tienen BoxCollider trigger, sin Rigidbody, layer integrada `Ignore Raycast` (2). Su geometría y layer se conservan en hijos. No empujan al enemigo ni se convierten en objetivos de daño; la futura recogida deberá consultar explícitamente la layer 2 y los triggers.

Materiales URP: `Assets/Art/Materials/Recycling/`.
VFX: `Assets/Prefabs/VFX/ContaminationVFX.prefab`, partículas pequeñas púrpura, nueve por segundo, máximo veinte por sistema, sin humo modelado, sin física ni luces. Guardian usa una instancia en bucle, detenida con la pérdida de tracking/pausa; los demás usan emisiones breves.
Catálogo desacoplado: `Assets/Resources/RecyclingVisualCatalog.asset`.

`RecyclableItem.Category`: botella Plastic, papel/cartón Paper, lata Metal, bolsa Other decorativa. `RecycleStation.Category` identifica cada estación. No existe aún recogida, clasificación, preguntas ni recompensas educativas. Los contenedores están preparados como prefabs y en la galería; no se colocan automáticamente en ARGame.

## Movimiento y contaminación

La única conexión con código previo está en `ModeloDeIntegracion`: después de colocar/escalar el modelo aprobado añade la presentación. No se editan Enemigo, NucleosJefe, GameManager, armas, oleadas, tracking, anchors, portal ni escenas de juego.

Se añade `ProceduralVisualPivot` bajo la instancia orientada a cámara, conservando las referencias de meshes/piezas. Solo se modifican sus localPosition/localRotation/localScale. La raíz funcional y su collider permanecen bajo el control original. NucleosJefe sigue alineando los colliders con sus meshes.

- Basic: respiración, balanceo al avanzar, preparación y embestida visual, sacudida al recibir daño y caída breve al morir. Una botella como máximo.
- Fast: oscilación más rápida, inclinación y desplazamiento lateral del visual, ataque/hit rápidos. Un papel/cartón como máximo.
- Tank: oscilación vertical de pasos más pesada, inclinación hacia delante, embestida y reacción de hit reducida. Dos residuos como máximo: metal y plástico.
- Guardian: levitación, rotación suave, pulsación, ataque y vibración al recibir daño/bloquear o cambiar sus núcleos; aura púrpura, sin residuos constantes.

`EnemyPollutionEmitter.Contaminate()` permite ordenar la acción. También se activa una vez automáticamente mientras el enemigo está vivo: Fast aproximadamente 1,3 s; Basic 2,1 s; Tank 2,8 s de tiempo activo. La pose breve es visual: no pausa ni modifica el combate. Tras el gesto nace el residuo junto al cuerpo, describe un arco de 0,42 s y se apoya en el plano del ancla usando SueloCombateAR. Botellas/latas se acuestan y su límite inferior se mide después de rotarlas. No se usan fuerzas ni gravedad física.

Máximo global de 32 residuos por superficie AR, además de los límites por enemigo. Se mantienen al terminar combate y se limpian con Retry o al descargar la escena/ancla. Las colas de caída/muerte son breves y se eliminan. No hay cambio de wave condicionado al reciclaje en esta entrega.

## Revisión

Abrir `Assets/Scenes/Review/RecyclingVisualReview.unity`, o menú **Rift Hunters > Recycling > 2 Abrir revisión visual**.
Escena independiente, excluida de Build Settings. En Play aparecen los cuatro prefabs funcionales; botones Contaminar, Hit, Ataque visual y Reiniciar. El botón para ver movimiento oculta solamente la galería de contenedores y centra la cámara en los monstruos/residuos. Para verla completa en PC elegir Free Aspect en Game View. No guardar cambios hechos en Play Mode.

La galería aplica únicamente presentación/objetivos de prueba a sus instancias; no modifica los prefabs de combate ni las estadísticas reales del juego. El jefe mantiene los núcleos funcionales originales.

Validaciones específicas: `Assets/Scripts/Recycling/Editor/RecyclingPresentationTests.cs`. Comprueban movimiento del hijo con raíz/collider estacionarios, perfiles, hit/escudo, límites de residuos, categorías, suelo, layer/colisión, núcleos, caída/muerte, ataques reales y contaminación automática. La prueba existente del equipo verifica además menú, HUD, tres armas, vida, score, recarga, oleadas, jefe, Game Over, Retry y Victory.

Los resultados finales se conservan en los archivos de validación de esta carpeta. Las pruebas de Editor no equivalen a una prueba física AR ni a una medición de FPS/temperatura en Android. No se genera APK en esta tarea.

Los cambios locales previos en ProjectSettings/ProjectSettings.asset y ProjectSettings/UnityConnectSettings.asset se conservan por separado, sin incluirlos en esta entrega. NO PUSH, NO MAIN, NO MERGE.
