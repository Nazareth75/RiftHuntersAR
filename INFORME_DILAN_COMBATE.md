# RIFT HUNTERS AR — Informe de avance e integración de Dilan

- **Fecha:** 13 de octubre de 2026
- **Responsabilidad:** Persona 2 — combate, armas, daño, enemigos y spawner
- **Rama de entrega:** Devmodule/dilan
- **Repositorio:** https://github.com/Nazareth75/RiftHuntersAR
- **Unity:** 6000.3.16f1

## 1. Qué se entrega y para qué sirve

Se entrega el código fuente y los assets de un combate independiente, conectado al prototipo AR del equipo. La intención es que los compañeros puedan continuar desde una base común: colocar el portal con AR, combatir con tres armas, generar diferentes enemigos, aplicar daño al jugador y actualizar puntos y oleadas.

No se está entregando una nueva APK. Dilan decidió posponer la build y compartir primero la rama para que el equipo continúe trabajando. El arte, los efectos y la interfaz actuales son provisionales.

Hay tres escenas con objetivos diferentes:

| Escena | Para qué sirve |
| --- | --- |
| Assets/Scenes/Gameplay/ARCombate.unity | Continuar el juego integrado: AR del compañero más nuestras tres armas y variantes de enemigo. Es la primera escena habilitada para el build. |
| Assets/Scenes/Gameplay/ARGame.unity | Conservar el prototipo original del compañero como referencia y para sus pruebas existentes. |
| Assets/Scenes/Pruebas/PruebaCombate.unity | Probar armas, daño, enemigos y pausa con una cámara normal, sin depender del teléfono ni del tracking AR. |

ARGame y ARCombate no ejecutan dos combates en paralelo. ARGame conserva el combate original; ARCombate utiliza el módulo integrado.

## 2. Qué hizo previamente el compañero

La rama **art-integration** fue preparada por el compañero de arte e integración, quien adelantó partes de AR, combate y gameplay para poder probar el prototipo. Estos avances se incorporaron mediante merge; no se atribuyen a Dilan.

El informe inicial compartido quedó desactualizado frente a la rama publicada: además del prototipo básico, la rama ya tenía vida del jugador, movimiento, munición, tres oleadas y reintento.

| Parte heredada | Qué hace |
| --- | --- |
| AR Foundation y ARCore 6.3.5 | Proporcionan los paquetes necesarios para usar realidad aumentada y el proveedor Android. |
| Cámara, planos y raycast AR | Detectan superficies horizontales y permiten seleccionar una posición tocando la pantalla. |
| Portal con ancla | Coloca una única instancia del portal y mantiene el contenido relacionado espacialmente con la superficie real. |
| PortalSpawnPoint | Separa el punto de aparición de enemigos de la representación visual del portal. El artista puede cambiar el modelo sin romper el spawn. |
| PlasmaWeapon original | Dispara un raycast central, aplica 25 de daño y evita disparos duplicados en el mismo frame. Su cargador es de 30 y su recarga original era instantánea. |
| EnemyHealth y EnemyMovement | Administran el enemigo básico original de 100 HP, muerte única, avance horizontal y ataques con cooldown. |
| PlayerHealth | Mantiene 100 HP, avisa de cambios y muerte, y permite restaurar la partida con Retry. |
| ScoreManager | Actualiza los puntos y avisa al HUD. El prototipo original sumaba 100 por baja. |
| EnemySpawner original | Genera enemigos desde el portal y registra cuáles siguen vivos. |
| ARPrototypeController | Coordina escaneo, colocación, cuenta regresiva, tres oleadas, Victory, Game Over y Retry, además del HUD temporal. |
| Configuración Android y URP | Identificador com.rifthunters.ar, IL2CPP/ARM64, orientación vertical, GLES3, API mínima 25 y target 36; incluye el fondo de cámara AR en el renderer móvil. |
| Placeholders y herramientas | Portal, enemigo, planos y materiales provisionales; pruebas de Editor y herramientas de build del prototipo. |

El compañero informó builds y pruebas de su versión. Sus resultados no validan automáticamente las funciones nuevas incorporadas en nuestra integración.

