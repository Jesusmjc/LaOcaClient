using LaOcaClient.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LaOcaClient.LaOcaService;
using System.ServiceModel;

namespace LaOcaClient
{
    /// <summary>
    /// Interaction logic for Partida.xaml
    /// </summary>
    public partial class Partida : Window, IServicioPartidaCallback
    {
        private LaOcaService.Sala sala;

        private LaOcaService.ServicioPartidaClient clientePartida;

        private Grid[] gridsJugadores;

        public Partida()
        {
            InitializeComponent();
        }

        public Partida(LaOcaService.Sala sala)
        {
            InitializeComponent();

            gridsJugadores = new Grid[4];
            gridsJugadores[0] = gridJugador1;
            gridsJugadores[1] = gridJugador2;
            gridsJugadores[2] = gridJugador3;
            gridsJugadores[3] = gridJugador4;

            this.sala = sala;

            AgregarCanalCallbackDePartida();
            MostrarJugadoresEnPartida();
            MostrarJugadorEnTurno();
        }

        private void AgregarCanalCallbackDePartida()
        {
            InstanceContext contexto = new InstanceContext(this);
            clientePartida = new LaOcaService.ServicioPartidaClient(contexto);

            clientePartida.AgregarCanalCallbackPartida(SingletonJugador.Instance.Jugador.NombreUsuario, sala.Codigo);
        }

        private void MostrarJugadoresEnPartida()
        {
            for (int i = 0; i < sala.Jugadores.Count; i++)
            {
                Jugador jugador = sala.Jugadores[sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[i]];
                JugadorEnSala jugadorEnSala = new JugadorEnSala(jugador, sala.Codigo);
                if (SingletonJugador.Instance.Jugador.NombreUsuario.Equals(sala.NombreHost))
                {
                    jugadorEnSala.CargarOpcionExpulsar();
                }

                gridsJugadores[i].Children.Add(jugadorEnSala);
            }
        }

        private void MostrarJugadorEnTurno()
        {
            string nombreJugadorEnTurno = sala.Partida.NombreJugadorEnTurno;
            lbNombreJugadorEnTurno.Content = nombreJugadorEnTurno;

            if (nombreJugadorEnTurno.Equals(SingletonJugador.Instance.Jugador.NombreUsuario))
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("¡Es tu turno de tirar!", "Hora de jugar", 3);
                ventanaTurno.Show();

                btnDados.IsEnabled = true;
            }
            else
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("Es turno de " + nombreJugadorEnTurno, "Hora de jugar", 3);
                ventanaTurno.Show();

                btnDados.IsEnabled = false;
            }
        }

        private void LanzarDados(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int numeroAleatorio = random.Next(2, 13);

            MessageBox.Show("Ha salido " + numeroAleatorio);

            int posicionJugadorActual = Array.IndexOf(sala.Partida.NombresDeJugadoresEnOrdenDeTurnos, sala.Partida.NombreJugadorEnTurno);

            string nombreSiguienteJugador = clientePartida.PasarTurnoASiguienteJugador(posicionJugadorActual, sala.Codigo);

            sala.Partida.NombreJugadorEnTurno = nombreSiguienteJugador;
            lbNombreJugadorEnTurno.Content = nombreSiguienteJugador;

            if (nombreSiguienteJugador.Equals(SingletonJugador.Instance.Jugador.NombreUsuario))
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("¡Es tu turno de tirar!", "Tu turno", 2);
                ventanaTurno.Show();

                btnDados.IsEnabled = true;
            }
            else
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("Es turno de " + nombreSiguienteJugador, "Hora de jugar", 2);
                ventanaTurno.Show();

                btnDados.IsEnabled = false;
            }
        }

        public void MostrarNuevoJugadorEnTurno(string nombreNuevoJugadorEnTurno)
        {
            sala.Partida.NombreJugadorEnTurno = nombreNuevoJugadorEnTurno;
            lbNombreJugadorEnTurno.Content = nombreNuevoJugadorEnTurno;

            if (nombreNuevoJugadorEnTurno.Equals(SingletonJugador.Instance.Jugador.NombreUsuario))
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("¡Es tu turno de tirar!", "Tu turno", 2);
                ventanaTurno.Show();

                btnDados.IsEnabled = true;
            }
            else
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("Es turno de " + nombreNuevoJugadorEnTurno, "Hora de jugar", 2);
                ventanaTurno.Show();

                btnDados.IsEnabled = false;
            }
        }
    }
}
