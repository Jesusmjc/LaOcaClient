using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using LaOcaClient.UserControls;

namespace LaOcaClient
{
    public partial class Partida : Window, IServicioPartidaCallback
    {
        private LaOcaService.Sala sala;
        private LaOcaService.ServicioPartidaClient clientePartida;
        private IServicioJugabilidad _servicioJugabilidad;
        private Dictionary<int, Point> _posicionesCasillas;
        private Grid[] gridsJugadores;
        private Dictionary<string, int> posicionesJugadores;
        private string _fichaAsignada;
        private Dictionary<string, string> fichaPorJugador = new Dictionary<string, string>();


        public Partida(LaOcaService.Sala sala)
        {
            InitializeComponent();

            _servicioJugabilidad = new ServicioJugabilidadClient();
            _posicionesCasillas = ObtenerPosicionesCasillas();
            posicionesJugadores = new Dictionary<string, int>();

            this.sala = sala;
            gridsJugadores = new Grid[4];
            gridsJugadores[0] = gridJugador1;
            gridsJugadores[1] = gridJugador2;
            gridsJugadores[2] = gridJugador3;
            gridsJugadores[3] = gridJugador4;

            AgregarCanalCallbackDePartida();
            MostrarJugadoresEnPartida();
            MostrarJugadorEnTurno();
        }

        private void AgregarCanalCallbackDePartida()
        {
            InstanceContext contexto = new InstanceContext(this);
            clientePartida = new LaOcaService.ServicioPartidaClient(contexto);
            clientePartida.AgregarCanalCallback(SingletonJugador.Instance.Jugador.NombreUsuario, sala.Codigo);
        }