## 3. Qué hicimos como persona 2

Primero desarrollamos el combate en una escena propia para poder avanzar sin depender de AR. Después lo conectamos a la base compartida.

### Armas

Todas las armas disparan desde el centro de la cámara. Usamos raycasts para determinar el impacto, sin crear una bala física para cada disparo.

| Arma | Comportamiento | Daño | Cargador | Cadencia mínima | Recarga |
| --- | --- | --- | --- | --- | --- |
| Plasma | Daña al enemigo al que apunta la mira. | 20 | 30 | 0,25 s | 1,5 s |
| Eléctrica | Necesita un primer impacto en un enemigo y salta hacia enemigos cercanos, hasta tres objetivos distintos. | 35 por objetivo | 8 | 0,8 s | 2 s |
| Explosiva | Explota en el punto de impacto; puede alcanzar enemigos alrededor, incluso al disparar al suelo. | 80 al objetivo directo o 40 a los cercanos | 5 | 1 s | 2,5 s |

La cadena eléctrica tiene un radio de 2,5 metros y la explosión un radio de 2 metros. Estos valores son configurables.

Cada arma conserva su propia munición al cambiar. Los disparos fallidos también consumen munición. No se puede disparar con el cargador vacío ni durante una recarga, y no se permite cambiar de arma para saltarse una recarga. La reserva es ilimitada en esta versión; no hay pickups ni economía de munición.

Un enemigo con varios colliders no recibe daño varias veces por la misma cadena o explosión. El objetivo directo de una explosión recibe 80, sin sumar además los 40 del área.

### Enemigos

Usamos el mismo componente Enemigo para las tres variantes, con valores y apariencia diferentes. Todavía son cápsulas provisionales; no se incorporaron monstruos 3D finales.

| Variante | Vida | Velocidad | Puntos | Ataque en la escena AR |
| --- | --- | --- | --- | --- |
| Normal | 50 HP | 0,6 m/s | 100 | 34 HP |
| Rápido | 30 HP | 1,2 m/s | 150 | 34 HP |
| Tanque | 200 HP | 0,3 m/s | 300 | 68 HP |

Avanzan horizontalmente hacia la cámara, conservando su altura. No utilizan NavMesh ni evitan obstáculos. Al llegar a la distancia de ataque, hacen daño una sola vez y desaparecen. Una baja da puntos una sola vez; un enemigo que alcanza al jugador no cuenta como baja.

El módulo independiente expresa el ataque como 1 o 2 unidades. EnlaceCombateAR multiplica esas unidades por 34 para conectarlas con los 100 HP del jugador compartido. Por eso la escala del HUD de prueba y la de la escena AR son diferentes.

### Respuesta visual y controles de prueba

Añadimos trazos de disparo, arcos eléctricos, ondas de explosión y un destello blanco al recibir daño. La mira de la prueba se pone verde al apuntar a un enemigo y muestra su vida restante.

En PruebaCombate se puede:

- Disparar con Espacio, clic en el escenario o el botón DISPARAR.
- Recargar con R o RECARGAR.
- Elegir armas con 1, 2 y 3 o sus botones.
- Apuntar con las flechas.
- Elegir el tipo de enemigo y crear un grupo de tres.
- Pausar/reanudar con P o el botón correspondiente.
- Reiniciar la prueba y restaurar vida, puntos y cargadores.

Estos controles y el HUD de prueba no constituyen la interfaz final del juego.

### Pausa, generación y limpieza

La pausa detiene disparos, movimiento, ataques y nuevas apariciones. También conserva el tiempo pendiente de recarga, sin cambiar globalmente Time.timeScale.

El generador informa cuántos enemigos siguen presentes y emite EnemigosAgotados cuando no quedan enemigos ni apariciones pendientes. Cancelar o limpiar el combate no se interpreta como completar una oleada ni otorga puntos.

Al terminar una partida se bloquean las armas, se cancelan las recargas y se limpian los enemigos. Reiniciar restablece los cargadores y permite comenzar nuevamente.

## 4. Qué hace cada script propio

