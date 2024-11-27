using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using LaOcaClient.UserControls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Linq;

namespace LaOcaClient
{
    public partial class Partida : Window, IServicioPartidaCallback
    {
        private LaOcaService.Sala _sala;
        private LaOcaService.ServicioPartidaClient _clientePartida;
        private IServicioJugabilidad _servicioJugabilidad;
        private Dictionary<int, Point> _posicionesCasillas;
        private Grid[] _gridsJugadores;
        private Dictionary<string, int> _posicionesJugadores;
        private Dictionary<string, string> _fichaPorJugador = new Dictionary<string, string>();
        private Dictionary<string, Image> _fichasPorJugador = new Dictionary<string, Image>();
        private Dictionary<string, int> _casillasRecorridasPorJugador = new Dictionary<string, int>();
    

        private List<int> _casillasDeOca = new List<int> { 1, 5, 9, 14, 18, 23, 27, 32, 36, 41, 45, 50, 54, 59 };
        private List<int> _casillasPuente = new List<int> { 6, 12 };
        private List<int> _casillasPosada = new List<int> { 19 };
        private List<int> _casillasDado = new List<int> { 26 };
        private List<int> _casillasPozo = new List<int> { 31 };
        private List<int> _casillasLaberinto = new List<int> { 42 };
        private List<int> _casillasCarcel = new List<int> { 52 };
        private List<int> _casillasCalavera = new List<int> { 58 };
        private List<int> _casillasMeta = new List<int> { 63 };

        Ficha ficha = new Ficha();
        private int _pocisionAnterior;

        private List<string> fichasDisponibles = new List<string>
        {
            "pack://application:,,,/LaOcaClient;component/Recursos/FichaOcaAmarilla.png",
            "pack://application:,,,/LaOcaClient;component/Recursos/FichaOcaAzul.png",
            "pack://application:,,,/LaOcaClient;component/Recursos/FichaOcaRosa.png",
            "pack://application:,,,/LaOcaClient;component/Recursos/FichaOcaVerde.png"
        };

        public Partida(LaOcaService.Sala sala)
        {
            InitializeComponent();

            _servicioJugabilidad = new ServicioJugabilidadClient();
            _posicionesCasillas = ObtenerPosicionesCasillas();
            _posicionesJugadores = new Dictionary<string, int>();
            this._sala = sala;

            _gridsJugadores = new Grid[4];
            _gridsJugadores[0] = gridJugador1;
            _gridsJugadores[1] = gridJugador2;
            _gridsJugadores[2] = gridJugador3;
            _gridsJugadores[3] = gridJugador4;

            AgregarCanalCallbackDePartida();
            MostrarJugadoresEnPartida();
            MostrarJugadorEnTurno();
        }

        private void AgregarCanalCallbackDePartida()
        {
            InstanceContext contexto = new InstanceContext(this);
            _clientePartida = new LaOcaService.ServicioPartidaClient(contexto);
            _clientePartida.AgregarCanalCallbackPartida(SingletonJugador.Instance.Jugador.NombreUsuario, _sala.Codigo);
        }

        private void MostrarJugadoresEnPartida()
        {
            Point posicionInicial = _posicionesCasillas[0];

            for (int i = 0; i < _sala.Jugadores.Count; i++)
            {
                Jugador jugador = _sala.Jugadores[_sala.Partida.NombresDeJugadoresEnOrdenDeTurnos[i]];

                string fichaPath = fichasDisponibles[i % fichasDisponibles.Count];
                Image ficha = new Image();
                ficha.Source = new BitmapImage(new Uri(fichaPath));
                ficha.Width = 85;
                ficha.Height = 85;
                _fichasPorJugador[jugador.NombreUsuario] = ficha;

                _casillasRecorridasPorJugador[jugador.NombreUsuario] = 0; // Inicializar el contador

                JugadorEnSala jugadorEnSala = new JugadorEnSala(jugador, _sala.Codigo);
                if (SingletonJugador.Instance.Jugador.NombreUsuario.Equals(_sala.NombreHost))
                {
                    jugadorEnSala.CargarOpcionExpulsar();
                }
                _gridsJugadores[i].Children.Add(jugadorEnSala);
                _posicionesJugadores[jugador.NombreUsuario] = 0;

                TableroCanvas.Children.Add(ficha);
                Canvas.SetLeft(ficha, posicionInicial.X + (i));
                Canvas.SetTop(ficha, posicionInicial.Y);

                Console.WriteLine($"Jugador {jugador.NombreUsuario} con ficha {fichaPath} añadido a la interfaz en la posición inicial.");
            }
        }

