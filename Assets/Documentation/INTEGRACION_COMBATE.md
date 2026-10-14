# Integracion de combate y AR

Rama de trabajo: Devmodule/dilan.
Base integrada: origin/art-integration (b5d19e7).
Merge: 9590b7c. Respaldo del combate anterior: ea171b9.

## Escenas
- Assets/Scenes/Gameplay/ARCombate.unity: escena integrada para continuar el desarrollo.
- Assets/Scenes/Gameplay/ARGame.unity: prototipo original, conservado para comparacion y pruebas existentes.
- Assets/Scenes/Pruebas/PruebaCombate.unity: prueba de combate con camara normal, sin telefono.

ARCombate es la primera escena habilitada en Build Settings.
La herramienta de build del prototipo elige ARCombate cuando existe.

## Conexiones
EnlaceCombateAR conecta SistemaArmas y GeneradorEnemigos con PortalSpawnPoint, PlayerHealth y ScoreManager.
ARPrototypeController conserva deteccion de planos, colocacion con ancla, estados, cuenta regresiva, oleadas y reintento.
PlasmaWeapon conserva Shoot, Reload, Ready y Feedback para los botones y pruebas del prototipo. En ARCombate delega el disparo a GestorArmas.
Cada escena tiene un unico sistema de combate activo. ARGame utiliza el prototipo; ARCombate utiliza nuestras tres armas.

Los enemigos se colocan bajo el ancla del portal.
La falta de tracking o el cambio de estado pausa enemigos, apariciones y recarga.
Al morir el jugador se detiene el combate y se limpian los enemigos.
Retry conserva el portal, restaura vida/puntos/armas y vuelve a la oleada 1.

## Balance actual
- Plasma: 20 de dano, 30 disparos, recarga de 1,5 segundos.
- Electrica: 35 de dano a hasta tres enemigos, 8 disparos, recarga de 2 segundos.
- Explosiva: 80 directo o 40 en area, 5 disparos, recarga de 2,5 segundos.
- Normal: 50 HP, velocidad 0,6 m/s, 100 puntos.
- Rapido: 30 HP, velocidad 1,2 m/s, 150 puntos.
- Tanque: 200 HP, velocidad 0,3 m/s, 300 puntos.
- Jugador: 100 HP. Normal/rapido restan 34; tanque resta 68.
- Los enemigos desaparecen despues de atacar una vez, siguiendo nuestro modulo.

Oleada 1: 3 normales.
Oleada 2: 4 normales y 1 rapido.
Oleada 3: 4 normales, 2 rapidos y 1 tanque.
Eliminar todos sin dejar que ataquen suma 1850 puntos.
Entre oleadas se reponen los cargadores y se selecciona plasma.

## Prueba pendiente en telefono
Tracking, colocacion, cambio de armas, cadenas electricas, explosion, pausa/reanudacion, Game Over y Retry requieren validacion fisica de esta integracion.
Los reportes Android anteriores pertenecen al prototipo del companero y no validan estos cambios.

## Equipo local
Unity 6000.3.16f1.
Android Build Support no esta instalado en esta instalacion de Unity. Instalarlo desde Unity Hub antes de generar un APK.
AR Foundation y ARCore 6.3.5 estan declarados en Packages y se descargan al importar el proyecto.
Las preferencias .vscode locales se conservan fuera de commits.
El stash de respaldo de configuracion anterior al merge se conserva para recuperacion.

## Verificacion realizada
Compilacion completa y pruebas automatizadas en Unity 6000.3.16f1, sobre una copia temporal.
- Pruebas originales del prototipo: cuatro impactos, muerte/puntuacion unica, bloqueo de duplicados, municion, Retry, movimiento horizontal, pausa por tracking y cooldown de ataque.
- ARCombate: referencia al enlace, sistema de armas y tres variantes, sin scripts propios faltantes.
- Plasma integrado: reduce 50 a 30 HP y consume una sola municion; rechaza el segundo disparo del mismo frame.
- Pausa: no permite disparar y conserva el estado de recarga. Fin de combate cancela esa recarga.
- Electrica: aplica 35 a cada uno de tres objetivos y consume una municion.
- Explosiva: aplica 80 al objetivo directo y 40 a los dos cercanos, sin duplicar dano.
- Cambio de arma: conserva los cargadores independientes.
- Puntuacion: 150 + 300 = 450. Vida: ataque de 68 deja 32 HP.
Estas pruebas de Editor no validan tracking AR real, el recorrido completo de oleadas ni el aspecto en telefono.
No se genero APK en este equipo.
