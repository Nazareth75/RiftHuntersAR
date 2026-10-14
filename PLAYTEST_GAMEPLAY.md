# Rift Hunters AR — Playtest de gameplay

HECHO: lógica de gameplay con placeholders. Biblia visual de Rift Hunters AR queda como referencia para sustituir Visual posteriormente. Sin arte final ni Boss/otras armas.

BUG DEL DISPARO: el prefab real tiene 100 HP y un CapsuleCollider; la escena tiene 25 damage y cero listeners persistentes de SHOOT. Antes había una suscripción en Start y un solo TakeDamage por raycast. No se reprodujo ni se confirmó la causa del supuesto disparo letal en Android. Se añadió guardia de un disparo por frame, cooldown 0,2 s y registro/desregistro del listener al habilitar/deshabilitar. Feedback: HIT -75 HP, HIT -50 HP, HIT -25 HP, ENEMY DESTROYED +100. Logs [Rift Shot] con target, damage, HP y ammo permiten investigar.

GAMEPLAY: SCANNING → PORTAL_PLACED → COUNTDOWN → WAVE → WAVE_COMPLETE → NEXT_WAVE → VICTORY; GAME_OVER al morir. RETRY conserva el portal anclado, limpia enemigos sin puntuarlos, restaura HP/munición/score y reinicia Wave 1. MAIN MENU pendiente para después.

PLAYER: 100 HP y LIFE 3/3 temporal. Un Basic inflige 34 HP; tres ataques producen Game Over. Una sola notificación de muerte; Retry permite otra partida.

ENEMIGOS: EnemyHealth y EnemyMovement reutilizables. Basic: 100 HP, velocidad 0,25 m/s, ataque a 0,65 m, daño 34, cooldown 2 s. Avance horizontal hacia la cámara, altura del suelo constante, sin NavMesh. Pausa sin SessionTracking. Collider y AimPoint separados de Visual. maxHealth/speed/damage serializados permiten configurar futuros Fast 50 HP y Tank 250 HP sin duplicar lógica. No hay variantes visuales finales. No se evitan obstáculos ni se separan enemigos entre sí todavía.

OLEADAS: 3, 5 y 7 Basic; aparición cada 2 s con tracking. Countdown 3 s, Wave Complete 3 s, Next Wave 1 s. Hay que eliminar todos antes de avanzar. EnemySpawner utiliza PortalSpawnPoint; lógica de oleadas en controlador y no en visual del portal.

ARMA: Plasma única, 30/30 inicial, -1 por disparo aceptado incluido MISS, bloqueo con 0, RELOAD instantáneo a 30 durante WAVE; recarga también entre oleadas. SHOOT requiere tracking y WAVE.

UI: temporal: vida, score, Wave/enemigos restantes, munición, SHOOT, RELOAD, RETRY, HP y muerte, X cian al acertar y flash rojo del indicador de vida al recibir daño. Sin VFX pesados.

PRUEBAS: menú Rift Hunters/Test Playtest. Audita prefab/escena reales y usa Physics.Raycast contra el prefab: cuatro impactos dejan 75/50/25/0; rechaza duplicados de frame/cooldown; comprueba cero munición, recarga, muerte única, Retry, avance horizontal, distancia mínima, pausa por tracking y cooldown de ataque. Smoke test de muerte/score +100 también pasa. HUD comprobado en una entrada limpia a Play con 100 HP y 30/30, sin excepciones nuevas. El recorrido completo de oleadas en AR necesita prueba física del nuevo APK; no se declara validado automáticamente.

APK: RiftHuntersAR_Playtest.apk separado de RiftHuntersAR_Prototype.apk. Misma identidad de aplicación: instalarlo actualiza la app del teléfono. Se conserva el APK anterior.

GIT: rama art-integration, commits locales pequeños; sin push. Cambios previos de ProjectSettings conservados fuera de estos commits.

ERRORES: causa del disparo letal original pendiente de reproducir en Android. Hubo una excepción durante hot reload de los scripts sobre la sesión antigua de Play; al iniciar una sesión limpia el HUD pasó Awake y no apareció. Avisos de subsistema XR ausente en Windows esperados.

SIGUIENTE: probar portal, cuatro impactos por Basic, oleadas 1/2/3, munición y recarga, daño y Game Over, Retry y Victory. Quince bajas deben sumar 1500 puntos. Ajustar dificultad con resultados físicos antes de integrar arte.

## Resultado de compilación

Unity: Succeeded, 0 errores, 2 advertencias. Advertencias: Diagnostics Data sin símbolos completos y modificación detectada en el recurso de pruebas TestReferenceImageLibrary.asset del paquete ARCore; no son errores de compilación del gameplay. No se editó manualmente el código del paquete.

APK verificado con apksigner: firma v2 válida, un firmante. Paquete com.rifthunters.ar, ARM64, min SDK 25 y target 36. Tamaño 70.803.396 bytes. SHA256: 0A4813291385BC66863EB2BB56B522CFE93AAD4E3CB768152CA208FB577C4182.

Archivo para adjuntar como documento en WhatsApp:
C:\Users\HP\Documents\Codex\2026-10-13\files-pasted-by-the-user-quiero\outputs\RiftHuntersAR_Playtest.apk

Commits: 3db32cd (disparo/munición), dd2f28e (gameplay), 01059a9 (pruebas/build/documentación). El APK anterior conserva SHA256 2527BFD5E1D4838F90DF9F905AD462B428100D63D2D65951F1226CE6C918B3E7.