        private void MostrarJugadorEnTurno()
        {
            string nombreJugadorEnTurno = _sala.Partida.NombreJugadorEnTurno;
            lbNombreJugadorEnTurno.Content = nombreJugadorEnTurno;

            if (nombreJugadorEnTurno.Equals(SingletonJugador.Instance.Jugador.NombreUsuario))
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("¡Es tu turno de tirar!", "Hora de jugar", 3);
                ventanaTurno.Show();
                BtnDados.IsEnabled = true;
            }
            else
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("Es turno de " + nombreJugadorEnTurno, "Hora de jugar", 3);
                ventanaTurno.Show();
                BtnDados.IsEnabled = false;
            }
        }

        private void LanzarDado(object sender, RoutedEventArgs e)
        {
            string nombreJugador = SingletonJugador.Instance.Jugador.NombreUsuario;

            if (_sala.Jugadores.TryGetValue(nombreJugador, out Jugador jugador))
            {
                if (jugador.TurnosPerdidos > 0)
                {
                    MessageBox.Show($"Pierdes un turno. Turnos restantes: {jugador.TurnosPerdidos}");
                    jugador.TurnosPerdidos--;
                    pasarTurnoSiguienteJugador();
                    return;
                }
            }

            //Random random = new Random();
            //int numeroAleatorio = random.Next(1, 7);
            int numeroAleatorio = 59;
            MessageBox.Show($"¡Has lanzado el dado! Salió el número {numeroAleatorio}.");
            MoverFicha(numeroAleatorio, nombreJugador);
        }

        private async void pasarTurnoSiguienteJugador()
        {
            string nombreJugador = SingletonJugador.Instance.Jugador.NombreUsuario;
            try
            {
                if (_sala.Partida.NombreJugadorEnTurno == nombreJugador)
                {
                    int posicionJugador = Array.IndexOf(_sala.Partida.NombresDeJugadoresEnOrdenDeTurnos, nombreJugador);
                    await _clientePartida.PasarTurnoASiguienteJugadorAsync(posicionJugador, _sala.Codigo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar de turno: {ex.Message}");
            }
        }

        private async void MoverFicha(int pasos, string nombreJugador)
        {
            BtnDados.IsEnabled = false;

            if (_posicionesJugadores.TryGetValue(nombreJugador, out int posicionActual))
            {
                int nuevaPosicion = posicionActual + pasos;
                _pocisionAnterior = nuevaPosicion;

                if (nuevaPosicion > 63)
                {
                    MessageBox.Show("Necesitas el número exacto para ganar.");
                    pasarTurnoSiguienteJugador();
                    return;
                }

                if (nuevaPosicion == 63)
                {
                    _posicionesJugadores[nombreJugador] = 63;
                    ActualizarInterfazGrafica(63, nombreJugador);
                    await _clientePartida.NotificarMovimientoFichaAsync(63, nombreJugador, _sala.Codigo);
                    return;
                }

                List<int> trayecto = GenerarTrayectoria(posicionActual, nuevaPosicion);

                for (int i = 0; i < trayecto.Count; i++)
                {
                    int posicion = trayecto[i];
                    _posicionesJugadores[nombreJugador] = posicion;

                    if (i > 0)
                    {
                        _casillasRecorridasPorJugador[nombreJugador]++;
                    }

                    await _clientePartida.NotificarMovimientoFichaAsync(posicion, nombreJugador, _sala.Codigo);

                    if (i == trayecto.Count - 1)
                    {
                        EvaluarCasilla(posicion, nombreJugador);
                    }

                    await Task.Delay(300); // Pausa para animación
                }
                _servicioJugabilidad.JugarTurno(pasos, _sala.Codigo, nombreJugador);
            }

            // Reactivar el botón solo cuando el movimiento y las evaluaciones hayan terminado
        }


        public void MovimientoFicha(int posicion, string nombreJugador)
        {
            Dispatcher.Invoke(() =>
            {
                ActualizarInterfazGrafica(posicion, nombreJugador);
            });
        }

        private async void EvaluarCasilla(int posicion, string nombreJugador)
        {
            Jugador jugador = _sala.Jugadores[nombreJugador];

            if (_casillasDeOca.Contains(posicion))
            {
                if (posicion == 59 && ((_pocisionAnterior - posicion) == 54))
                {
                    pasarTurnoSiguienteJugador();
                    return;
                }
                else if (posicion == 59)
                {
                    MessageBox.Show("¡Has caído en la oca dorada y te lleva directo a la meta!");

                    var trayecto = GenerarTrayectoria(58, 63);

                    foreach (var pos in trayecto)
                    {
                        _posicionesJugadores[nombreJugador] = pos;
                        ActualizarInterfazGrafica(pos, nombreJugador);

                        await _clientePartida.NotificarMovimientoFichaAsync(pos, nombreJugador, _sala.Codigo);

                        await Task.Delay(300);
                    }

                    _posicionesJugadores[nombreJugador] = 63;
                    ActualizarInterfazGrafica(63, nombreJugador);
                    MostrarPantallaVictoria();
                    return;
                }

                else
                {
                    MessageBox.Show("¡De oca a oca y tiro porque me toca!");
                    BtnDados.IsEnabled = true;
                    return;
                }
            }

            if (_casillasPuente.Contains(posicion))
            {
                MessageBox.Show("¡Has caído en el puente! Avanzas automáticamente y vuelves a tirar.");
                BtnDados.IsEnabled = true;
                return;
            }

            if (_casillasPosada.Contains(posicion))
            {
                MessageBox.Show("¡Caíste en la posada! Pierdes un turno.");
                jugador.TurnosPerdidos = 1;
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasDado.Contains(posicion))
            {
                MessageBox.Show("¡Caíste en la casilla de dados! Tira de nuevo.");
                BtnDados.IsEnabled = true;
                return;
            }

            if (_casillasPozo.Contains(posicion))
            {
                MessageBox.Show("¡Caíste en el pozo! Pierdes tres turnos.");
                jugador.TurnosPerdidos = 3;
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasLaberinto.Contains(posicion))
            {
                MessageBox.Show("¡Entraste al laberinto! Retrocedes a la casilla 30.");
                _posicionesJugadores[nombreJugador] = 30;
                ActualizarInterfazGrafica(30, nombreJugador);
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasCarcel.Contains(posicion))
            {
                MessageBox.Show("¡Estás en la cárcel! Pierdes dos turnos.");
                jugador.TurnosPerdidos = 2;
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasCalavera.Contains(posicion))
            {
                MessageBox.Show("¡Caíste en la calavera! Regresas al principio (casilla 1).");
                _posicionesJugadores[nombreJugador] = 1;
                ActualizarInterfazGrafica(1, nombreJugador);
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasMeta.Contains(posicion))
            {
                _posicionesJugadores[nombreJugador] = 63;
                ActualizarInterfazGrafica(63, nombreJugador);
                MostrarPantallaVictoria();
                return;
            }

            pasarTurnoSiguienteJugador();
        }

        private void MostrarPantallaVictoria()
        {
            var jugadoresOrdenados = _casillasRecorridasPorJugador
                .OrderByDescending(j => j.Value)
                .ToList();

            Victoria ventanaVictoria = new Victoria(jugadoresOrdenados);
            ventanaVictoria.ShowDialog();
        }


        private List<Jugador> ObtenerJugadoresOrdenadosPorPosicion()
        {
            // Crear una lista de jugadores a partir del diccionario de jugadores en la sala
            List<Jugador> jugadores = new List<Jugador>(_sala.Jugadores.Values);

            // Ordenar la lista de jugadores por la posición en el tablero
            jugadores.Sort((jugador1, jugador2) =>
                _posicionesJugadores[jugador2.NombreUsuario].CompareTo(_posicionesJugadores[jugador1.NombreUsuario]));

            return jugadores;
        }


        private List<int> GenerarTrayectoria(int posicionActual, int nuevaPosicion)
        {
            var trayecto = new List<int>();

            for (int i = posicionActual + 1; i <= nuevaPosicion && i <= 63; i++)
            {
                trayecto.Add(i);
            }

            return trayecto;
        }


        private void ActualizarInterfazGrafica(int nuevaPosicion, string nombreJugador)
        {
            if (_fichasPorJugador.TryGetValue(nombreJugador, out Image ficha))
            {
                if (_posicionesCasillas.TryGetValue(nuevaPosicion, out Point nuevaPosicionCanvas))
                {
                    Dispatcher.Invoke(() =>
                    {
                        Canvas.SetLeft(ficha, nuevaPosicionCanvas.X);
                        Canvas.SetTop(ficha, nuevaPosicionCanvas.Y);
                    });
                    Console.WriteLine($"La ficha de {nombreJugador} está ahora en la casilla {nuevaPosicion}");
                }
                else
                {
                    Console.WriteLine($"Posición {nuevaPosicion} no encontrada en el tablero.");
                }
            }
            else
            {
                Console.WriteLine($"Ficha para el jugador {nombreJugador} no encontrada.");
            }
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
                { 62, new Point(22, -42) },
                { 63, new Point(141, -42) },
            };
            return posiciones;
        }

        public void ActualizarPosicionFicha(int nuevaPosicion, string nombreJugador)
        {
            if (_posicionesJugadores.ContainsKey(nombreJugador))
            {
                _posicionesJugadores[nombreJugador] = nuevaPosicion;
                ActualizarInterfazGrafica(nuevaPosicion, nombreJugador);
            }
            else
            {
                Console.WriteLine($"Jugador {nombreJugador} no encontrado en el diccionario de posiciones.");
            }
        }

        public void MostrarNuevoJugadorEnTurno(string nombreNuevoJugadorEnTurno)
        {
            _sala.Partida.NombreJugadorEnTurno = nombreNuevoJugadorEnTurno;
            lbNombreJugadorEnTurno.Content = nombreNuevoJugadorEnTurno;

            if (nombreNuevoJugadorEnTurno.Equals(SingletonJugador.Instance.Jugador.NombreUsuario))
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("¡Es tu turno de tirar!", "Tu turno", 2);
                ventanaTurno.Show();
                BtnDados.IsEnabled = true;
            }
            else
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico("Es turno de " + nombreNuevoJugadorEnTurno, "Hora de jugar", 2);
                ventanaTurno.Show();
                BtnDados.IsEnabled = false;
            }
        }
    }
}