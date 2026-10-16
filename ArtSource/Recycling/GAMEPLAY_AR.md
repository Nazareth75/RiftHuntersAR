# Recuperación ambiental dentro de ARGame

Rama local: `integracion-equipo`, base `2a2ed98`. Sin commit ni push. No se regeneran modelos, FBX ni fuentes Blender.

## Flujo real

La escena de producción sigue siendo `Assets/Scenes/Gameplay/ARGame.unity`, cargada desde el menú existente. Se conservan cámara AR, detección de plano, ancla, portal, SpawnPoint, armas, score, vidas y jefe. `RecyclingVisualReview` sigue excluida de Build Settings.

Escanear → colocar portal → cuenta atrás original → combate original → último enemigo retirado y ningún grupo pendiente → recuperación → clasificar residuos → ZONA RECUPERADA durante 1,6 s → siguiente estado del GameManager/PlanOleadas original.

`GameManager` conserva el estado de oleada/jefe durante la recuperación y bloquea la petición de grupos/avance con `RecoveryInProgress`. La interfaz opcional `IWaveRecovery` entrega el control a `RecyclingGameplay`. Solo `GameManager.CompleteRecovery()` avanza, y rechaza la llamada mientras la recuperación no haya concluido. No hay un segundo secuenciador de waves. Sin el componente opcional, las escenas de combate antiguas mantienen su comportamiento.

Si una wave termina sin residuos clasificables —por ejemplo, se eliminaron todos los enemigos antes de contaminar— muestra la recuperación breve y continúa sin inventar residuos ni bloquear la partida. Other no cuenta.

## Monstruos y residuos

Se incrementaron moderadamente las oscilaciones del hijo `ProceduralVisualPivot`: Basic con pasos/balanceo, Fast con oscilación más rápida y zigzag visual, Tank con peso/pasos/embestida, Guardian con levitación más visible. No se animan la posición o escala de la raíz funcional. No se modifica `NucleosJefe` ni los FBX aprobados.

- Basic: gesto lateral y una botella, Plastic.
- Fast: gesto breve y un papel/cartón, Paper.
- Tank: sacudida pesada y hasta dos residuos, Metal y Plastic.
- Guardian: aura durante combate, sin spam. Al morir libera una sola vez tres residuos de recuperación final, uno por categoría. Una contaminación de zona ligera permanece hasta recuperar la zona.

El residuo sale de cerca del cuerpo, recorre un arco de 0,42 s, se apoya sobre el plano del ancla y reproduce el VFX púrpura al aterrizar. Continúa allí hasta clasificarlo o reiniciar. Límites: 1 Basic, 1 Fast, 2 Tank, 3 finales del Guardian, máximo global 32 residuos por superficie. La wave más grande actual puede dejar 10. Los objetos siguen sin Rigidbody; colliders trigger en Ignore Raycast. Los raycasts normales de armas no los incluyen.

## Clasificar

Solo durante recuperación, con tracking estable:

1. Tocar un residuo: se tiñe de verde claro sin modificar sus materiales compartidos.
2. Tocar una estación: verde/plástico, azul/papel, naranja/metal.
3. Si coincide, entra mediante una trayectoria breve al contenedor, desaparece y reproduce partículas verdes ligeras. Suma exactamente +50 ECO y resta uno al contador.
4. Si no coincide, muestra CONTENEDOR INCORRECTO y mantiene el residuo seleccionado para reintentar. No cambia HP, score ni ECO.

La consulta táctil incluye explícitamente layer 2 y triggers. Ignora toques sobre controles UI. Tras un raycast sin impacto, admite un margen pequeño de selección para botellas/latas en pantalla móvil. No se recoge ni clasifica durante combate. ECO es independiente del score; aparece bajo los paneles superiores, dentro de SafeArea, y se conserva entre oleadas.

Al terminar, se ocultan estaciones y se eliminan los VFX temporales. El sonido existente de cambio de nivel sigue controlado por `SonidosPartida`; no se añade otra reproducción ni audio nuevo.

## Colocación de estaciones

Solo aparecen después del combate. Comparten el ancla del portal y se apoyan en la misma superficie. La búsqueda comprueba los extremos de sus huellas contra el contorno detectado, distancia al jugador/portal/SpawnPoint, separación y visibilidad de los residuos. La fila se lee PLÁSTICO–PAPEL–METAL de izquierda a derecha mirando hacia ella. Se intenta escala 1, luego 0,95 y 0,90 y separaciones de 0,55/0,50 m. No se aplastan centros contra el borde del plano.

Si el plano no tiene espacio físico suficiente, se solicita volver a escanear una zona mayor y se ofrece un botón explícito que reinicia la partida en ARGame. No se colocan estaciones flotantes, encima del jugador o superpuestas para forzar el avance. En móvil es normal mover/girar el teléfono para observar toda la estación y el suelo.

## Jefe, Game Over y Retry

Derrotar los núcleos conserva sus escudos y orden original. Derrotar Guardian → tres residuos finales + aura de zona → clasificarlos → ZONA RECUPERADA → limpieza del aura → Victory original.

Game Over en combate funciona igual. Retry reinicia waves, generador/jefe/núcleos, vida, score, armas/munición, ECO, selección, residuos, estaciones, VFX y la puerta de recuperación. Se conserva el portal/ancla como lo hacía Retry originalmente. Regresar al menú descarga la escena y sus objetos. La pérdida de tracking bloquea interacción/temporizadores de recuperación y pausa el aura final.

## Revisar en PC sin sustituir ARGame

En Unity, menú **Rift Hunters → Reciclaje AR → 3 Revisar ARGame en PC (F8 termina combate)**.

Abre la escena real y entra en Play. El fixture, exclusivo del Editor, simula una superficie y coloca el portal con el método de prueba ya existente. Reduce la velocidad de sus instancias para inspección. El botón TERMINAR COMBATE DE PRUEBA (o F8 con Game View enfocada) termina el combate de prueba después de dar tiempo a ver los residuos; los enemigos se generan mediante el sistema original. Al entrar en recuperación, usar clic en residuo y clic en estación. Las flechas permiten mirar alrededor. El fixture no se serializa en ARGame ni se incluye en Android. Salir de Play descarta todos sus cambios.

Para probar el combate manual normal, salir del fixture y abrir ARGame en Play con los controles de escritorio existentes. La experiencia Android mantiene colocación/touch AR reales.

## Validación

- `Assets/Scripts/Recycling/Editor/RecyclingGameplayTests.cs`: flujo real, contaminación de Basic/Fast/Tank, residuos persistentes, bloqueo de wave, tres estaciones ancladas/no solapadas, toques reales mediante HandleTap, correcto/incorrecto, +50 ECO sin duplicación, SafeArea, cinco recuperaciones, núcleos/jefe, recuperación final, Victory, Game Over y Retry.
- Prueba geométrica: fila en plano delimitado; orden correcto; rechazo de plano demasiado pequeño.
- `TeamIntegrationTests`: regresión del menú, portal, armas, selector, disparos, recarga, vida, score, waves, jefe, Game Over, Retry y Victory, incluyendo las nuevas recuperaciones.
- Validación de todas las escenas/prefabs: Missing Scripts y referencias serializadas.

Resultados y capturas finales en `outputs/RecyclingGameplay` del espacio de trabajo Codex. Las pruebas de Editor no sustituyen la comprobación física de tracking, superficie y rendimiento en el teléfono. No se genera APK en esta tarea.

ProjectSettings.asset y UnityConnectSettings.asset previos se conservan sin incorporarlos a la entrega. NO COMMIT FINAL. NO PUSH. MAIN intacta.
