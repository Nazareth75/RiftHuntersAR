# Pipeline Persona 4 — EnemyBasic

Rama exclusiva: diseno-visual, base b5d19e7 de art-integration. No modificar gameplay ni integrar hasta aprobación visual.

Blender comprobado: C:\Program Files\Blender Foundation\Blender 5.2\blender.exe, versión 5.2.2 LTS. Python/bpy incluidos; no dependencias externas ni plugins instalados. Exportador FBX incluido realmente ejecutado.

## Ejecutar en PowerShell

Desde la raíz del repositorio:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --factory-startup --python-exit-code 1 --python Tools/ArtPipeline/generar_enemy_basic.py -- --samples 32 --resolution 1000 --revision 2
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --factory-startup --python-exit-code 1 --python Tools/ArtPipeline/validar_enemy_basic.py
```

El generador limpia únicamente la escena del proceso de trabajo nuevo. Nunca ejecutar desde una escena Blender personal con cambios sin guardar. Es determinista: crea geometría, materiales, jerarquía, fuente, FBX, estadísticas y cuatro PNG. Regenera sus propios archivos de salida; backups .blend1 y logs no se versionan. No guarda nada en el home. El render es Cycles CPU con cuatro hilos para dejar recursos al usuario.

## Carpetas

- ArtSource/References/RiftHuntersAR_ConceptoOficial.png: imagen proporcionada por el usuario.
- ArtSource/EnemyBasic/EnemyBasic_Final.blend: fuente editable, piezas y estudio de preview.
- ArtSource/EnemyBasic/EnemyBasic_Stats.json: mediciones de geometría/escala.
- ArtExports/EnemyBasic/EnemyBasic_Final.fbx: ocho partes semánticas, sin luces/cámara/suelo.
- ArtRenders/EnemyBasic/EnemyBasic_{Perspectiva,Frente,Lateral,Trasera}.png: revisión visual.

Todo se mantiene fuera de Assets para evitar importación prematura o dependencia de Blender en PCs de compañeros. Después de aprobación se copiará exclusivamente el FBX a Assets/Art/Models y se crearán materiales URP; esa etapa NO se ejecutó.

## Modelo y materiales

EnemyBasic_Final → Body/Armor/Head/Core/Leg_FL/Leg_FR/Leg_BL/Leg_BR. Cada grupo tiene una malla triangulada. Pivote en suelo, altura 0,60 m, frontal en -Y Blender, export -Z forward/Y up para Unity. Round-trip de FBX comprueba escala y suelo; orientación efectiva con EnemyMovement se verificará al integrar, no se asume por el render.

Tres materiales del enemigo: MetalDark, MetalLight y EnemyEnergy. Sin texturas, sin rig/animaciones todavía. Material PreviewOnly_Floor solo pertenece al estudio .blend y no al FBX. Los nodos Principled se usan como preview, no se promete que Unity reproduzca emisión automáticamente: tras aprobación se remapearán a URP/Lit y emisión naranja. No requiere bloom para distinguir zonas energéticas.

La fuente preserva partes separadas. Ocho meshes implican varios draw calls/submeshes; al integrar se decidirá combinar la versión estática o conservar partes si se anima, con medición Android. Presupuesto poligonal está en Stats; no se afirma rendimiento móvil sin prueba.

## Validación y referencia

Primera iteración: cuatro patas, torso ancho, placas, mandíbula/núcleo y espinas, pero demasiado gris claro/núcleo pálido. Segunda iteración: grafito más oscuro, energía más saturada, fragmentos sobre núcleo, fisuras pequeñas en frente y mayor margen de encuadre.

Conservado del Basic conceptual: postura cuadrúpeda agresiva, espalda acorazada baja, placas angulares, hombros anchos, púas, núcleo frontal naranja y ojos estrechos. Simplificado: grietas orgánicas/roca fina, partículas de lava, microdetalle, dentición irregular y articulaciones complejas. Es una aproximación low-poly biomecánica, no copia 3D exacta ni modelo animado terminado.

Validación automática revisa .blend, tres materiales del modelo, ocho partes, triangulación, renders existentes, exportación/reimportación FBX y altura/pivote. La inspección visual de las cuatro imágenes sigue siendo obligatoria antes de pedir aprobación. No se hacen assets Fast/Tank/Boss/portal/armas/UI en esta etapa.
