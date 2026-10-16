# Mejoras de colocación y suelo AR — Dilan

Base: `integracion-equipo`, commit `46d9d90`. Rama de trabajo: `Devmodule/dilan-mejoras`.

## Qué cambia

- `ColocacionPortalAR`: raycast bajo la mira sobre planos horizontales orientados hacia arriba. Previsualización con un aro de 40 segmentos, verde cuando se puede colocar y ámbar cuando falta una condición. Distancia horizontal entre 2 y 5 metros, margen de 45 cm alrededor del punto dentro del plano, diferencia de altura de al menos 50 cm respecto al teléfono y estabilidad del punto durante 0,35 segundos. Se solicita autofocus a la cámara. Los valores principales son configurables en el componente.
- `ARPrototypeController`: el toque fuera de la UI confirma el punto mostrado en la mira. El portal queda vertical y orientado hacia el jugador, unido al ancla del plano elegido. Solo acepta anclas con tracking completo y requiere 0,45 segundos de seguimiento continuo antes de reanudar. Los mensajes de poca luz, movimiento o falta de detalles también se muestran durante el escaneo inicial. Al volver de segundo plano se vuelve a comprobar estabilidad.
- `SueloCombateAR`: guarda una copia del contorno detectado en coordenadas del ancla. Limita el centro de los puntos de aparición al polígono y proyecta el movimiento a la altura del suelo anclado. No cambia de superficie durante la partida ni persigue continuamente las actualizaciones del plano.
- `GeneradorEnemigos`: apoya la aparición en el suelo y espera si el jugador está a menos de 1,4 metros de ese punto. Evita crear enemigos directamente sobre el jugador cuando se acerca al portal.
- `Enemigo`: proyecta el desplazamiento horizontal al suelo anclado y reinicia la preparación del ataque al reanudar. El jefe recibe un margen de 0,8 segundos antes de poder atacar tras una pausa.
- `ModeloDeIntegracion`: calcula la base del modelo aprobado y ajusta solo su posición visual para que los pies queden sobre el suelo. Ajusta el collider funcional a esa posición. El jefe sigue usando el ajuste específico del torso y sus tres núcleos.
- `GameManager`: detiene los tiempos de partida, anuncios y cuenta atrás mientras el controlador indica pausa o seguimiento insuficiente, sin cambiar globalmente `Time.timeScale`.
- `Assets/link.xml`: conserva `SphereCollider` para las esferas creadas durante ejecución. Corrige la dependencia que podía desaparecer en builds IL2CPP.

## Alcance físico

Se utiliza un suelo horizontal de juego. El contorno limita las apariciones, pero el movimiento puede continuar sobre la prolongación de ese suelo para aproximarse al jugador sin exigir escanear todo el recorrido. No hay detección semántica garantizada del piso: una mesa suficientemente baja también puede ser un plano horizontal. No se reconstruyen muebles, paredes ni escaleras; tampoco se añade oclusión por profundidad. El contorno limita el centro de aparición, no todo el volumen de un tanque o jefe.

El ancla conserva la referencia espacial de los objetos. ARCore puede corregir su estimación y aún puede haber deriva. Si el ancla queda sin seguimiento, se pausa la partida y se pide mirar la zona original; no se sustituye automáticamente por otra ancla ni se traslada el portal.

No hay una espera obligatoria para escanear toda la habitación ni un porcentaje ficticio. Los 0,35 segundos empiezan cuando ya existe un punto válido. La detección inicial depende de cámara, iluminación, textura y dispositivo.

## Compatibilidad

Se conservan Unity 6000.3.16f1, AR Foundation y ARCore 6.3.5, ARM64/IL2CPP y la configuración Android existente. No se exige Depth, ToF, Cloud Anchors, conexión de red para las nuevas reglas ni NavMesh dinámico.

