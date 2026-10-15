# Rift Hunters AR — integración final

## Historial y conservación

Base local inicial: 0c2f13e74874651a53c894934ab563e8b13f9e88, integracion-equipo.
Se incorporó por fast-forward el avance remoto 8ecf5f8 (movimiento y separación de enemigos) y se combinó 032c81f de Kathlen sin conflictos. El merge se dejó sin commit hasta verificar la APK, por instrucción del usuario.

Se conservaron AR, tracking, anchors, colocación, cámara, portal/SpawnPoint, lógica de Dilan, vida/flujo de Kathlen, ocho FBX y 36 PNG aprobados, materiales y VFX. No se regeneró arte. Las capturas opcionales de la rama de Kathlen no se incorporan al commit de integración. No se incluyen cambios locales anteriores de ProjectSettings.asset ni UnityConnectSettings.asset.

## Oleadas, sin alterar las cantidades de Kathlen

1. Cinco Basic.
2. Tres Basic, dos Fast, dos Basic, dos Fast (5 Basic + 4 Fast).
3. Tres Fast, tres Basic, cuatro Fast (3 Basic + 7 Fast).
4. Un Tank, tres Basic, dos Fast, un Tank (2 Tank + 3 Basic + 2 Fast).
5. Guardian después de las cuatro oleadas.

PlanOleadas conserva sus anuncios y tiempos. En la prueba completa tras Retry: 32 enemigos y 4650 puntos, Victory.

## Modelos comprobados en instancias reales

CatalogoPartida -> AdaptadorGenerador -> GeneradorEnemigos -> prefab funcional -> ModeloDeIntegracion -> MeshFilter/FBX:

- EnemigoNormal -> EnemyBasicPlaceholder -> EnemyBasic_Final.fbx.
- EnemigoRapido -> EnemyFast_Visual -> EnemyFast_Final.fbx.
- EnemigoTanque -> EnemyTank_Visual -> EnemyTank_Final.fbx.
- EnemigoJefe -> RiftGuardian_Visual -> RiftGuardian_Final.fbx.
- PortalPlaceholder -> Portal_Final.fbx y SpawnPoint.

Las alturas visuales observadas son aproximadamente 0.60, 0.52, 0.95 y 1.85 unidades, respectivamente, con raíz funcional escala (1,1,1). No se modificaron HP, daño ni velocidades para corregir escala.

## Armas y teléfono

WeaponVisuals escucha GestorArmas.ArmaCambiada y activa exclusivamente PlasmaBlaster_Visual, ElectricCannon_Visual o EnergyLauncher_Visual. Sus referencias se guardan en ARGame. No modifica las clases de daño, cadena ni área. Tres botones táctiles PLASMA/ELECTRIC/ENERGY usan los iconos aprobados y llaman a ARPrototypeController.SeleccionarArma. Se evita el selector provisional sobre el texto de munición cuando el selector nuevo está presente. El soporte de primera persona conserva su escala y se encuadra en Portrait.

Pruebas: Plasma consume una bala/aplica daño/suma puntos una vez; Electric afecta tres objetivos por 35 HP; Energy aplica 80 al directo y 40 a los otros dos; cargadores 30/8/5 y recarga temporizada. Las ocho esquinas del volumen de cada arma quedan en el viewport de Portrait y por delante del near clipping plane. Solo hay un modelo activo.

## Guardian

Se reutiliza NucleosJefe: brazos 100 HP cada uno, centro 150 HP, cuerpo protegido, centro bloqueado hasta romper ambos brazos, muerte/puntos/Victory únicos. Los colliders funcionales siguen los renderers WeakPoint_Left_Mesh, WeakPoint_Right_Mesh y WeakPoint_Core_Mesh sin crear nuevos modelos. Se ocultan las esferas de presentación y los weak points rotos. El collider genérico del cuerpo se ajusta al torso Body_Mesh para no tapar los núcleos; el jefe aparece orientado al jugador. La prueba confirma la alineación y el primer impacto de raycast en cada núcleo.

