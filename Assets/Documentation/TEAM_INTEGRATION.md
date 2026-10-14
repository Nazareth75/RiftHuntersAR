# Integración del equipo — 14 de octubre de 2026

## Base, resultado y límites

Repositorio: https://github.com/Nazareth75/RiftHuntersAR.git

Base: `integracion-visual`, commit `59a60749ca91455e8ed290adb3c3e54f804b15cc`.

Resultado: `integracion-equipo`. Todas las incorporaciones y correcciones se realizaron únicamente en esa rama. No se modificaron los commits de main, integracion-visual, diseno-visual, art-integration, ar-system, kathlen ni Devmodule/dilan. No se borraron ramas, no se usó reset --hard, force push ni selección automática de una versión completa del proyecto. No se generó APK.

El proyecto se validó con Unity 6000.3.16f1 y sus versiones de paquetes existentes.

## Inspección previa y ancestros

Se revisaron status, ramas locales/remotas, remoto, fetch --all --prune, historial/graph, merge-base, commits exclusivos, diffstat y cambios por archivo antes de comenzar.

| Rama remota inspeccionada | Commit | Ancestro común con la base visual |
|---|---|---|
| origin/integracion-visual | 59a60749ca91455e8ed290adb3c3e54f804b15cc | Es la base |
| origin/ar-system | bf61ea4b3ce4643c9f1519d2a628ed88b9db461d | b5d19e78e5f184a6c1f2ed3f719801abdc62a1e5 |
| origin/Devmodule/dilan | bf61ea4b3ce4643c9f1519d2a628ed88b9db461d | b5d19e78e5f184a6c1f2ed3f719801abdc62a1e5 |
| origin/kathlen | 85c9b93ec8a33a35da9043068e0007a9e6c738cd | 59a60749ca91455e8ed290adb3c3e54f804b15cc |

AR-system y Devmodule/dilan tienen exactamente el mismo árbol e historial en sus puntas iniciales inspeccionadas. Por ello se incorporó el contenido compartido una sola vez, con un merge de ar-system. La rama Dilan queda incluida como ancestro de integracion-equipo; hacer otro merge no aportaría nuevos archivos.

Kathlen ya incluía tanto la base visual como el combate de Dilan. Se comprobó esa relación y se integraron sus cambios propios, sin importar de nuevo el arte ni recuperar implementaciones antiguas que su propia rama ya había sustituido.

## AR-system y Devmodule/dilan

Cinco commits exclusivos frente a integracion-visual, compartidos por ambas ramas:

- ea171b9: módulo de combate con tres armas y enemigos.
- 9590b7c: integración previa de art-integration en Devmodule/dilan.
- 87039d6: conexión de tres armas/enemigos al prototipo AR.
- 01861b0: documentación de combate e integración.
- bf61ea4: corrección del inicio de oleadas y prueba integrada en escritorio.

Principales archivos: Combat/PlasmaWeapon.cs; Combate/Arma.cs, ArmaPlasma.cs, ArmaElectrica.cs, ArmaExplosiva.cs, GestorArmas.cs, PulsoExplosion.cs y TrazoDisparo.cs; Enemigos/Enemigo.cs, GeneradorEnemigos.cs y RespuestaVisualEnemigo.cs; Integration/EnlaceCombateAR.cs, ARPrototypeController.cs, PruebaIntegradaEscritorio.cs y Editor/PrototypeBuilder.cs; Game/ScoreManager.cs. También se conservaron sus prefabs, materiales de combate, escenas ARCombate/PartidaIntegrada/PruebaCombate y documentación.

El listado completo es de 69 archivos y está en el inventario de inspección entregado. Library, Temp, Logs y APK no se incorporaron a Git.

Qué se incorporó:

- Gestor de tres armas, cargadores independientes, cooldown, recarga temporizada, cambio de arma con protección durante recarga.
- Plasma, cadena eléctrica y daño de área de la explosiva.
- Enemigos normal/rápido/tanque, generador con conteo y eventos, puntuación variable por enemigo y ataque conectado a PlayerHealth.
- Adaptación de PlasmaWeapon para delegar al nuevo sistema conservando la API y el disparo original de respaldo.
- Enlace AR de cámara/origen/anchor, pausa por tracking y pausa de aplicación.
- Escenas y controles de prueba de escritorio.

