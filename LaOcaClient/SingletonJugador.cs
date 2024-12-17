using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaClient
{
    public class SingletonJugador
    {
        private static SingletonJugador _Instance;
        private static readonly object _LockObject = new object();

        public Jugador Jugador {  get; set; }

        public bool EsInvitado { get; set; }


        private SingletonJugador() { }

        public static SingletonJugador Instance
        {
            get
            {
                lock (_LockObject)
                {
                    if (_Instance == null)
                    {
                        _Instance = new SingletonJugador();
                    }
                }

                return _Instance;
            }
        }
    }
}