| Archivo | Responsabilidad |
| --- | --- |
| Assets/Scripts/Combate/Arma.cs | Comparte cámara, raycast, cadencia, cargador, recarga, pausa y eventos de disparo/munición. Las armas concretas definen cómo aplicar el impacto. |
| Assets/Scripts/Combate/ArmaPlasma.cs | Aplica daño a un único enemigo impactado. |
| Assets/Scripts/Combate/ArmaElectrica.cs | Busca objetivos cercanos, evita repetir enemigos y dibuja los saltos eléctricos. |
| Assets/Scripts/Combate/ArmaExplosiva.cs | Distingue daño directo y de área; permite sustituir su efecto provisional por un prefab de partículas. |
| Assets/Scripts/Combate/GestorArmas.cs | Selecciona el arma, recibe las órdenes de disparo/recarga, asigna la cámara y permite pausar, detener o reiniciar las armas. |
| Assets/Scripts/Combate/TrazoDisparo.cs | Escucha el evento de disparo y representa su recorrido brevemente con un LineRenderer. |
| Assets/Scripts/Combate/PulsoExplosion.cs | Anima los anillos provisionales de explosión. No interviene en el daño ni tiene collider. |
| Assets/Scripts/Combate/ControlesPruebaCombate.cs | Configura la escena de prueba, instancia el sistema de armas y presenta controles, vida, puntos y mensajes temporales. |
| Assets/Scripts/Enemigos/Enemigo.cs | Administra vida, movimiento, ataque, muerte y retiro del enemigo; emite eventos para conectarlo al juego. |
| Assets/Scripts/Enemigos/GeneradorEnemigos.cs | Ejecuta apariciones con cantidad/intervalo, mantiene referencias de los enemigos, permite pausa/limpieza y registra el final del combate generado. |
| Assets/Scripts/Enemigos/RespuestaVisualEnemigo.cs | Produce el destello de daño sin modificar permanentemente el material compartido de los enemigos. |

Los nombres propios y comentarios se mantienen en español. Assets, Scripts, Scenes, Prefabs, Materials, Editor y Resources conservan sus nombres originales; métodos de Unity como Awake, Update y OnGUI también. Los componentes heredados mantienen sus nombres para conservar compatibilidad.

Los .meta se versionan junto a cada asset: contienen los identificadores que permiten a Unity conservar las referencias.

## 5. Qué cambiamos para unir ambos trabajos

Hacer merge no bastaba: el disparo del prototipo buscaba EnemyHealth, mientras nuestras armas buscan Enemigo. Había que conectar cámara, salud, daño, puntos y oleadas para que no quedaran dos sistemas separados.

| Archivo o asset | Cambio y propósito |
| --- | --- |
| Assets/Scripts/Integration/EnlaceCombateAR.cs | Nuevo punto de conexión: instancia SistemaArmas, conecta los enemigos con PlayerHealth/ScoreManager, decide la variante que solicita cada oleada y coordina pausa/reinicio. |
| Assets/Scripts/Combat/PlasmaWeapon.cs | Conserva Shoot, Reload, Ready y Feedback para el prototipo y sus pruebas. En ARCombate delega al GestorArmas integrado y sigue bloqueando llamadas duplicadas del mismo frame. |
| Assets/Scripts/Game/ScoreManager.cs | Se añadió AgregarPuntos(cantidad) para aceptar 100, 150 o 300. AddEnemyScore sigue disponible para el prototipo original. |
| Assets/Scripts/Integration/ARPrototypeController.cs | Conserva el flujo AR y de partida; conecta el enlace, muestra las tres armas, actualiza munición según el arma seleccionada y adapta apariciones, tracking, Game Over y Retry. |
| Assets/Scripts/Enemigos/GeneradorEnemigos.cs | Acepta un contenedor opcional para colocar los enemigos bajo el ancla del portal. |
| Assets/Scripts/Integration/Editor/PrototypeBuilder.cs | La herramienta heredada elige ARCombate para el build si existe. La dependencia del editor Android queda condicionada para que su ausencia no bloquee la compilación en Windows. |
| ProjectSettings/EditorBuildSettings.asset | ARCombate queda como primera escena habilitada; ARGame se conserva deshabilitada para ese build. |
| Assets/Scenes/Gameplay/ARCombate.unity | Nueva escena con referencias guardadas al enlace, SistemaArmas y las tres variantes, basada en la escena AR del compañero. |