No se encontraron diferencias adicionales de una Persona 1 independiente entre las puntas iniciales de ar-system y Dilan: son el mismo commit. La configuración AR Foundation/ARCore, detección, raycast y anchors que ya estaban en la base se conservaron.

## Kathlen

Seis commits propios de Kathlen frente al historial compartido de combate/arte:

- 75baea8: flujo de partida de Persona 3.
- f4e44e9: unión del prototipo y combate con Kathlen.
- d7a6eb0: unión del combate de Dilan con diseño visual y partida.
- d1a684f: placement simulado con Enter/clic.
- 292e6b5: controles de laptop con mouse/teclado.
- 85c9b93: consolidación de la partida completa y controles.

Sus once commits exclusivos frente a la base visual incluyen además los cinco commits compartidos de Dilan. No se cuentan como once implementaciones nuevas independientes.

Principales archivos propios: Game/GameManager.cs, GameState.cs, GameplayHooks.cs, ContratosPartida.cs, PuentePartida.cs y PlanOleadas.cs; Integration/AdaptadorGenerador.cs, CatalogoPartida.cs, ControlesComputadora.cs, ARPrototypeController.cs y EnlaceCombateAR.cs; Enemigos/ModeloDeIntegracion.cs; UI/Visual/VisualHUD.cs; Resources/CatalogoPartida.asset y conexiones visuales de los prefabs de enemigos, incluido el jefe.

Qué se incorporó:

- Flujo único de partida con cuenta 3/2/1/INVASIÓN, cuatro oleadas, jefe, Victory, Game Over y Retry.
- PuenteVida conecta el PlayerHealth existente de 100 HP con GameManager. PlayerHealth no fue reemplazado ni duplicado.
- PuentePuntuacion conecta el ScoreManager existente. Los flags de doble aplicación de daño/puntos permanecen deshabilitados.
- El adaptador de generador reutiliza el generador de Dilan y el SpawnPoint del portal.
- HUD aprobado conectado a vida, nombre/cargador del arma, cuatro oleadas, contador, tiempo y resultados de la nueva partida.
- Controles exclusivos de computadora: Enter/clic placement, mouse/flechas/WASD apuntar, clic/Espacio disparar, R recargar, 1/2/3 o rueda seleccionar arma, P/Esc pausa, Enter reintento.
- Modelos Basic/Fast/Tank/Guardian y efectos aprobados conectados a las nuevas entidades mediante ModeloDeIntegracion. Su lógica técnica no depende de la lógica incluida en el modelo visual antiguo.

Las implementaciones antiguas PlayerLives/WaveManager/HUDController del commit inicial de Kathlen ya habían sido sustituidas en la punta de esa misma rama. No se recuperaron para ejecutar dos sistemas de vida, HUD u oleadas; el trabajo queda conservado en el historial y en su arquitectura actual de puentes.

## Conflictos y decisiones por archivo

| Etapa / archivo | Motivo | Resolución |
|---|---|---|
| AR/combate — ARPrototypeController.cs | Nuevos campos/controles frente a getters del HUD visual | Se conservaron ambos y AliveEnemies pasó a consultar el conteo del sistema real |
| AR/combate — PrototypeBuilder.cs | Build anterior de ARCombate frente al flujo MainMenu → ARGame | Se conservó el flujo visual principal y el método de pruebas integrado del compañero; no se ejecutó build |
| AR/combate — EditorBuildSettings.asset | Orden de escenas y desactivación del gameplay anterior | MainMenu y ARGame siguen activos en ese orden; ARCombate se conserva como escena alternativa desactivada del flujo principal |
| Kathlen — ARPrototypeController.cs | Getters duplicados y coroutine antigua frente a GameManager/AdaptadorGenerador | Se mantuvieron los getters equivalentes del compañero y su flujo único de partida; no hay dos generadores de oleadas activos |
| Kathlen — PrototypeBuilder.cs | Acceso local a SDK que la versión de Kathlen había retirado para portabilidad | Se conservó la versión sin forzar SDK del compañero, manteniendo MainMenu/ARGame y las pruebas; no APK |

