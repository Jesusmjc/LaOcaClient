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

        private List<int> _casillasDeOca = new List<int> { 1, 5, 9, 14, 18, 23, 27, 32, 36, 41, 45, 50, 54, 59 };
        private List<int> _casillasPuente = new List<int> { 6, 12 };
        private List<int> _casillasPosada = new List<int> { 19 };
        private List<int> _casillasDado = new List<int> { 26 };
        private List<int> _casillasPozo = new List<int> { 31 };
        private List<int> _casillasLaberinto = new List<int> { 42 };
        private List<int> _casillasCarcel = new List<int> { 52 };
        private List<int> _casillasCalavera = new List<int> { 58 };
        private List<int> _casillasMeta = new List<int> { 63 };

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
            _clientePartida.AgregarCanalCallback(SingletonJugador.Instance.Jugador.NombreUsuario, _sala.Codigo);
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

                JugadorEnSala jugadorEnSala = new JugadorEnSala(jugador.NombreUsuario, jugador.IdJugador);
                if (SingletonJugador.Instance.Jugador.NombreUsuario.Equals(sala.NombreHost))
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

            if (_posicionesJugadores.TryGetValue(nombreJugador, out int nuevaPosicion))
            {
                Jugador jugador = _sala.Jugadores[nombreJugador];

                if (jugador.TurnosPerdidos > 0)
                {
                    MessageBox.Show($"Pierdes un turno. Turnos restantes: {jugador.TurnosPerdidos}");
                    jugador.TurnosPerdidos--;
                    pasarTurnoSiguienteJugador();
                    return;
                }

                Random random = new Random();
                int numeroAleatorio = random.Next(2, 7);
                MessageBox.Show("Ha salido " + numeroAleatorio);
                MoverFicha(numeroAleatorio, nombreJugador);

                if (_posicionesJugadores.TryGetValue(nombreJugador, out nuevaPosicion))
                {
                    // Casillas tipo OCA
                    if (_casillasDeOca.Contains(nuevaPosicion))
                    {
                        if (nuevaPosicion == 59)
                        {
                            MessageBox.Show("¡Has caído en la oca dorada y te lleva directo a la meta!");
                            nuevaPosicion = 63;
                            _posicionesJugadores[nombreJugador] = nuevaPosicion;
                            ActualizarInterfazGrafica(nuevaPosicion, nombreJugador);
                            MessageBox.Show("¡Felicidades! ¡Has ganado!");
                            BtnDados.IsEnabled = false;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("¡De oca a oca y tiro porque me toca!");
                            BtnDados.IsEnabled = true;
                            return;
                        }
                    }

                    // Casillas tipo PUENTE
                    if (_casillasPuente.Contains(nuevaPosicion))
                    {
                        MessageBox.Show("¡Has caído en el puente! Avanzas automáticamente y vuelves a tirar.");
                        BtnDados.IsEnabled = true;
                        return;
                    }

                    // Casillas tipo POSADA
                    if (_casillasPosada.Contains(nuevaPosicion))
                    {
                        MessageBox.Show("¡Caíste en la posada! Pierdes un turno.");
                        jugador.TurnosPerdidos = 1;
                        pasarTurnoSiguienteJugador();
                        return;
                    }

                    // Casillas tipo DADO
                    if (_casillasDado.Contains(nuevaPosicion))
                    {
                        MessageBox.Show("¡Caíste en la casilla de dados! Tira de nuevo.");
                        BtnDados.IsEnabled = true;
                        return;
                    }

                    // Casillas tipo POZO
                    if (_casillasPozo.Contains(nuevaPosicion))
                    {
                        MessageBox.Show("¡Caíste en el pozo! Pierdes tres turnos.");
                        jugador.TurnosPerdidos = 3;
                        pasarTurnoSiguienteJugador();
                        return;
                    }

                    // Casillas tipo LABERINTO
                    if (_casillasLaberinto.Contains(nuevaPosicion))
                    {
                        MessageBox.Show("¡Entraste al laberinto! Retrocedes a la casilla 30.");
                        pasarTurnoSiguienteJugador();
                        return;
                    }

                    // Casillas tipo CARCEL
                    if (_casillasCarcel.Contains(nuevaPosicion))
                    {
                        MessageBox.Show("¡Estás en la cárcel! Pierdes dos turnos.");
                        jugador.TurnosPerdidos = 2;
                        pasarTurnoSiguienteJugador();
                        return;
                    }

                    // Casillas tipo CALAVERA
                    if (_casillasCalavera.Contains(nuevaPosicion))
                    {
                        MessageBox.Show("¡Caíste en la calavera! Regresas al principio (casilla 1).");
                        pasarTurnoSiguienteJugador();
                        return;
                    }

                    // Casillas tipo META
                    if (_casillasMeta.Contains(nuevaPosicion))
                    {
                        MessageBox.Show("¡Felicidades! ¡Has ganado!");
                        BtnDados.IsEnabled = false;
                        return;
                    }
                }
                pasarTurnoSiguienteJugador();
            }
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

        private void MoverFicha(int pasos, string nombreJugador)
        {
            if (_posicionesJugadores.TryGetValue(nombreJugador, out int posicionActual))
            {
                int nuevaPosicion = posicionActual + pasos;

                // Verificar si el movimiento excede la casilla final
                if (nuevaPosicion > 63)
                {
                    MessageBox.Show("Necesitas el número exacto para ganar.");
                    return;
                }

                // Verificar si el jugador está en la casilla 59 y no obtiene un 4 exacto
                if (posicionActual == 59 && pasos != 4)
                {
                    MessageBox.Show("Necesitas un 4 exacto para ganar desde la casilla 59.");
                    return;
                }

                if (nuevaPosicion < 0)
                {
                    nuevaPosicion = 1;
                }
                else if (nuevaPosicion >= _posicionesCasillas.Count)
                {
                    nuevaPosicion = _posicionesCasillas.Count - 1;
                }

                _posicionesJugadores[nombreJugador] = nuevaPosicion;
                ActualizarInterfazGrafica(nuevaPosicion, nombreJugador);
                _servicioJugabilidad.JugarTurno(pasos, _sala.Codigo, nombreJugador);
            }
        }

        private void ActualizarInterfazGrafica(int nuevaPosicion, string nombreJugador)
        {
            if (_fichasPorJugador.TryGetValue(nombreJugador, out Image ficha))
            {
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