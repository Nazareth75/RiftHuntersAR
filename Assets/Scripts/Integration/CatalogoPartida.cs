using UnityEngine;
using RiftHunters.Combate;

namespace RiftHuntersAR.Integration
{
    public sealed class CatalogoPartida : ScriptableObject
    {
        public GestorArmas sistemaArmas;
        public Enemigo normal;
        public Enemigo rapido;
        public Enemigo tanque;
        public Enemigo jefe;
    }
}
