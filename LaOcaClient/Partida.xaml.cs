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

            _ = VerificarConexionConServidor();


        }

        private void AgregarCanalCallbackDePartida()
        {
            InstanceContext contexto = new InstanceContext(this);
            _clientePartida = new LaOcaService.ServicioPartidaClient(contexto);
            _clientePartida.AgregarCanalCallbackPartida(SingletonJugador.Instance.Jugador.NombreUsuario, SalaActual.Codigo);

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

            foreach (var jugador in SalaActual.Jugadores.Values)
            {
                StackPanel panelJugador = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                if (_fichasPorJugador.TryGetValue(jugador.NombreUsuario, out Image ficha))
                {
                    Image imagenFicha = new Image
                    {
                        Source = ficha.Source,
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

                string fichaPath = fichasDisponibles[i % fichasDisponibles.Count];
                Image ficha = new Image
                {
                    Source = new BitmapImage(new Uri(fichaPath)),
                    Width = 85,
                    Height = 85
                };

                if (SingletonJugador.Instance.Jugador.NombreUsuario == jugador.NombreUsuario)
                {
                    ficha.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = Colors.Magenta,
                        Direction = 0,
                        ShadowDepth = 0,
                        Opacity = 1,
                        BlurRadius = 30
                    };
                }


                _fichasPorJugador[jugador.NombreUsuario] = ficha;
                _casillasRecorridasPorJugador[jugador.NombreUsuario] = 0;

                JugadorEnSala jugadorEnSala = new JugadorEnSala(jugador, this);
                if (SingletonJugador.Instance.Jugador.NombreUsuario.Equals(SalaActual.NombreHost))
                {
                    jugadorEnSala.CargarOpcionExpulsar();
                }

                _gridsJugadores[i].Children.Add(jugadorEnSala);
                _posicionesJugadores[jugador.NombreUsuario] = 0;

                TableroCanvas.Children.Add(ficha);
                Canvas.SetLeft(ficha, posicionInicial.X + (i));
                Canvas.SetTop(ficha, posicionInicial.Y);
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
                BtnDados.IsEnabled = true;
                BtnAbandonar.IsEnabled = true;
            }
            else
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico(Properties.Resources.lbEsTurnoDe + nombreJugadorEnTurno, Properties.Resources.tituloHoraDeJugar, 3);
                ventanaTurno.Show();
                BtnDados.IsEnabled = false;
                BtnAbandonar.IsEnabled = false;
            }
        }

        private void LanzarDado(object sender, RoutedEventArgs e)
        {
            string nombreJugador = SingletonJugador.Instance.Jugador.NombreUsuario;

            try
            {
                if (SalaActual.Jugadores.TryGetValue(nombreJugador, out Jugador jugador))
                {
                    if (jugador.TurnosPerdidos > 0)
                    {
                        MessageBox.Show(Properties.Resources.msgPierdesUnTurno + $"{jugador.TurnosPerdidos}");
                        jugador.TurnosPerdidos--;
                        pasarTurnoSiguienteJugador();
                        return;
                    }
                }

                Random random = new Random();
                int numeroAleatorio = random.Next(1, 7);
                MessageBox.Show(Properties.Resources.msgLanzarDado + $"{numeroAleatorio}.");
                MoverFicha(numeroAleatorio, nombreJugador);
            }
            catch (CommunicationException)
            {
                ManejarCaidaServidor();
            }
            catch (TimeoutException)
            {
                ManejarCaidaServidor();
            }
        }


        private async void pasarTurnoSiguienteJugador()
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
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgErrorCambiarTurno);
            }
        }

        private async void MoverFicha(int pasos, string nombreJugador)
        {
            BtnDados.IsEnabled = false;
            BtnAbandonar.IsEnabled = false;

            if (_posicionesJugadores.TryGetValue(nombreJugador, out int posicionActual))
            {
                int nuevaPosicion = posicionActual + pasos;
                _pocisionAnterior = nuevaPosicion;

                if (nuevaPosicion > 63)
                {
                    MessageBox.Show(Properties.Resources.msgNecesitasNumeroExacto);
                    pasarTurnoSiguienteJugador();
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
                        EvaluarCasilla(posicion, nombreJugador);
                    }

                    await Task.Delay(300);
                }
                _servicioJugabilidad.JugarTurno(pasos, SalaActual.Codigo, nombreJugador);
            }
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
            Jugador jugador = SalaActual.Jugadores[nombreJugador];

            if (_casillasDeOca.Contains(posicion))
            {
                if (posicion == 59 && ((_pocisionAnterior - posicion) == 54))
                {
                    pasarTurnoSiguienteJugador();
                    return;
                }
                else if (posicion == 59)
                {
                    MessageBox.Show(Properties.Resources.msgOcaDorada);
                    var trayecto = GenerarTrayectoria(59, 63);

                    foreach (var pos in trayecto)
                    {
                        _posicionesJugadores[nombreJugador] = pos;
                        ActualizarInterfazGrafica(pos, nombreJugador);
                        await _clientePartida.NotificarMovimientoFichaAsync(pos, nombreJugador, SalaActual.Codigo);
                        await Task.Delay(300);
                    }

                    await _clientePartida.NotificarMovimientoFichaAsync(posicion, nombreJugador, SalaActual.Codigo);
                    return;
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgDeOcaAOca);
                    BtnDados.IsEnabled = true;
                    BtnAbandonar.IsEnabled = true;
                    return;
                }
            }

            if (_casillasPuente.Contains(posicion))
            {
                MessageBox.Show(Properties.Resources.msgPuente);
                BtnDados.IsEnabled = true;
                BtnAbandonar.IsEnabled = true;
                return;
            }

            if (_casillasPosada.Contains(posicion))
            {
                MessageBox.Show(Properties.Resources.msgPosada);
                jugador.TurnosPerdidos = 1;
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasDado.Contains(posicion))
            {
                MessageBox.Show(Properties.Resources.msgDados);
                BtnDados.IsEnabled = true;
                BtnAbandonar.IsEnabled = true;
                return;
            }

            if (_casillasPozo.Contains(posicion))
            {
                MessageBox.Show(Properties.Resources.msgPozo);
                jugador.TurnosPerdidos = 3;
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasLaberinto.Contains(posicion))
            {
                MessageBox.Show(Properties.Resources.msgLaberinto);
                _posicionesJugadores[nombreJugador] = 30;
                ActualizarInterfazGrafica(30, nombreJugador);
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasCarcel.Contains(posicion))
            {
                MessageBox.Show(Properties.Resources.msgCarcel);
                jugador.TurnosPerdidos = 2;
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasCalavera.Contains(posicion))
            {
                MessageBox.Show(Properties.Resources.msgCalavera);
                _posicionesJugadores[nombreJugador] = 1;
                ActualizarInterfazGrafica(1, nombreJugador);
                pasarTurnoSiguienteJugador();
                return;
            }

            if (_casillasMeta.Contains(posicion))
            {
                await _clientePartida.NotificarMovimientoFichaAsync(posicion, nombreJugador, SalaActual.Codigo);
                return;
            }

            pasarTurnoSiguienteJugador();
        }

        public void MostrarPantallaVictoria(KeyValuePair<string, int>[] jugadoresOrdenados)
        {
            Dispatcher.Invoke(() =>
            {
                Victoria ventanaVictoria = new Victoria(jugadoresOrdenados.ToList());
                ventanaVictoria.Show();
                this.Close();
            });
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
                }
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
        }

        public void MostrarNuevoJugadorEnTurno(string nombreNuevoJugadorEnTurno)
        {
            SalaActual.Partida.NombreJugadorEnTurno = nombreNuevoJugadorEnTurno;
            lbNombreJugadorEnTurno.Content = nombreNuevoJugadorEnTurno;

            if (nombreNuevoJugadorEnTurno.Equals(SingletonJugador.Instance.Jugador.NombreUsuario))
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico(Properties.Resources.msgEsTuTurno, Properties.Resources.tituloHoraDeJugar, 2);
                ventanaTurno.Show();
                BtnDados.IsEnabled = true;
                BtnAbandonar.IsEnabled = true;
            }
            else
            {
                VentanaCierreAutomatico ventanaTurno = new VentanaCierreAutomatico(Properties.Resources.lbEsTurnoDe + nombreNuevoJugadorEnTurno, Properties.Resources.tituloHoraDeJugar, 2);
                ventanaTurno.Show();
                BtnDados.IsEnabled = false;
                BtnAbandonar.IsEnabled = false;
            }
        }

        public void MostrarDesconexionJugador(string nombreJugador)
        {
            throw new NotImplementedException();
        }

        public void ExpulsarAMenúPrincipal(string motivo)
        {
            throw new NotImplementedException();
        }

        public void ActualizarEstadoAmistad(string nombreJugadorEmisor)
        {
            throw new NotImplementedException();
        }
        
        private async void BtnAbandonar_Click(object sender, RoutedEventArgs e)
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

                        if (_fichasPorJugador.TryGetValue(nombreJugador, out Image ficha))
                        {
                            TableroCanvas.Children.Remove(ficha);
                            _fichasPorJugador.Remove(nombreJugador);
                        }

                        Dispatcher.Invoke(() =>
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
                catch (Exception)
                {
                    MessageBox.Show(Properties.Resources.msgErrorAbandonoPartida);
                }
            }
        }

        public void NotificarAbandonoJugador(string nombreJugador)
        {
            Dispatcher.Invoke(() =>
            {
                lbNotificacion.Content = $"{nombreJugador}" + Properties.Resources.msgHaAbandonadoLaPartida;
                lbNotificacion.Visibility = Visibility.Visible;

                if (_fichasPorJugador.TryGetValue(nombreJugador, out Image ficha))
                {
                    TableroCanvas.Children.Remove(ficha);
                    _fichasPorJugador.Remove(nombreJugador);
                }

                foreach (var panelJugador in FichasJugadoresPanel.Children.OfType<StackPanel>())
                {
                    var nombreTextBlock = panelJugador.Children.OfType<TextBlock>().FirstOrDefault();
                    if (nombreTextBlock != null && nombreTextBlock.Text == nombreJugador)
                    {
                        nombreTextBlock.Foreground = Brushes.Red;
                        break;
                    }
                }

                foreach (var grid in _gridsJugadores)
                {
                    foreach (var child in grid.Children.OfType<JugadorEnSala>().ToList())
                    {
                        if (child.lbNombreJugador.Content.ToString() == nombreJugador)
                        {
                            grid.Children.Remove(child);
                            break;
                        }
                    }
                }
            });
        }


        private void ManejarCaidaServidor()
        {
            MessageBox.Show(
                "El servidor se encuentra fuera de servicio. La aplicación se cerrará automáticamente.",
                "Servidor no disponible",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );

            Application.Current.Shutdown();
        }


        private async Task VerificarConexionConServidor()
        {
            while (true)
            {
                try
                {
                    await _clientePartida.HeartbeatAsync();
                }
                catch (CommunicationException)
                {
                    ManejarCaidaServidor();
                    break;
                }
                catch (TimeoutException)
                {
                    ManejarCaidaServidor();
                    break;
                }

                await Task.Delay(5000);
            }
        }
        private async Task GuardarEstadisticasAsync(int idJugador, int casillasRecorridas, bool ganoPartida)
        {
            try
            {
                await _clientePartida.GuardarEstadisticasJugadorAsync(idJugador, casillasRecorridas, ganoPartida);
                MessageBox.Show("Estadísticas guardadas correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FaultException)
            {
                var resultado = MessageBox.Show(
                    "Hubo un error al guardar las estadísticas. ¿Deseas reintentar?",
                    "Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    await GuardarEstadisticasAsync(idJugador, casillasRecorridas, ganoPartida);
                }
            }
        }



    }
}