Se resolvieron los conflictos del merge en los .meta de las carpetas compartidas Scripts y Prefabs. Se conservaron los identificadores de los scripts, escenas y prefabs individuales.

Las herramientas propias que utilizamos para preparar los assets se retiraron del proyecto una vez que escenas y prefabs quedaron guardados. Los assets entregados funcionan sin esos generadores. Las herramientas heredadas del compañero permanecen para sus pruebas y builds.

## 6. Assets preparados para continuar

| Ubicación | Qué contiene y cómo se utiliza |
| --- | --- |
| Assets/Prefabs/Armas/SistemaArmas.prefab | GestorArmas, las tres armas y sus trazos configurados. Se instancia y recibe la cámara AR desde el enlace. |
| Assets/Prefabs/Enemigos/EnemigoNormal.prefab | Variante normal con vida, collider y representación provisional. |
| Assets/Prefabs/Enemigos/EnemigoRapido.prefab | Variante rápida, más pequeña y con menos vida. |
| Assets/Prefabs/Enemigos/EnemigoTanque.prefab | Variante grande, lenta y resistente. |
| Assets/Materials/Combate | Materiales provisionales de los efectos de las armas. |
| Assets/Prefabs/Portal/PortalPlaceholder.prefab | Portal heredado, con Visual y SpawnPoint separados. |
| Assets/Prefabs/Enemies/EnemyBasicPlaceholder.prefab | Enemigo del prototipo original; permanece para referencia y pruebas. |
| Assets/Scripts/LEEME.md | Instrucciones de configuración y métodos/eventos del módulo de combate. |
| Assets/Documentation/INTEGRACION_COMBATE.md | Detalles del enlace, balance, verificaciones y limitaciones. |
| Assets/Documentation/TEAM_HANDOFF.md | Entrega contextual del compañero, con el estado de la base compartida. |

Para colocar modelos finales, sustituir el hijo Visual del prefab y conservar el objeto raíz, su collider técnico y su componente de enemigo. Revisar la escala y evitar colliders duplicados en los modelos importados.

## 7. Cómo funciona la partida integrada

1. La sesión AR reconoce el entorno.
2. El jugador toca una superficie y se coloca el portal con ancla.
3. Comienza la cuenta regresiva de tres segundos.
4. Los enemigos aparecen desde el SpawnPoint del portal.
5. El jugador apunta moviendo el teléfono, elige un arma y dispara.
6. Las bajas actualizan ScoreManager; los ataques actualizan PlayerHealth.
7. La oleada termina cuando ya no quedan enemigos.
8. Entre oleadas se reponen los cargadores y se selecciona plasma.
9. Tras la tercera oleada se muestra Victory; si la vida llega a cero, Game Over.
10. Retry conserva el portal, limpia los enemigos y reinicia vida, puntos y armas.

| Oleada | Composición |
| --- | --- |
| 1 | 3 normales |
| 2 | 4 normales y 1 rápido |
| 3 | 4 normales, 2 rápidos y 1 tanque |

Eliminar a los 15 enemigos suma **1850 puntos**. La puntuación puede ser menor si algunos alcanzan al jugador y desaparecen sin ser eliminados.

La pérdida de tracking y las interrupciones de la aplicación pausan el combate integrado. El recorrido completo y la recuperación en un teléfono siguen pendientes de validación física.

## 8. Qué se comprobó

**Pruebas manuales de Dilan:** confirmó en Play el combate independiente, disparo, cambio de armas, recarga, electricidad, explosión y respuesta visual. Estas confirmaciones pertenecen a la escena de prueba, no al flujo AR físico.

**Pruebas automatizadas en una copia temporal con Unity 6000.3.16f1:**