El usuario identifica el teléfono como TECNO POVA 6 / LI7. La consulta de la lista oficial de Google no encontró ese modelo: no se afirma certificación ARCore ni soporte Depth. El APK anterior del usuario sí mostró cámara y planos; eso es evidencia de ejecución en ese teléfono, no una certificación de todas sus funciones.

Fuentes:
- https://developers.google.com/ar/devices
- https://developers.google.com/ar/develop/performance
- https://developers.google.com/ar/develop/depth
- https://docs.unity3d.com/ja/6000.0/ScriptReference/GameObject.CreatePrimitive.html

## Cómo probar

En Editor: abrir `MainMenu`, Play e iniciar. El modo de computadora conserva su colocación simulada y permite comprobar combate y progresión. La cámara AR, distancia física, reconocimiento y estabilidad del ancla requieren teléfono.

En el próximo APK: apuntar a suelo iluminado a 2–5 m, observar las indicaciones y confirmar tocando fuera de los botones cuando el aro sea verde. Revisar pies de los enemigos y distancia de aparición. Apartar el teléfono o tapar momentáneamente la cámara: el combate y los tiempos deben detenerse y reanudar tras recuperar seguimiento estable. Comprobar brazos, núcleo central y victoria del jefe. No se generó APK durante esta mejora.

## Validación realizada

- Compilación C# completa con referencias del Editor: correcta.
- Compilación C# completa con definiciones de Android: correcta. No incluye empaquetado Gradle ni validación del linker de un APK nuevo.
- Prueba en Unity de proyección al suelo, contorno de aparición y desplazamiento del ancla: PASS.
- El primer recorrido automatizado alcanzó cuatro oleadas, jefe, Victory y regreso al menú; su resultado global marcó FAIL por dos excepciones internas de UnityEditor.Search.SearchDatabase. No se presentan como errores del juego ni como una prueba global limpia.
- La comprobación adicional de modelos apoyados y pausa no produjo resultado final porque el Editor aislado quedó detenido al inicializar/importar. Se cerró únicamente esa instancia de pruebas. La ventana del proyecto del usuario permanece intacta.
- Falta validar la colocación y el seguimiento físico con un APK nuevo en el teléfono. El APK anterior no contiene estas mejoras.

## Corrección tras la prueba en teléfono — 15 de octubre de 2026

Esta revisión sustituye los tamaños y restricciones anteriores. El usuario reportó lentitud, tamaños excesivos, enemigos flotando y colocación difícil; no se considera validado el rendimiento físico de la revisión anterior.

- Alturas visuales físicas: normal 0,28 m; rápido 0,24 m; tanque 0,42 m; Guardian 0,80 m; portal 0,65 m. El cálculo usa únicamente geometría visible y compensa escalas heredadas. El apoyo y ajuste del collider se aplican también en Editor, no solo si existe el componente AR.
- Se desactiva la animación de posición de los fragmentos de criaturas para que el apoyo no se deshaga. Las armas en primera persona reducen su escala al 75 %.
- Colocación: distancia horizontal 1,1–4 m; margen del aro 18 cm; estabilidad 0,2 s; superficie al menos 65 cm por debajo del teléfono. Si ya se detectó un plano horizontal suficientemente amplio más bajo, se pide apuntar a ese piso en lugar de una superficie elevada. Esto es una heurística, no clasificación semántica garantizada.
- Raycast, revisión de bordes y aro a 10 Hz; buffer fijo de 24 puntos. Tras colocar el portal se solicita PlaneDetectionMode.None, conservando el manager y ancla, y se dejan de recorrer todos los planos cada frame.
- Recuperación de tracking completo tras 0,2 s. Continúa la pausa de combate y reloj; no se acepta tracking limitado como completo.
- Apariciones espaciadas 35 cm, separación entre enemigos 30 cm y espera si el punto queda a menos de 80 cm del jugador. Se ajustan velocidades y distancias de ataque a la escala menor; no cambian HP, daño, munición ni cantidades de oleadas.
- VisualMotion cachea cámara y arrays de materiales; los pulsos se actualizan a 10 Hz. El ajuste del collider del torso del jefe deja de recalcularse cada frame y sus núcleos se actualizan a 10 Hz. Se evita que la animación de esferas sobrescriba el tamaño de los colliders alineados al modelo.
- Pipeline móvil: render scale 0,7; HDR, sombras principales y luces adicionales desactivados. No se alteran APIs gráficas, versiones de paquetes ni permisos.
- Los mensajes de HP por impacto quedan solo en Editor, sin generar ese tráfico de consola en Android.

