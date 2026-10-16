# Tienda ECO y campaña de 12 oleadas

Implementación en `Devmodule/dilan-mejoras`, sobre la integración ambiental del equipo. Los cambios se aplican al proyecto original, en la escena `ARGame`; no hay que regenerar escenarios ni prefabs.

## Flujo de una partida

1. Colocar el portal y jugar la oleada.
2. Clasificar los residuos: cada clasificación correcta entrega **50 ECO**, una sola vez.
3. Ver el mensaje de zona recuperada.
4. Elegir **TIENDA** o **SIGUIENTE**. El combate permanece detenido mientras se decide.
5. En la tienda, elegir Plasma, Electric o Energy; comprar mejoras o recuperar vida. **VOLVER** regresa a los dos botones.
6. **SIGUIENTE** inicia la siguiente oleada con los cargadores restaurados y las mejoras conservadas.
7. Tras el jefe de la oleada 12, completar la última recogida conduce a Victoria.

Las compras pertenecen a la partida actual. Retry o una nueva partida restablecen la moneda y los atributos originales. El ECO ganado total se conserva separado del saldo durante la partida, para mostrarlo en las estadísticas finales aunque se haya gastado.

## Balance inicial

Cada arma tiene tres niveles por atributo, independientes de las otras armas.

| Atributo | Precio de cada nivel | Efecto acumulado sobre el valor original |
|---|---|---|
| Daño | 250 / 500 / 1000 ECO | +20% / +40% / +60% |
| Cargador | 200 / 400 / 800 ECO | +25% / +50% / +75%, redondeado hacia arriba |
| Tiempo de recarga | 200 / 400 / 800 ECO | −12% / −24% / −36% |

La mejora de daño de Energy también aumenta su daño de área. Comprar cargador agrega la diferencia de capacidad; al pasar de oleada se restaura la munición de las tres armas.

Recuperar **34 HP**, hasta 100, cuesta 200 ECO tras las oleadas 1–4, 250 tras 5–8 y 300 tras 9–11. No permite pagar con la vida completa ni revivir después de morir. Las compras sin saldo y las mejoras posteriores al nivel 3 se rechazan.

Maximizar las tres armas cuesta **13 650 ECO**. Con los límites de residuos, el ingreso de una partida está muy por debajo: conviene especializarse y reservar moneda para curarse. Es un primer balance funcional; la dificultad y la comodidad de compra necesitan una partida real en el teléfono para afinarse.

## Oleadas y rendimiento

| Oleada | Enemigos previstos |
|---|---|
| 1 | 5 básicos |
| 2 | 5 básicos y 2 rápidos |
| 3 | 5 básicos, 2 rápidos y 1 tanque |
| 4 | Jefe |
| 5 | 5 básicos, 3 rápidos y 2 tanques |
| 6 | 5 básicos, 4 rápidos y 2 tanques |
| 7 | 6 básicos, 4 rápidos y 2 tanques |
| 8 | Jefe reforzado |
| 9 | 7 básicos, 5 rápidos y 2 tanques |
| 10 | 5 básicos, 6 rápidos y 3 tanques |
| 11 | 5 básicos, 6 rápidos y 3 tanques |
| 12 | Jefe final y recuperación final |

- Como máximo **6 enemigos vivos a la vez** y **14 enemigos previstos por oleada normal**. Los pendientes esperan; no se eliminan del desafío.
- Como máximo **10 residuos en la zona**, incluidos los que están cayendo. La recogida anterior se limpia antes de la siguiente oleada.
- Vida normal: +13% del valor original por oleada transcurrida.
- Velocidad normal: +15% inicial, con un incremento gradual del 2% por oleada transcurrida sobre esa velocidad.
- Jefes: resistencia de los núcleos ×1, ×1,45 y ×1,9 en las oleadas 4, 8 y 12; los posteriores se mueven algo más rápido y atacan con menor intervalo. Se mantiene el orden brazos → centro.
- Los enemigos eliminados antes de contaminar también pueden dejar un residuo, si hay espacio, para que matar rápido no suprima toda la recompensa ECO.

El tamaño y el anclaje AR existentes se mantienen. La clasificación compacta sigue disponible cuando no caben contenedores físicos; no exige reiniciar el escaneo.

## Responsabilidad de los cambios

- El equipo aportó la integración AR, modelos, interfaces, animación procedural, contaminación, clasificación, recuperación y pantallas de resultados.
- Esta mejora conecta ese reciclaje con una economía de partida y tienda, añade la progresión de doce oleadas, escala enemigos y jefes y limita las entidades activas.
- `BalancePartida`: límites y multiplicadores de dificultad.
- `PlanOleadas`: cantidades, tipos y cadencia de los grupos.
- `GameManager`: espera entre oleadas, botón Siguiente, jefes 4/8/12 y Victoria final.
- `TiendaECO`: precios, saldo, niveles, curación y reinicio.
- `InterfazTienda`: interfaz con la tipografía, colores cian/oscuros e iconos de armas existentes.
- `Arma`, `ArmaExplosiva`, `GestorArmas`: estadísticas mejorables y acceso a las tres armas.
- `RecyclingGameplay`: ECO ganado y saldo gastable, enlace de tienda con recuperación.
- `GeneradorEnemigos`, `PollutionZone`, `RecyclableItem`, `EnemyPollutionEmitter`: límites y registro de enemigos/residuos.
- `AdaptadorGenerador`, `NucleosJefe`: dificultad por oleada y resistencia escalada de los núcleos.
- `PlayerHealth`: curación limitada; el controlador evita mostrar daño al curarse.
- `VisualHUD`: contador de doce oleadas y ECO ganado en resultados.

## Cómo probarlo

1. Esperar a que Unity termine de compilar y abrir `Assets/Scenes/Gameplay/ARGame.unity`.
2. En PC, usar el modo de prueba de escritorio existente; en Android, colocar el portal como siempre.
3. Completar la primera oleada y recoger sus residuos. Deben aparecer Tienda y Siguiente después del mensaje.
4. Entrar a Tienda: comprobar saldo, pestañas y precios; comprar una mejora y verificar que el saldo baja exactamente su coste.
5. Intentar comprar sin suficiente saldo: el botón debe quedar desactivado. Una vida completa tampoco permite comprar curación.
6. Volver y pulsar Siguiente: comienza la oleada 2, conserva la mejora y restaura munición. El tiempo de tienda no cuenta como tiempo de partida.
7. Tras recibir daño, probar la curación: suma hasta 34 HP y nunca supera 100.
8. Revisar los jefes de las oleadas 4, 8 y 12: primero ambos brazos, luego el centro. El último solo termina la partida después de recoger sus residuos.
9. Usar Retry: vuelve a la oleada 1, vida completa, saldo cero y armas originales.

Se verificó el flujo real de ARGame en una copia aislada con Unity 6000.3.16f1: doce oleadas, jefes, clasificación, compras de todos los niveles de las tres armas, rechazo de compras inválidas, curación, espera explícita, límite real de seis vivos y reinicio, sin errores de ejecución. También compiló el código de ejecución con las referencias de Android. Esto no sustituye una prueba de rendimiento y tacto en el teléfono. No se generó una nueva APK para esta entrega.