## Audio

Se analizó la duración y amplitud de los MP3; los originales permanecen intactos en la carpeta sonidos. Se conservan los clips CC0 existentes para otras acciones. Se reutilizan los doce canales de SonidoJuego y los eventos de SonidosPartida. Las copias utilizadas se prepararon como WAV mono 32 kHz con duración acotada, fundidos y pico limitado; Audio.json registra duración, inicio y amplitud exactos.

| Original | Importado | Acción |
|---|---|---|
| arma-2.mp3 | Assets/Resources/Sonidos/Usuario/plasma.wav | Disparo Plasma |
| arma-laser.mp3 | Assets/Resources/Sonidos/Usuario/electrica.wav | Disparo Electric |
| arma-3.mp3 | Assets/Resources/Sonidos/Usuario/energia.wav | Disparo Energy |
| mounstruo.mp3 | Assets/Resources/Sonidos/Usuario/enemigo.wav | Muerte de enemigo normal/rápido/tanque |
| game-over.mp3 | Assets/Resources/Sonidos/Usuario/game_over.wav | Entrada en Game Over |
| next-level.mp3 | Assets/Resources/Sonidos/Usuario/next_level.wav | Entrada en waves 2, 3 y 4 |

El archivo arma-4 (40.2 s) no se importó. sonido_de_arma1 no se añadió como duplicado. La prueba registra exactamente una reproducción de Game Over y tres next-level en el recorrido completo, además de eventos reales para los otros cuatro clips. Esto comprueba reproducción por AudioSource, no la mezcla percibida en un teléfono físico.

## HUD y pruebas

Se conservaron ResultPanel, título, estadísticas, Retry/MainMenu, anuncios, mensaje de jefe y Boss HUD de Kathlen. Se comprobó que las regiones de resultados no se superponen. El texto de munición adapta tamaño para mostrar nombre y cargador; las armas/selector/mira se ocultan durante resultados. El selector permanece dentro de SafeArea y no invade disparar, recargar ni HUD esencial.

Prueba_Completa.txt documenta el recorrido real MainMenu -> Iniciar -> ARGame, placement de fixture/SpawnPoint, cuatro oleadas y cuatro FBX instanciados, tres modelos/selección móvil, daño/cadena/área, ammo/recarga, vidas/score, Game Over, Retry, núcleos/Boss HUD, Victory y regreso al menú. Las posiciones/velocidades/tiempo del fixture solo existen en Play Mode y no se guardan en assets. Se inspeccionaron capturas reales de Game Over, Victory, Boss y armas. Resultado: PASS, errores=0.

Referencias.txt: todas las escenas abren; Missing Scripts=0; referencias serializadas rotas de escenas y prefabs=0. Los warnings de los seis subsistemas AR ausentes en Editor no son errores de ARCore en dispositivo.

## Pendiente exclusivamente en teléfono

ARCore/permisos/cámara, superficies reales, tracking y anchors, colocación/estabilidad del portal, escala física, rendimiento, mezcla/volumen real, respuesta táctil, SafeArea con recortes físicos, recorrido completo de cuatro waves y Guardian en dispositivo.

No main. No force push. No eliminación de ramas. APK y archivos de validación excluidos de Git.

## APK verificada

RiftHuntersAR_IntegracionEquipo_Final.apk: 70.890.573 bytes; SHA256 4B5C457689E1257E1D829AB5E6C1DE04612C96CC2E58141958C0B2D8F7DFA28C. Build Succeeded, Errors 0, Warnings 1. Firma APK v2 válida. com.rifthunters.ar, API 25/36, ARM64/IL2CPP, GLES3, Portrait, cámara y ARCore requeridos. Primer intento interrumpido por BeeDriver; reintento completo exitoso (499 s). No se actualizaron paquetes. Build anterior y cachés no se versionan.
