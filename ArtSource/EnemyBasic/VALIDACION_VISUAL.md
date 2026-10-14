# EnemyBasic_Final — revisión 2, pendiente de aprobación

Fecha: 2026-10-13. Rama diseno-visual, creada desde b5d19e7 de art-integration. No integración Unity, no gameplay cambiado y ningún otro asset modelado.

## Resultado real de ejecución

Blender 5.2.2 LTS x64, C:\Program Files\Blender Foundation\Blender 5.2\blender.exe. CLI/Python/bpy, guardado blend, exportación FBX y render Cycles CPU ejecutados. Sin plugins ni software instalados por el agente.

Script reutilizable Tools/ArtPipeline/generar_enemy_basic.py. Validador Tools/ArtPipeline/validar_enemy_basic.py. README explica ejecución desde raíz, ejes y remapeo URP futuro.

Modelo: 2.000 triángulos, 1.204 vértices en las mallas fuente, ocho partes semánticas, tres materiales de modelo (MetalDark, MetalLight, EnemyEnergy), cero texturas. Dimensiones Blender: ancho X 0,90747 m, largo Y 1,10974 m, alto Z 0,60000 m. Pivote en suelo; frontal -Y Blender; export -Z forward/Y up. Fuente editable estática, sin rig ni animación. Round-trip FBX confirma 0,60000 m, ocho meshes, mismo presupuesto triangular y ningún objeto de estudio exportado. Orientación en Unity y rendimiento Android se validarán al integrar después de aprobación.

## Rutas exactas

- Fuente: C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ArtSource\EnemyBasic\EnemyBasic_Final.blend
- FBX: C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ArtExports\EnemyBasic\EnemyBasic_Final.fbx
- Generador: C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\Tools\ArtPipeline\generar_enemy_basic.py
- Perspectiva: C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ArtRenders\EnemyBasic\EnemyBasic_Perspectiva.png
- Frente: C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ArtRenders\EnemyBasic\EnemyBasic_Frente.png
- Lateral: C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ArtRenders\EnemyBasic\EnemyBasic_Lateral.png
- Trasera: C:\Users\HP\Desktop\tareas para subir hoy\program multimedia\juegoHunter\RiftHuntersAR\ArtRenders\EnemyBasic\EnemyBasic_Trasera.png

## Evaluación visual realizada por el agente

| Criterio | Comparación contra el Basic del concepto |
|---|---|
| Silueta | Cuadrúpeda, ancha, cabeza integrada, espalda acorazada y espinas. No humanoide ni cápsula. |
| Proporciones | Postura baja y patas abiertas, extremidades delanteras robustas. Concepto más orgánico; versión actual más biomecánica y simétrica. |
| Colores | Grafito/metal y naranja frontal; luz de estudio añade reflejos azules. La referencia tiene más rojo y zonas de lava. |
| Agresividad | Ojos estrechos, mandíbula angular, colmillos, garras y hombros con púas. Sin rig todavía, postura estática. |
| Similitud | Conserva lectura de depredador blindado con núcleo frontal; no es reproducción exacta del concept art. |
| Legibilidad low-poly | Placas grandes/facetas reconocibles a distancia, 2.000 triángulos. Núcleo frontal será visible aun sin bloom. |

Primera iteración demasiado clara y núcleo pálido. Segunda cambió grafito/reflexión/emisión, añadió fragmentos/fisuras faciales y margen de encuadre; se renderizaron nuevamente las cuatro vistas. Simplificaciones: microgrietas, lava orgánica, dentición compleja, partículas y detalle fino. La fuente separa Body/Armor/Head/Core y cuatro patas; ocho meshes pueden implicar varios submeshes/draw calls y necesitar combinación/rigging según uso final. No prometer rendimiento Android todavía.

## Limitaciones y aprobación

Los materiales Blender son preview, no shaders Unity definitivos. El FBX no garantiza remapeo de emisión; se crearán/remapearán materiales URP solo después de aprobación. La primera ejecución encontró error de referencias a objetos después de Join, corregido antes de obtener archivos válidos. La segunda generación terminó bien pero la miniatura automática intentó escribir fuera del proyecto; desactivada en pipeline y guardado posterior comprimido comprobado sin ese error. Warnings de deprecación de use_nodes en Blender 6 son avisos futuros; Blender 5.2 funciona.

No se tocó EnemyHealth, EnemyMovement, EnemySpawner, PlasmaWeapon, ScoreManager, PlayerHealth, oleadas, AR, anchors ni placement. Cambios locales previos de ProjectSettings se conservan sin commit. art-integration sigue en su commit base. Renders y fuentes permanecen fuera de Assets.

Esperar respuesta del usuario: APROBADO o HAY QUE CAMBIARLO. No empezar Portal/Fast/Tank/Boss/armas/HUD ni integrar este modelo antes de esa respuesta.