- Compilación completa, incluidos los scripts de Editor.
- Pruebas originales: cuatro impactos de 25 sobre 100 HP, muerte/puntos una sola vez, protección frente a duplicados, cargador vacío/recarga, vida/Retry, movimiento horizontal, pausa por tracking y cooldown de ataque.
- Referencias de ARCombate, sistema de armas y variantes, sin scripts propios faltantes.
- Plasma integrado: 50 a 30 HP con un disparo y una munición; segundo disparo del mismo frame rechazado.
- Pausa: disparo bloqueado y estado de recarga conservado; fin de combate cancela la recarga.
- Eléctrica: 35 de daño a tres objetivos con una munición.
- Explosiva: 80 al directo y 40 a los cercanos, sin daño duplicado.
- Cargadores independientes al cambiar.
- Puntuación de variantes y escala de daño sobre la vida compartida.

**No comprobado en esta integración:** tracking real, permisos/cámara en dispositivo, ciclo completo de oleadas, apariencia/rendimiento en Android, recuperación tras suspender la aplicación y una nueva APK.

## 9. Incidencia local de paquetes y estado de build

Unity no pudo terminar de instalar ARCore en la caché local: EPERM durante un rename. Eso produjo el error de namespace UnityEditor.XR.ARCore y avisos del renderer por scripts no disponibles.

Se restituyó ARCore 6.3.5 en Library/PackageCache desde la copia oficial que se había descargado y validado. También se comprobó que ARBackgroundRendererFeature existe y coincide con la referencia del renderer. Fue una reparación local de caché; Library no se publica en Git y cada compañero debe importar sus paquetes.

La build jugable se pospone por decisión de Dilan. En su instalación se detectó que falta Android Build Support.

**Android Studio no es obligatorio para generar una APK desde Unity.** Unity Hub permite instalar Android Build Support, Android SDK & NDK Tools y OpenJDK para la versión correspondiente. Referencia: [documentación oficial de Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/android/getting-started/sdksetup/install-dependencies).

Los SDK o rutas locales de la computadora del compañero no se transfieren automáticamente con Git. Quien genere la build debe configurar sus herramientas Android.

## 10. Qué corresponde continuar a cada persona

| Persona | Próximo trabajo recomendado |
| --- | --- |
| 1 — AR | Revisar tracking, anclas, permisos, reanudación y compatibilidad en teléfonos. Coordinar cualquier cambio a cámara o SpawnPoint. |
| 2 — Combate, Dilan | Ajustar daño, velocidades, distancias y radios con pruebas reales; corregir incidencias de las tres armas y enemigos. |
| 3 — Juego/UI | Ordenar el controlador de partida y el HUD definitivo, manteniendo las conexiones de vida, puntos, oleadas, cambio de arma y Retry. |
| 4 — Arte/Integración | Sustituir placeholders por modelos, materiales, partículas, audio y HUD final; conservar los componentes técnicos y revisar rendimiento. |

Boss, menús definitivos, pickups, progresión y multijugador no están implementados en nuestro módulo. Deben acordarse con el equipo antes de ampliar el alcance.

## 11. Cómo recibir esta entrega

Si es una copia nueva del repositorio:

    git clone --branch Devmodule/dilan https://github.com/Nazareth75/RiftHuntersAR.git

Si ya tienen un clon, primero guardar o confirmar sus cambios locales y después:

    git fetch origin
    git switch --track origin/Devmodule/dilan

Si la rama Devmodule/dilan ya existe localmente, usar git switch Devmodule/dilan y revisar su estado antes de actualizarla. No sobrescribir cambios locales para cambiar de rama.

Abrir con Unity 6000.3.16f1 y esperar la importación de paquetes. Probar primero PruebaCombate sin AR. Para continuar el juego compartido, abrir ARCombate.

Que todos revisen esta base no significa que deban trabajar sobre una única rama: cada persona mantiene su rama de módulo y coordina la integración. Evitar modificar simultáneamente la misma escena.

Historial de esta entrega:

- ea171b9: módulo independiente de combate.
- 9590b7c: merge de art-integration en Devmodule/dilan.
- 87039d6: conexión del combate con el prototipo AR.
- Este informe se entrega en un commit adicional de documentación.

Se conservó un respaldo local de configuración antes del merge. .vscode, cachés, Library, Temp y builds no forman parte de la entrega. main no se modificó.
