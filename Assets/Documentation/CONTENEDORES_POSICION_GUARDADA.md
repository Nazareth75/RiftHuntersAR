# Contenedores con posición guardada

Antes, cada recuperación buscaba automáticamente una distribución alrededor del portal. El plano pequeño podía impedir colocar los tres objetos, activando la clasificación con botones, y la búsqueda podía elegir el espacio detrás del portal. Los contenedores se destruían al terminar la recogida.

Ahora la primera recuperación que tenga residuos pide elegir la posición:

1. Apuntar al suelo ya reconocido, delante del portal o hacia un lado.
2. Mover el teléfono hasta ver la vista previa verde con los tres contenedores.
3. Tocar el escenario o **COLOCAR AQUÍ**. El botón solo se habilita si cabe la fila completa y hay tracking.
4. Clasificar la basura como antes.
5. Durante las oleadas, la tienda y los mensajes finales, los contenedores se ocultan. En la siguiente recogida reaparecen como los mismos objetos en la misma posición, sin otra confirmación.

La fila comparte el ancla del portal; no sigue la cámara. Se guarda dentro de esa sesión y también se conserva al usar Retry mientras se mantiene el mismo portal. Al salir de la escena se descarta: no se guarda una coordenada AR para reutilizarla después de cerrar la aplicación.

**REUBICAR CONTENEDORES** permite elegir otra posición durante la recogida. Si la superficie reconocida no alcanza, **USAR BOTONES** activa explícitamente la clasificación compacta y recuerda esa elección. Reubicar permite volver a intentar la colocación física.

El sistema comprueba que la huella completa de los tres basureros quede dentro del suelo guardado, separados del portal y su aparición de enemigos, dentro de la vista y sin quedar detrás del portal. Prueba tamaños pequeños de forma automática. No vuelve a escanear toda la habitación ni crea otro ancla. El soporte representa el suelo del portal, no detecta mesas ni obstáculos reales nuevos.

Cambios principales: nuevo `ColocacionContenedores`, integración en `RecyclingGameplay`, controles en `RecyclingHUD` y adaptación de las pruebas de reciclaje. El sistema de oleadas y la economía permanecen conectados al mismo flujo.

Validación: una copia aislada de Unity completó las doce oleadas, jefes, recogidas y tienda sin errores de ejecución, confirmando una sola ubicación y comprobando que los mismos contenedores reaparecen en el mismo sitio. Falta verificar comodidad, tracking y colocación física en el teléfono.

Para comprobarlo en Android hace falta generar e instalar una APK nueva. La APK anterior conserva el comportamiento anterior.