No hubo conflicto de contenido en ARGame.unity, MainMenu.unity ni en los prefabs de HUD/portal. No se reemplazaron esas escenas completas ni se eliminaron sus objetos, referencias o GUID.

## Correcciones mínimas de integración

- VisualHUD reconoce `Impacto:` de la nueva lógica para disparar el VFX de impacto aprobado, además del mensaje de respaldo `HIT`. No aplica daño ni consume ammo.
- ModeloDeIntegracion hace que únicamente la instancia visual mire a la cámara, porque la nueva lógica de Enemigo mueve el root sin rotarlo. Mantiene el root técnico/collider, la geometría y las animaciones de fragmentos existentes.
- El validador visual dejó de depender obligatoriamente de un archivo en Temp que Unity elimina al cerrar. No se reimportaron ni regeneraron los modelos para reconstruir esa caché.
- La prueba antigua de HUD ahora compara la munición con el cargador real y acepta el formato nuevo con nombre de arma.
- TeamIntegrationTests.cs es un validador Editor-only sin comandos de build, con inspección de assets y prueba de la partida real en Play Mode.

## Diseño protegido

Frente a 59a6074 no hay cambios de contenido en Assets/Art, Assets/Effects, Prefabs/UI, Prefabs/Portal, MainMenu.unity ni ARGame.unity. Los ocho FBX, los 36 PNG, sus .meta, materiales y VFX originales siguen presentes. Se mantienen los tres objetos separados del Guardian, sus fragmentos y las representaciones visuales de las armas.

El Canvas provisional permanece oculto y su raycaster desactivado. Las acciones del HUD nuevo siguen utilizando las interfaces/eventos del combate real. El portal conserva SpawnPoint y los sistemas de detección/placement/anchors no fueron reemplazados. No se duplicaron GUID de assets existentes ni se cambiaron versiones de paquetes.

## Verificación

Primera etapa AR/combate:

- Compilación sin errores.
- Escena/prefabs/componentes/referencias y shaders aprobados: PASS.
- Pruebas de respaldo: cuatro raycasts 75/50/25/0 HP, muerte única, guards frame/cooldown, ammo/reload, vida/retry y movimiento/ataques: PASS.
- MainMenu → ARGame y HUD en Play Mode: PASS, errores=0.

Resultado combinado y correcciones visuales:

- Todos los archivos de escena abren.
- Missing scripts: 0. Missing referencias serializadas de prefabs: 0.
- Catálogo con armas, normal, rápido, tanque y jefe: referencias válidas.
- Modelos aprobados y colliders funcionales de cada enemigo: presentes.
- GUID duplicados en Assets: 0.
- Evento real INICIAR carga ARGame.
- Una sola partida y vida/puntuación compartidas; HUD y munición inicial correctos.
- Placement de fixture con el portal real y SpawnPoint, mesh y VFX presentes.
- Cuenta atrás → Wave1; enemigo del equipo con modelo aprobado y sin EnemyHealth antiguo duplicado.
- Botón real del HUD dispara por raycast, aplica daño/muerte, consume ammo una vez y suma los puntos una vez.
- Recarga temporizada, crosshair_hit, orientación visual y VFX de impacto del nuevo mensaje: PASS.
- Selección/disparo de eléctrica (cargador 8) y explosiva (cargador 5): PASS.
- PlayerHealth/PuenteVida → Game Over y corazones aprobados: PASS.
- Botón real Retry restaura vida, score, ammo y generador.
- Cuatro oleadas y jefe → Victory, contador y HUD de jefe/resultados: PASS.
- Volver a MainMenu limpia el singleton de GameManager: PASS.
- Consola final de la prueba: 0 errores. Las advertencias de subsistemas AR no disponibles en Editor son esperables; no equivalen a una prueba física de ARCore.

Los fixtures de prueba ajustan posiciones/velocidad y aceleran el tiempo únicamente durante Play Mode para recorrer las oleadas. No guardan esos cambios en assets, no prueban tracking físico ni generan APK. La comprobación de disparo utiliza el botón real y el raycast real; eléctrica/explosiva también se seleccionan/disparan por sus interfaces reales.

## Cambios locales previos protegidos

