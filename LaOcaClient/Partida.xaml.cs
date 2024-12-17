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
    public partial class Partida : Window, IVentanaSala, IServicioPartidaCallback
    {
        public LaOcaService.Sala SalaActual { get; set; }
        public ServicioActualizacionJugadoresEnSalaClient ClienteJugadoresEnSala { get; set; }

        private IniciarSesion _iniciarSesion = new IniciarSesion();
        private LaOcaService.ServicioPartidaClient _clientePartida;
        private IServicioJugabilidad _servicioJugabilidad;
        private Dictionary<int, Point> _posicionesCasillas;
        private Grid[] _gridsJugadores;
        private Dictionary<string, int> _posicionesJugadores;
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

        private int _pocisionAnterior;

        private System.Windows.Threading.DispatcherTimer _timerPing;
        private bool CierreVoluntario = false;

        private List<string> _fichasDisponibles = new List<string>
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
            this.SalaActual = sala;

            _gridsJugadores = new Grid[4];
            _gridsJugadores[0] = gridJugador1;
            _gridsJugadores[1] = gridJugador2;
            _gridsJugadores[2] = gridJugador3;
            _gridsJugadores[3] = gridJugador4;

            AgregarCanalCallbackDePartida();
            MostrarJugadoresEnPartida();
            MostrarJugadorEnTurno();
            MostrarFichasYJugadores();

            _timerPing = new System.Windows.Threading.DispatcherTimer();
            _timerPing.Interval = TimeSpan.FromSeconds(5);
            _timerPing.Tick += ComprobarServidor;
            _timerPing.Start();
        }

        private async void ComprobarServidor(object sender, EventArgs e)
        {
            try
            {
                bool servidorActivo = await Task.Run(() => _clientePartida.Ping());
                if (!servidorActivo)
                {
                    throw new CommunicationException("Servidor no responde.");
                }
            }
            catch (CommunicationException)
            {
                NotificarServidorCaido();
            }
            catch (TimeoutException)
            {
                NotificarServidorCaido();
            }
            catch (Exception)
            {
                NotificarServidorCaido();
            }
        }

        private void NotificarServidorCaido()
        {
            _timerPing.Stop();
            MessageBox.Show(Properties.Resources.msgComunnicationEx,
                            Properties.Resources.globalTituloError,
                            MessageBoxButton.OK, MessageBoxImage.Error);

            foreach (var jugador in SalaActual.Jugadores.Values)
            {
                try
                {
                    var callback = jugador.CanalCallbackPartida as IServicioPartidaCallback;
                    if (callback != null)
                    {
                        callback.NotificarAbandonoJugador(SingletonJugador.Instance.Jugador.NombreUsuario);
                    }
                }
                catch (FaultException)
                {
                    MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (CommunicationException)
                {
                    MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                    _iniciarSesion.Show();
                    this.Close();
                }
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            _iniciarSesion.Show();
            this.Close();
        }

        private void AgregarCanalCallbackDePartida()
        {
            try
            {
                InstanceContext contexto = new InstanceContext(this);
                _clientePartida = new LaOcaService.ServicioPartidaClient(contexto);
                _clientePartida.AgregarCanalCallbackPartida(SingletonJugador.Instance.Jugador.NombreUsuario, SalaActual.Codigo);
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarFichasYJugadores()
        {
            FichasJugadoresPanel.Children.Clear();

            FichasJugadoresPanel.Children.Add(new TextBlock
            {
                Text = Properties.Resources.lbJugadoresEnPartida,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var jugadoresConFichas = SalaActual.Jugadores.Values.Select(jugador => new
            {
                NombreUsuario = jugador.NombreUsuario,
                Ficha = _fichasPorJugador.TryGetValue(jugador.NombreUsuario, out Image ficha) ? ficha : null
            });

            foreach (var jugador in jugadoresConFichas)
            {
                StackPanel panelJugador = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                if (jugador.Ficha != null)
                {
                    Image imagenFicha = new Image
                    {
                        Source = jugador.Ficha.Source,
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    panelJugador.Children.Add(imagenFicha);
                }

                TextBlock nombreJugador = new TextBlock
                {
                    Text = jugador.NombreUsuario,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                panelJugador.Children.Add(nombreJugador);

                FichasJugadoresPanel.Children.Add(panelJugador);
            }
        }

        private void MostrarJugadoresEnPartida()
        {
            Point posicionInicial = _posicionesCasillas[0];

            for (int i = 0; i < SalaActual.Jugadores.Count; i++)
            {
                Jugador jugador = SalaActual.Jugadores[SalaActual.Partida.NombresDeJugadoresEnOrdenDeTurnos[i]];

                string fichaPath = _fichasDisponibles[i % _fichasDisponibles.Count];
                Image fichaObtenida = new Image
                {
                    Source = new BitmapImage(new Uri(fichaPath)),
                    Width = 85,
                    Height = 85
                };

                if (SingletonJugador.Instance.Jugador.NombreUsuario == jugador.NombreUsuario)
                {
                    fichaObtenida.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = Colors.Magenta,
                        Direction = 0,
                        ShadowDepth = 0,
                        Opacity = 1,
                        BlurRadius = 30
                    };
                }

                _fichasPorJugador[jugador.NombreUsuario] = fichaObtenida;
                _casillasRecorridasPorJugador[jugador.NombreUsuario] = 0;

                JugadorEnSala jugadorEnSala = new JugadorEnSala(jugador);
                if (SingletonJugador.Instance.Jugador.NombreUsuario.Equals(SalaActual.NombreHost))
                {
                    jugadorEnSala.CargarOpcionExpulsar();
                }

                _gridsJugadores[i].Children.Add(jugadorEnSala);
                _posicionesJugadores[jugador.NombreUsuario] = 0;

                cvTablero.Children.Add(fichaObtenida);
                Canvas.SetLeft(fichaObtenida, posicionInicial.X + (i));
                Canvas.SetTop(fichaObtenida, posicionInicial.Y);
            }
        }

        private void MostrarJugadorEnTurno()
        {
            string nombreJugadorEnTurno = SalaActual.Partida.NombreJugadorEnTurno;
            lbNombreJugadorEnTurno.Content = nombreJugadorEnTurno;

            if (nombreJugadorEnTurno.Equals(SingletonJugador.Instance.Jugador.NombreUsuario))
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico(Properties.Resources.msgEsTuTurno, Properties.Resources.tituloHoraDeJugar, 3);
                ventanaTurno.Show();
                btnDado.IsEnabled = true;
                btnAbandonar.IsEnabled = true;
            }
            else
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico(Properties.Resources.lbEsTurnoDe + nombreJugadorEnTurno, Properties.Resources.tituloHoraDeJugar, 3);
                ventanaTurno.Show();
                btnDado.IsEnabled = false;
                btnAbandonar.IsEnabled = false;
            }
        }

        private async void LanzarDado(object sender, RoutedEventArgs e)
        {
            string nombreJugador = SingletonJugador.Instance.Jugador.NombreUsuario;

            try
            {
                if (SalaActual.Jugadores.TryGetValue(nombreJugador, out Jugador jugador) && (jugador.TurnosPerdidos > 0))
                {
                    MessageBox.Show(Properties.Resources.msgPierdesUnTurno + $"{jugador.TurnosPerdidos}");
                    jugador.TurnosPerdidos--;
                    await PasarTurnoSiguienteJugador();
                    return;
                }

                Random random = new Random();
                int numeroAleatorio = random.Next(1, 7);
                MessageBox.Show(Properties.Resources.msgLanzarDado + $"{numeroAleatorio}.");
                await MoverFicha(numeroAleatorio, nombreJugador);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PasarTurnoSiguienteJugador()
        {
            try
            {
                var jugadoresEnOrden = SalaActual.Partida.NombresDeJugadoresEnOrdenDeTurnos.ToList();

                if (jugadoresEnOrden.Count > 1)
                {
                    int posicionJugador = jugadoresEnOrden.IndexOf(SingletonJugador.Instance.Jugador.NombreUsuario);

                    if (posicionJugador >= 0)
                    {
                        await _clientePartida.PasarTurnoASiguienteJugadorAsync(posicionJugador, SalaActual.Codigo);
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.msgJugadorNoEstaEnListaTurnos);
                    }
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgPartidaTerminadaSoloUnJugador);
                }
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task MoverFicha(int pasos, string nombreJugador)
        {
            btnDado.IsEnabled = false;
            btnAbandonar.IsEnabled = false;

            try
            {
                if (_posicionesJugadores.TryGetValue(nombreJugador, out int posicionActual))
                {
                    int nuevaPosicion = posicionActual + pasos;
                    _pocisionAnterior = nuevaPosicion;

                    if (nuevaPosicion > 63)
                    {
                        MessageBox.Show(Properties.Resources.msgNecesitasNumeroExacto);
                        await PasarTurnoSiguienteJugador();
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

                        await _clientePartida.NotificarMovimientoFichaAsync(posicion, nombreJugador, SalaActual.Codigo);

                        if (i == trayecto.Count - 1)
                        {
                            await EvaluarCasilla(posicion, nombreJugador);
                        }

                        await Task.Delay(300);
                    }
                    _servicioJugabilidad.JugarTurno(pasos, SalaActual.Codigo, nombreJugador);
                }
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MovimientoFicha(int posicion, string nombreJugador)
        {
            Dispatcher.Invoke(() =>
            {
                ActualizarInterfazGrafica(posicion, nombreJugador);
            });
        }

        private async Task EvaluarCasilla(int posicion, string nombreJugador)
        {
            Jugador jugador = SalaActual.Jugadores[nombreJugador];

            try
            {
                if (_casillasDeOca.Contains(posicion))
                {
                    await ManejarCasillaOca(posicion, nombreJugador);
                    return;
                }

                if (_casillasPuente.Contains(posicion))
                {
                    MostrarMensajeYHabilitarBotones(Properties.Resources.msgPuente);
                    return;
                }

                if (_casillasPosada.Contains(posicion))
                {
                    await ManejarCasillaConTurnosPerdidos(jugador, 1, Properties.Resources.msgPosada);
                    return;
                }

                if (_casillasDado.Contains(posicion))
                {
                    MostrarMensajeYHabilitarBotones(Properties.Resources.msgDados);
                    return;
                }

                if (_casillasPozo.Contains(posicion))
                {
                    await ManejarCasillaConTurnosPerdidos(jugador, 3, Properties.Resources.msgPozo);
                    return;
                }

                if (_casillasLaberinto.Contains(posicion))
                {
                    await ManejarCasillaLaberinto(nombreJugador);
                    return;
                }

                if (_casillasCarcel.Contains(posicion))
                {
                    await ManejarCasillaConTurnosPerdidos(jugador, 2, Properties.Resources.msgCarcel);
                    return;
                }

                if (_casillasCalavera.Contains(posicion))
                {
                    await ManejarCasillaCalavera(nombreJugador);
                    return;
                }

                if (_casillasMeta.Contains(posicion))
                {
                    await NotificarMovimiento(posicion, nombreJugador);
                    return;
                }
                await PasarTurnoSiguienteJugador();
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ManejarCasillaOca(int posicion, string nombreJugador)
        {
            try
            {
                if (posicion == 59 && ((_pocisionAnterior - posicion) == 54))
                {
                    await PasarTurnoSiguienteJugador();
                }
                else if (posicion == 59)
                {
                    MessageBox.Show(Properties.Resources.msgOcaDorada);
                    var trayecto = GenerarTrayectoria(59, 63);
                    foreach (var pos in trayecto)
                    {
                        _posicionesJugadores[nombreJugador] = pos;
                        ActualizarInterfazGrafica(pos, nombreJugador);
                        await NotificarMovimiento(pos, nombreJugador);
                        await Task.Delay(300);
                    }
                    await NotificarMovimiento(posicion, nombreJugador);
                }
                else
                {
                    MostrarMensajeYHabilitarBotones(Properties.Resources.msgDeOcaAOca);
                }
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarMensajeYHabilitarBotones(string mensaje)
        {
            MessageBox.Show(mensaje);
            btnDado.IsEnabled = true;
            btnAbandonar.IsEnabled = true;
        }

        private async Task ManejarCasillaConTurnosPerdidos(Jugador jugador, int turnosPerdidos, string mensaje)
        {
            try
            {
                MessageBox.Show(mensaje);
                jugador.TurnosPerdidos = turnosPerdidos;
                await PasarTurnoSiguienteJugador();
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ManejarCasillaLaberinto(string nombreJugador)
        {
            try
            {
                MessageBox.Show(Properties.Resources.msgLaberinto);
                _posicionesJugadores[nombreJugador] = 30;
                ActualizarInterfazGrafica(30, nombreJugador);
                await PasarTurnoSiguienteJugador();
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ManejarCasillaCalavera(string nombreJugador)
        {
            try
            {
                MessageBox.Show(Properties.Resources.msgCalavera);
                _posicionesJugadores[nombreJugador] = 1;
                ActualizarInterfazGrafica(1, nombreJugador);
                await PasarTurnoSiguienteJugador();
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task NotificarMovimiento(int posicion, string nombreJugador)
        {
            try
            {
                await _clientePartida.NotificarMovimientoFichaAsync(posicion, nombreJugador, SalaActual.Codigo);
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MostrarPantallaVictoria(KeyValuePair<string, int>[] jugadoresOrdenados)
        {
            _timerPing.Stop();
            Dispatcher.Invoke(() =>
            {
                Victoria ventanaVictoria = new Victoria(jugadoresOrdenados.ToList());
                ventanaVictoria.Show();
                this.Close();
            });
        }

        private static List<int> GenerarTrayectoria(int posicionActual, int nuevaPosicion)
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
            if (_fichasPorJugador.TryGetValue(nombreJugador, out Image fichaObtenida) && (_posicionesCasillas.TryGetValue(nuevaPosicion, out Point nuevaPosicionCanvas)))
            {
                Dispatcher.Invoke(() =>
                {
                    Canvas.SetLeft(fichaObtenida, nuevaPosicionCanvas.X);
                    Canvas.SetTop(fichaObtenida, nuevaPosicionCanvas.Y);
                }); 
            }
        }

        private static Dictionary<int, Point> ObtenerPosicionesCasillas()
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
        }

        public void MostrarNuevoJugadorEnTurno(string nombreNuevoJugadorEnTurno)
        {
            SalaActual.Partida.NombreJugadorEnTurno = nombreNuevoJugadorEnTurno;
            lbNombreJugadorEnTurno.Content = nombreNuevoJugadorEnTurno;

            if (nombreNuevoJugadorEnTurno.Equals(SingletonJugador.Instance.Jugador.NombreUsuario))
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico(Properties.Resources.msgEsTuTurno, Properties.Resources.tituloHoraDeJugar, 2);
                ventanaTurno.Show();
                btnDado.IsEnabled = true;
                btnAbandonar.IsEnabled = true;
            }
            else
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico(Properties.Resources.lbEsTurnoDe + nombreNuevoJugadorEnTurno, Properties.Resources.tituloHoraDeJugar, 2);
                ventanaTurno.Show();
                btnDado.IsEnabled = false;
                btnAbandonar.IsEnabled = false;
            }
        }

        private async void BtnAbandonar(object sender, RoutedEventArgs e)
        {
            string nombreJugador = SingletonJugador.Instance.Jugador.NombreUsuario;
            MessageBoxResult resultado = MessageBox.Show(Properties.Resources.msgAbandonarPartidaEnCurso, Properties.Resources.tituloConfirmacion, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    if (SalaActual.Jugadores.ContainsKey(nombreJugador))
                    {
                        await _clientePartida.AbandonarPartidaAsync(nombreJugador, SalaActual.Codigo);

                        if (_fichasPorJugador.TryGetValue(nombreJugador, out Image fichaObtenida))
                        {
                            cvTablero.Children.Remove(fichaObtenida);
                            _fichasPorJugador.Remove(nombreJugador);
                        }

                        await Dispatcher.InvokeAsync(() =>
                        {
                            lbNotificacion.Content = $"{nombreJugador}" + Properties.Resources.msgHaAbandonadoLaPartida;
                            lbNotificacion.Visibility = Visibility.Visible;
                        });

                        MessageBox.Show(Properties.Resources.msgPartidaAbandonada);
                        MenuPrincipal menu = new MenuPrincipal();
                        menu.Show();
                        this.Close();
                    }
                }
                catch (FaultException)
                {
                    MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (CommunicationException)
                {
                    MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                    _iniciarSesion.Show();
                    this.Close();
                }
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void NotificarAbandonoJugador(string nombreJugador)
        {
            Dispatcher.Invoke(() =>
            {
                MostrarNotificacionAbandono(nombreJugador);
                EliminarFichaJugador(nombreJugador);
                ResaltarNombreJugadorEnPanel(nombreJugador);
                EliminarJugadorDeGrids(nombreJugador);
            });
        }

        private void MostrarNotificacionAbandono(string nombreJugador)
        {
            lbNotificacion.Content = $"{nombreJugador} {Properties.Resources.msgHaAbandonadoLaPartida}";
            lbNotificacion.Visibility = Visibility.Visible;
        }

        private void EliminarFichaJugador(string nombreJugador)
        {
            if (_fichasPorJugador.TryGetValue(nombreJugador, out Image fichaObtenida))
            {
                cvTablero.Children.Remove(fichaObtenida);
                _fichasPorJugador.Remove(nombreJugador);
            }
        }

        private void ResaltarNombreJugadorEnPanel(string nombreJugador)
        {
            foreach (var panelJugador in FichasJugadoresPanel.Children.OfType<StackPanel>())
            {
                var nombreTextBlock = panelJugador.Children.OfType<TextBlock>().FirstOrDefault();
                if (nombreTextBlock != null && nombreTextBlock.Text == nombreJugador)
                {
                    nombreTextBlock.Foreground = Brushes.Red;
                    break;
                }
            }
        }

        private void EliminarJugadorDeGrids(string nombreJugador)
        {
            var jugadorEnSala = _gridsJugadores
                .SelectMany(grid => grid.Children.OfType<JugadorEnSala>())
                .FirstOrDefault(child => child.lbNombreJugador.Content.ToString() == nombreJugador);

            if (jugadorEnSala != null)
            {
                var grid = _gridsJugadores.First(g => g.Children.Contains(jugadorEnSala));
                grid.Children.Remove(jugadorEnSala);
            }
        }

        public void MostrarMensajeError(string mensaje)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(mensaje, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                });
            });
        }

        public void MostrarOpcionesErrorBD(string nombreJugadorGanador)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (nombreJugadorGanador == SingletonJugador.Instance.Jugador.NombreUsuario)
                    {
                        MessageBoxResult resultado = MessageBox.Show(
                            Properties.Resources.msgErrorGuardarEstadisticas,
                            Properties.Resources.tituloExcepcionGeneral,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (resultado == MessageBoxResult.Yes)
                        {
                            Task.Run(() =>
                            {
                                ReintentarGuardarEstadisticas();
                            });
                        }
                        else
                        {
                            Task.Run(() =>
                            {
                                FinalizarSinGuardarEstadisticas();
                            });
                        }
                    }
                });
            });
        }

        private async void ReintentarGuardarEstadisticas()
        {
            try
            {
                await _clientePartida.ReintentarGuardarEstadisticasAsync(SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario);
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void FinalizarSinGuardarEstadisticas()
        {
            try
            {
                await _clientePartida.FinalizarSinGuardarEstadisticasAsync(SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario);
            }
            catch (FaultException)
            {
                MessageBox.Show(Properties.Resources.globalErrorBD, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                _iniciarSesion.Show();
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {

            base.OnClosing(e);
            _timerPing.Stop();
        }

        public void MostrarMensajeExito(string mensaje)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(mensaje, Properties.Resources.globalTituloExito, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }
}