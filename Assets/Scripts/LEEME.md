# Modulo de combate

Escena de prueba: Assets/Scenes/Pruebas/PruebaCombate.unity.
Abrir la escena con doble clic desde Project. Esperar la compilacion y pulsar Play.

## Componentes
- Arma: disparo, municion, cadencia y recarga compartidos.
- ArmaPlasma: dano a un enemigo.
- ArmaElectrica: cadena de hasta tres enemigos distintos.
- ArmaExplosiva: dano directo y en area sin duplicar impactos.
- GestorArmas: seleccion, disparo y recarga del arma actual.
- Enemigo: vida, movimiento, ataque y aviso de muerte.
- GeneradorEnemigos: aparicion de un grupo desde el portal.
- ControlesPruebaCombate: configuracion y HUD exclusivos de la prueba.

## Conexion con los companeros
- AR: asignar la camara a las armas con AsignarCamara(camara).
- AR: configurar GeneradorEnemigos con el prefab, el punto del portal y el jugador.
- Juego: llamar GenerarGrupo(cantidad, intervalo) al comenzar una oleada.
- Juego: suscribirse a EnemigoGenerado para conectar cada enemigo.
- Vida: escuchar AtacoJugador; recibe la cantidad de dano.
- Puntuacion: escuchar Murio y leer los Puntos del enemigo.
- HUD: leer GestorArmas.Actual y escuchar ArmaCambiada / MunicionCambiada.
- Botones: llamar Disparar(), Recargar() y SeleccionarArma(0, 1 o 2).
- Arte: sustituir el hijo Visual de los prefabs. Conservar el objeto raiz con Enemigo y CapsuleCollider.
- Los prefabs se encuentran en Assets/Prefabs/Enemigos.
- La prueba y el juego utilizan referencias a prefabs del Inspector.

## Convenciones
Clases, metodos propios, variables y comentarios en espanol, sin tildes en identificadores.
Awake, Start, Update, OnGUI y otras APIs de Unity conservan sus nombres oficiales.
Assets, Editor y Resources conservan los nombres que Unity necesita. Scripts, Scenes y Prefabs mantienen la convencion del proyecto.
FormerlySerializedAs conserva valores anteriores del Inspector tras los cambios de nombres.
Los archivos .meta mantienen la identidad de scripts, prefabs y escena; deben incluirse en Git.

## Respuesta visual
- TrazoDisparo escucha DisparoRealizado; asignar el arma y un material URP Unlit.
- RespuestaVisualEnemigo escucha VidaCambiada y produce un destello sin modificar el material compartido.
- La mira de prueba se pone verde al apuntar a un enemigo y muestra Vida / VidaMaxima.
- Los prefabs incluyen RespuestaVisualEnemigo; sus valores se ajustan desde el Inspector.

## Prefab del sistema de armas
Ruta: Assets/Prefabs/Armas/SistemaArmas.prefab.
Contiene GestorArmas, las tres armas y los efectos de disparo. No contiene HUD ni controles de prueba.
Los materiales se guardan en Assets/Materials/Combate.
La escena PruebaCombate instancia este mismo prefab al iniciar.

1. Arrastrar SistemaArmas.prefab a la escena AR.
2. Asignar la camara AR a las tres armas desde el Inspector, o ejecutar:

    gestorArmas.AsignarCamara(camaraAR);

3. Conectar los botones del HUD a GestorArmas:
   Disparar(), Recargar(), SeleccionarArma(0), SeleccionarArma(1), SeleccionarArma(2).
4. Al reintentar la partida, llamar ReiniciarArmas().
5. La recarga y el disparo siguen funcionando sin ControlesPruebaCombate.
6. Al terminar una partida, llamar DetenerCombate() y generador.Limpiar().

## Spawner en AR
Crear un objeto con GeneradorEnemigos y asignar:
- Prefab Enemigo: uno de los tres prefabs.
- Punto Aparicion: un Transform ubicado en el portal, a nivel del suelo.
- Objetivo: el Transform de la camara AR.

Tambien puede configurarse por codigo:

    generador.Configurar(prefabEnemigo, puntoPortal, camaraAR.transform);
    generador.EnemigoGenerado += ConectarEnemigo;
    generador.GenerarGrupo(5, 1f);

En ConectarEnemigo, suscribirse a Murio y AtacoJugador.
Murio entrega el Enemigo: leer enemigo.Puntos.
AtacoJugador entrega un int con el dano.
GeneradorEnemigos no decide las oleadas ni administra la vida del jugador.
No iniciar un segundo grupo mientras EstaGenerando sea true.
Al terminar o reintentar, llamar Limpiar().

## Pausa y fin de partida
- Pausar el combate con gestorArmas.Pausar(true) y generador.Pausar(true).
- Reanudar ambos con Pausar(false). La recarga conserva el tiempo que faltaba.
- La pausa no cambia Time.timeScale ni detiene el seguimiento de la camara AR.
- Para terminar: gestorArmas.DetenerCombate() y generador.Limpiar().
- Para reintentar: gestorArmas.ReiniciarArmas(), configurar el generador y crear el grupo.
- EnemigosVivos indica cuantos enemigos generados siguen presentes.
- EnemigosAgotados se emite una vez cuando no quedan enemigos ni apariciones pendientes.
- Si se agregan varios grupos antes de despejar la zona, el evento espera a todos.
- Limpiar cancela enemigos y apariciones sin emitir EnemigosAgotados ni sumar puntos.
- En PruebaCombate, usar P o PAUSAR para comprobar la pausa durante una recarga.