Validación: compilación C# Editor y Android; comprobación en Unity de los cuatro prefabs reales y portal con las alturas indicadas, base visible y collider apoyados, y tres SphereCollider del jefe. La primera ejecución del comprobador temporal encontró además llamadas Destroy fuera de Play en la limpieza heredada; se corrigieron con retirada segura de componentes, liberación de materiales y omisión de audio/VFX fuera de Play. La comprobación del Editor no es una medición de FPS ni valida el suelo real del teléfono. No se generó otro APK.

## Controles y fluidez — 15 de octubre de 2026

Respaldo previo publicado: a5c5ecc, rama Devmodule/dilan-mejoras.

- BotonDisparoSostenido se conecta al botón existente desde VisualHUD, sin regenerar escenas. Dispara inmediatamente al tocar y mantiene intentos mientras el dedo siga dentro. Cada arma conserva su cadencia. Al soltar, salir, bloquearse el botón o perder foco se cancela; el clic de liberación no añade otra bala. Las invocaciones de onClick por teclado o pruebas siguen disponibles.
- Arma utiliza primero el raycast exacto. Si no impacta directamente un enemigo, permite una tolerancia física de 3,5 cm y máximo 1,5 grados. Solo considera enemigos vivos y vulnerables, y comprueba línea de visión con otro raycast. No mueve la cámara, no atraviesa colliders y no elige núcleos protegidos como candidatos asistidos. Usa un buffer fijo de 32 impactos, sin listas de búsqueda por disparo.
- GestorArmas permite seleccionar durante recarga. Guardar un arma congela su tiempo de recarga pendiente y conserva sus balas; volver a seleccionarla reanuda ese tiempo. No se completa la recarga mientras está guardada. El HUD muestra su progreso porcentual. También se habilita el cambio durante recarga en la escena de pruebas de combate.
- PlasmaWeapon solo emite feedback cuando se consumió una bala. Evita repetir sonido y VFX del disparo anterior durante los intentos bloqueados por la cadencia.
- Enemigo reduce gradualmente el empuje de separación al acercarse al jugador para que su trayectoria lateral sea más fácil de seguir. No cambia vida, daño ni velocidad.
- GeneradorEnemigos alterna intervalos de 0,65 y 1,35 veces el intervalo del grupo y deja 0,7 s al terminar grupos de varios enemigos. Conserva los tipos, orden y cantidades del plan: 5/9/10/7 y un Guardian. No se añadieron modelos ni animaciones.

Pruebas realizadas en una copia aislada de Unity usando una escena física real: impacto exacto y cadencia; tolerancia junto al borde; pared que bloquea; exclusión de objetivo protegido; selección durante recarga; conservación del tiempo pendiente y de cargadores 30/8/5; ausencia de feedback duplicado; toque breve, mantener, soltar, salir del botón y botón bloqueado; cantidades de oleadas. Todas pasaron. Compilación C# para Editor y Android correcta. No se generó ni instaló un APK nuevo; falta valorar la comodidad táctil en el teléfono.

Prueba manual recomendada: mantener DISPARAR en lugar de pulsarlo repetidamente; soltar y comprobar que se detiene; rozar el borde de una criatura con la mira; intentar el mismo disparo detrás de un obstáculo con collider; recargar Plasma, cambiar a otra arma y volver para comprobar que retoma su recarga; observar las parejas y el movimiento cuando se acercan.
