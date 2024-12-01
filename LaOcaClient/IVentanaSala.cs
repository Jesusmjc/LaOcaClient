using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaOcaClient
{
    public interface IVentanaSala : IServicioActualizacionJugadoresEnSalaCallback
    {
        LaOcaService.Sala SalaActual { set;  get; }

        ServicioActualizacionJugadoresEnSalaClient ClienteJugadoresEnSala { set; get; }
    }
}