        private void MostrarJugadoresEnPartida()
        {
            for (int i = 0; i < sala.Jugadores.Count; i++)
            {
                Jugador jugador = sala.Jugadores[sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[i]];

                fichaPorJugador[jugador.NombreUsuario] = jugador.FichaAsignada;

                JugadorEnSala jugadorEnSala = new JugadorEnSala(jugador.NombreUsuario, jugador.IdJugador);
                gridsJugadores[i].Children.Add(jugadorEnSala);
                posicionesJugadores[jugador.NombreUsuario] = 0; 
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

        private async void LanzarDados(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int numeroAleatorio = random.Next(2, 7);
            MessageBox.Show("Ha salido " + numeroAleatorio);

            MoverFicha(numeroAleatorio, SingletonJugador.Instance.Jugador.NombreUsuario);

            if (sala.Partida.NombreJugadorEnTurno == SingletonJugador.Instance.Jugador.NombreUsuario)
            {
                int posicionJugador = Array.IndexOf(sala.Partida.NombresDeJugadoresEnOrdenDeTurnos, SingletonJugador.Instance.Jugador.NombreUsuario);
                try
                {
                    await clientePartida.PasarTurnoASiguienteJugadorAsync(posicionJugador, sala.Codigo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al pasar el turno: {ex.Message}");
                }
            }
        }

        private void MoverFicha(int pasos, string nombreJugador)
        {
            int posicionActual = posicionesJugadores[nombreJugador];
            int nuevaPosicion = posicionActual + pasos;

            if (nuevaPosicion >= _posicionesCasillas.Count)
                nuevaPosicion = _posicionesCasillas.Count - 1;

            posicionesJugadores[nombreJugador] = nuevaPosicion;

            ActualizarInterfazGrafica(nuevaPosicion, nombreJugador);

            _servicioJugabilidad.JugarTurno(pasos);
        }

        private void ActualizarInterfazGrafica(int nuevaPosicion, string nombreJugador)
        {
            Image ficha = ObtenerFichaPorJugador(nombreJugador);
            if (_posicionesCasillas.TryGetValue(nuevaPosicion, out Point nuevaPosicionCanvas))
            {
                Canvas.SetLeft(ficha, nuevaPosicionCanvas.X);
                Canvas.SetTop(ficha, nuevaPosicionCanvas.Y);
                Console.WriteLine($"La ficha de {nombreJugador} está ahora en la casilla {nuevaPosicion}");
            }
            else
            {
                Console.WriteLine($"Posición {nuevaPosicion} no encontrada en el tablero.");
            }
        }

        private Image ObtenerFichaPorJugador(string nombreJugador)
        {
            string fichaAsignada;
            if (fichaPorJugador.TryGetValue(nombreJugador, out fichaAsignada))
            {
                if (fichaAsignada == "FichaOcaAmarilla")
                    return FichaOcaAmarilla;
                else if (fichaAsignada == "FichaOcaAzul")
                    return FichaOcaAzul;
                else if (fichaAsignada == "FichaOcaRosa")
                    return FichaOcaRosa;
                else if (fichaAsignada == "FichaOcaVerde")
                    return FichaOcaVerde;
            }
            return FichaOcaAmarilla;
        }

        private Dictionary<int, Point> ObtenerPosicionesCasillas()
        {
            var posiciones = new Dictionary<int, Point>
            {
                { 0, new Point(-584, 315) },
                { 1, new Point(-454, 315) },
                { 2, new Point(-335, 315) },
                { 3, new Point(-216, 318) },
                { 4, new Point(-97, 315) },
                { 5, new Point(22, 315) },
                { 6, new Point(141, 315) },
                { 7, new Point(260, 315) },
                { 8, new Point(379, 315) },
                { 9, new Point(499, 315) },
                { 10, new Point(499, 196) },
                { 11, new Point(499, 77) },
                { 12, new Point(499, -42) },
                { 13, new Point(499, -161) },
                { 14, new Point(499, -281) },
                { 15, new Point(499, -400) },
                { 16, new Point(379, -400) },
                { 17, new Point(260, -400) },
                { 18, new Point(141, -400) },
                { 19, new Point(22, -400) },
                { 20, new Point(-97, -400) },
                { 21, new Point(-216, -400) },
                { 22, new Point(-335, -400) },
                { 23, new Point(-454, -400) },
                { 24, new Point(-454, -281) },
                { 25, new Point(-454, -161) },
                { 26, new Point(-454, -42) },
                { 27, new Point(-454, 77) },
                { 28, new Point(-454, 196) },
                { 29, new Point(-335, 196) },
                { 30, new Point(-216, 196) },
                { 31, new Point(-97, 196) },
                { 32, new Point(22, 196) },
                { 33, new Point(141, 196) },
                { 34, new Point(260, 196) },
                { 35, new Point(379, 196) },
                { 36, new Point(379, 77) },
                { 37, new Point(379, -42) },
                { 38, new Point(379, -161) },
                { 39, new Point(379, -281) },
                { 40, new Point(260, -281) },
                { 41, new Point(141, -281) },
                { 42, new Point(22, -281) },
                { 43, new Point(-97, -281) },
                { 44, new Point(-216, -281) },
                { 45, new Point(-335, -281) },
                { 46, new Point(-335, -161) },
                { 47, new Point(-335, -42) },
                { 48, new Point(-335, 77) },
                { 49, new Point(-216, 77) },
                { 50, new Point(-97, 77) },
                { 51, new Point(22, 77) },
                { 52, new Point(141, 77) },
                { 53, new Point(260, 77) },
                { 54, new Point(260, -42) },
                { 55, new Point(260, -161) },
                { 56, new Point(141, -161) },
                { 57, new Point(22, -161) },
                { 58, new Point(-97, -161) },
                { 59, new Point(-216, -161) },
                { 60, new Point(-216, -42) },
                { 61, new Point(-97, -42) },
                { 62, new Point(-22, -42) },
                { 63, new Point(141, -42) },
            };
            return posiciones;
        }

        public void ActualizarPosicionFicha(int nuevaPosicion, string nombreJugador)
        {
            posicionesJugadores[nombreJugador] = nuevaPosicion;

            ActualizarInterfazGrafica(nuevaPosicion, nombreJugador);
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