Antes de crear la rama se guardaron ProjectSettings.asset y UnityConnectSettings.asset en una copia externa de trabajo y en el stash `2330a056840bb3c787f46a9010ff07c0c9505bbe`, con mensaje respaldo-local-previo-integracion-equipo-2026-10-14. El stash no se eliminó. No se incluyeron esos cambios locales en los commits de integración.

## Pendientes exactos

1. Revisión de esta integración por el usuario antes de autorizar una nueva APK.
2. Prueba física de ARCore: permiso/cámara, tracking, planes cian, placement/anchor, ocultación de rejilla, pausa/reanudación y estabilidad espacial. No se generó APK en esta fase.
3. Revisar en teléfono la silueta de los colliders de normal/rápido/tanque/jefe frente a los modelos bajos; se conservaron los valores funcionales del equipo y no se rebalancearon.
4. Revisar HUD portrait/safe area, longitud del nombre de arma/munición y superposición de los textos de tiempo/resultados en diferentes pantallas.
5. Revisar en teléfono la alineación de los núcleos funcionales creados por Kathlen con los WeakPoint_Left/Core/Right del modelo aprobado. Los tres objetos de arte se conservan, y la lógica nueva genera sus propias esferas de daño.
6. Las tres armas conservan la lógica del compañero; la representación de primera persona conserva el modelo plasma de la base. Los modelos eléctricos/de energía aprobados siguen preparados, sin un cambio automático de modelo de primera persona añadido en esta integración.
7. Rigging/animación de caminar/atacar y rendimiento Android siguen pendientes. Revisar volumen y mezcla en teléfono del audio que ya aportó Kathlen.

No merge a main. No APK. Las ramas originales quedan intactas y el resultado se publica únicamente en origin/integracion-equipo.

## Actualizaciones remotas recibidas durante la revisión

Antes de publicar se detectaron nuevos commits de los compañeros y se inspeccionaron e incorporaron también:

- ar-system: 7131fd1, f43b6bd y a944738. Punta final a94473893f9dbdb4f60ec8e06f7a50741744ea34. Añaden retención del anchor, pausa al perder seguimiento y mensajes por NotTrackingReason con retardo de 0,5 s. Cambio adicional: ARPrototypeController.cs. Devmodule/dilan sigue en bf61ea4; comparte los cinco commits anteriores, sin los tres nuevos de AR.
- Kathlen: be92425, punta be924252a2a21c68ac3a634d5811d121a8a47cc3. Añade NucleosJefe.cs, SonidoJuego.cs, SonidosPartida.cs, 30 clips OGG y licencia CC0 de Kenney incluida por la compañera. Modifica Enemigo, ArmaPlasma, AdaptadorGenerador, ARPrototypeController, VisualHUD y VisualMainMenu. Son 76 archivos adicionales, inventariados por separado. Cuerpo y centro protegidos; primero ambos brazos, luego centro; muerte y puntos únicos. No se descargó ni creó audio nuevo.
- AR tiene ocho commits exclusivos frente a la base; Dilan cinco. Kathlen tiene doce frente a la base, siete propios y cinco compartidos. Se conservaron los historiales sin duplicar sistemas activos.
- El merge de AR actual encontró cuatro grupos de conflicto en ARPrototypeController: se combinaron anchor/mensajes/retardo con los getters y GameManager de Kathlen; se omitió únicamente la coroutine antigua que duplicaría el flujo. El HUD muestra el mensaje nuevo cuando WAVE está pausada por tracking.
- El último merge de Kathlen encontró un conflicto en VisualHUD: se conservaron tanto el mensaje de tracking como la pista de núcleos, con prioridad al tracking perdido. ARGame/MainMenu y prefabs visuales no tuvieron conflictos.
- Se corrigieron marcadores remanentes CRLF antes de validar. Unity entró en modo seguro; tras recompilar, al primer Play Mode sufrió un cierre nativo en recarga de assemblies (serialization layout). Se repitió la validación desde un arranque limpio; el resultado final se adjunta a este informe.

Resultado final desde arranque limpio: PASS partida completa, errores=0; PASS escudos/brazos/centro/muerte/puntuación única del jefe. El cierre nativo no se repitió. Las pruebas de hardware AR y audio percibido siguen pendientes.
