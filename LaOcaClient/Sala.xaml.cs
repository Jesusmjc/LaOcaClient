using LaOcaClient.LaOcaService;
using LaOcaClient.UserControls;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.ServiceModel;
using System.ServiceModel.Security;
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

namespace LaOcaClient
{
    public partial class Sala : Window, IVentanaSala, IServicioChatCallback, IServicioSalaCallback, IServicioActualizacionJugadoresEnSalaCallback
    {
        public LaOcaService.Sala SalaActual { get; set; }
        public ServicioActualizacionJugadoresEnSalaClient ClienteJugadoresEnSala {  get; set; }
        public bool EstaAbierta = true;

        private LaOcaService.ServicioCuentaClient _clienteCuenta = new LaOcaService.ServicioCuentaClient();
        private IniciarSesion _ventanaIniciarSesion = new IniciarSesion();
        private InstanceContext _contexto;
        private LaOcaService.ServicioChatClient _clienteChat;
        private LaOcaService.ServicioSalaClient _clienteSala;
        private Grid[] _gridsJugadores;
        private JugadorEnSala[] _jugadoresEnSala;
        private Social _ventanaSocial;
        private bool _ventanaEstaAbierta = true;
        private IniciarSesion _iniciarSesion = new IniciarSesion();

        public Sala()
        {
            try
            {
                InitializeComponent();

                PrepararSala();
                MostrarPrimerJugador();
                UnirseAlChat();
            }
            catch (RegresarAInicioSesionException) 
            {
                RedirigirAInicioSesion();   
            }
        }

        public Sala(string nombreSala, string visibilidad)
        {
            try
            {
                InitializeComponent();

                PrepararSala();
                CrearSala(nombreSala, visibilidad);
                MostrarPrimerJugador();
                UnirseAlChat();
            }
            catch (RegresarAInicioSesionException)
            {
                RedirigirAInicioSesion();
            }
        }

        public Sala(LaOcaService.Sala sala)
        {
            try
            {
                InitializeComponent();

                this.SalaActual = sala;
                lbNombreSala.Content = SalaActual.Nombre;
                lbCodigoSala.Content = SalaActual.Codigo;

                PrepararSala();
                MostrarJugadoresEnSala();
                AgregarJugadorASala();
                UnirseAlChat();
            }
            catch (RegresarAInicioSesionException)
            {
                RedirigirAInicioSesion();
            }
        }

        private void PrepararSala()
        {
            _gridsJugadores = new Grid[4];
            _gridsJugadores[0] = gridJugadorSala1;
            _gridsJugadores[1] = gridJugadorSala2;
            _gridsJugadores[2] = gridJugadorSala3;
            _gridsJugadores[3] = gridJugadorSala4;
            _jugadoresEnSala = new JugadorEnSala[4];
            _contexto = new InstanceContext(this);
            _clienteChat = new LaOcaService.ServicioChatClient(_contexto);
            _clienteSala = new LaOcaService.ServicioSalaClient(_contexto);
            ClienteJugadoresEnSala = new LaOcaService.ServicioActualizacionJugadoresEnSalaClient(_contexto);
        }

        private void CrearSala(string nombreSala, string visibilidad)
        {
            btnIniciarPartida.Visibility = Visibility.Visible;

            LaOcaService.Sala nuevaSala = new LaOcaService.Sala()
            {
                Nombre = nombreSala,
                Codigo = GenerarCodigoSala(),
                Jugadores = new Dictionary<string, LaOcaService.Jugador>(),
                Visibilidad = visibilidad,
                NombreHost = SingletonJugador.Instance.Jugador.NombreUsuario
            };

            nuevaSala.Jugadores.Add(SingletonJugador.Instance.Jugador.NombreUsuario, SingletonJugador.Instance.Jugador);
            SalaActual = nuevaSala;

            lbNombreSala.Content = nuevaSala.Nombre;
            lbCodigoSala.Content = nuevaSala.Codigo;

            int resultadoAgregarSala = 0;

            try
            {
                resultadoAgregarSala = _clienteSala.AgregarNuevaSala(nuevaSala);
                ClienteJugadoresEnSala.AgregarCanalCallbackActualizacionJugadoresEnSala(SingletonJugador.Instance.Jugador.NombreUsuario, SalaActual.Codigo);

                if (resultadoAgregarSala == 0)
                {
                    MessageBox.Show(Properties.Resources.msgErrorCrearSala, Properties.Resources.tituloErrorSala, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(_clienteCuenta);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                    _clienteSala = new ServicioSalaClient(new InstanceContext (this));
                }
                catch (RegresarAInicioSesionException)
                {
                    RedirigirAInicioSesion();
                }
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarPrimerJugador()
        {
            JugadorEnSala jugadorSala = new JugadorEnSala(SingletonJugador.Instance.Jugador, this);
            _gridsJugadores[0].Children.Add(jugadorSala);
            _jugadoresEnSala[0] = jugadorSala;
        }

        private void MostrarJugadoresEnSala()
        {
            JugadorEnSala hostEnSala = new JugadorEnSala(SalaActual.Jugadores[SalaActual.NombreHost], this);
            _gridsJugadores[0].Children.Add(hostEnSala);
            _jugadoresEnSala[0] = hostEnSala;

            JugadorEnSala jugadorSala = new JugadorEnSala(SingletonJugador.Instance.Jugador, this);
            _gridsJugadores[SalaActual.Jugadores.Count].Children.Add(jugadorSala);
            _jugadoresEnSala[SalaActual.Jugadores.Count] = jugadorSala;

            int posicion = 1;

            foreach (var parJugador in SalaActual.Jugadores)
            {
                if (!parJugador.Key.Equals(SingletonJugador.Instance.Jugador.NombreUsuario) && !parJugador.Key.Equals(SalaActual.NombreHost))
                {
                    JugadorEnSala jugadorEnSala = new JugadorEnSala(parJugador.Value, this);
                   
                    _gridsJugadores[posicion].Children.Add(jugadorEnSala);
                    _jugadoresEnSala[posicion] = jugadorEnSala;

                    posicion++;
                }
            }
        }

        private string GenerarCodigoSala()
        {
            Random _random = new Random();
            string codigoSala;
            bool esCodigoUnico = false;

            do
            {
                StringBuilder codigoSalaBuilder = new StringBuilder();
                for (int i = 0; i < 4; i++)
                {
                    codigoSalaBuilder.Append(_random.Next(0, 10).ToString());
                }
                codigoSala = codigoSalaBuilder.ToString();

                try
                {
                    esCodigoUnico = _clienteSala.VerificarCodigoSalaEsUnico(codigoSala);
                }
                catch (TimeoutException)
                {
                    MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (CommunicationException)
                {
                    try
                    {
                        ManejadorExcepciones.ManejarCommunicationException(_clienteCuenta);
                        MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                        _clienteSala = new ServicioSalaClient(new InstanceContext(this));
                    }
                    catch (RegresarAInicioSesionException)
                    {
                        RedirigirAInicioSesion();
                    }
                }
            } while (!esCodigoUnico);

            return codigoSala;
        }

        private void AgregarJugadorASala()
        {
            SalaActual.Jugadores.Add(SingletonJugador.Instance.Jugador.NombreUsuario, SingletonJugador.Instance.Jugador);

            try
            {
                _clienteSala.AgregarJugadorASala(SingletonJugador.Instance.Jugador, SalaActual.Codigo);
                ClienteJugadoresEnSala.AgregarCanalCallbackActualizacionJugadoresEnSala(SingletonJugador.Instance.Jugador.NombreUsuario, SalaActual.Codigo);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(_clienteCuenta);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                    _clienteSala = new ServicioSalaClient(new InstanceContext(this));
                }
                catch (RegresarAInicioSesionException)
                {
                    RedirigirAInicioSesion();
                }
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnirseAlChat()
        {
            try
            {
                _clienteChat.UnirseAlChat(SingletonJugador.Instance.Jugador.NombreUsuario, SalaActual.Codigo);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(_clienteCuenta);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                    _clienteChat = new ServicioChatClient(new InstanceContext(this));
                }
                catch (RegresarAInicioSesionException)
                {
                    RedirigirAInicioSesion();
                }
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MostrarMensaje(string nombreJugador, string mensaje)
        {
            Dispatcher.Invoke(() =>
            {
                lbxChat.Items.Add($"{nombreJugador}: {mensaje}");
            });
        }

        private void EnviarMensaje(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string mensaje = tbxMensaje.Text.ToString();

                if (!string.IsNullOrWhiteSpace(mensaje))
                {
                    try
                    {
                        _clienteChat.EnviarMensaje(SingletonJugador.Instance.Jugador.NombreUsuario, mensaje, SalaActual.Codigo);
                        MostrarMensaje(SingletonJugador.Instance.Jugador.NombreUsuario, mensaje);
                    }
                    catch (TimeoutException)
                    {
                        MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (CommunicationException)
                    {
                        try
                        {
                            ManejadorExcepciones.ManejarCommunicationException(_clienteCuenta);
                            MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                            _clienteChat = new ServicioChatClient(new InstanceContext(this));
                        }
                        catch (RegresarAInicioSesionException)
                        {
                            RedirigirAInicioSesion();
                        }
                    }
                }

                tbxMensaje.Text = "";
            }
        }

        public void MostrarNuevoJugadorEnSala(Jugador nuevoJugador)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                JugadorEnSala nuevoJugadorEnSala = new JugadorEnSala(nuevoJugador, this);
                _gridsJugadores[SalaActual.Jugadores.Count].Children.Add(nuevoJugadorEnSala);
                _jugadoresEnSala[SalaActual.Jugadores.Count] = nuevoJugadorEnSala;

                SalaActual.Jugadores.Add(nuevoJugador.NombreUsuario, nuevoJugador);

                if (SalaActual.Jugadores.Count > 1)
                {
                    btnIniciarPartida.IsEnabled = true;
                }
            });
        }

        private void LimpiarTextoEjemplo(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && tbxMensaje.Text.ToString().Equals(Properties.Resources.lbEscribeUnMsg))
            {
                textBox.Clear();
            }
        }

        private void IniciarPartida(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_clienteCuenta.ProbarConexionConBD() && _clienteCuenta.ProbarConexionConServidor())
                {
                    if (SalaActual.Jugadores.Count >= 2)
                    {
                        LaOcaService.Partida nuevaPartida = _clienteSala.IniciarPartida(SalaActual.Codigo);

                        SalaActual.Partida = nuevaPartida;

                        Partida ventanaPartida = new Partida(SalaActual);
                        this.Close();
                        ventanaPartida.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show(Properties.Resources.msgDosOMasJugadores, Properties.Resources.tituloMasJugadores, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(_clienteCuenta);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                    _clienteCuenta = new ServicioCuentaClient();
                }
                catch (RegresarAInicioSesionException)
                {
                    RedirigirAInicioSesion();
                }
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void MostrarVentanaDePartida(LaOcaService.Partida partida)
        {
            SalaActual.Partida = partida;

            Partida ventanaPartida = new Partida(SalaActual);
            this.Close();
            ventanaPartida.ShowDialog();
        }

        private void RegresarAMenuPrincipal(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult resultado = MessageBox.Show(Properties.Resources.msgAbandonarSala, Properties.Resources.tituloAbandonarPartida, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                if (!SingletonJugador.Instance.Jugador.NombreUsuario.Equals(SalaActual.NombreHost))
                {
                    try
                    {
                        ClienteJugadoresEnSala.NotificarDesconexion(SingletonJugador.Instance.Jugador.NombreUsuario, SalaActual.Codigo);

                        MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
                        this.Close();
                        ventanaMenuPrincipal.ShowDialog();
                    }
                    catch (TimeoutException)
                    {
                        MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (CommunicationException)
                    {
                        try
                        {
                            ManejadorExcepciones.ManejarCommunicationException(_clienteCuenta);
                            MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                            InstanceContext contexto = new InstanceContext(this);
                            ClienteJugadoresEnSala = new ServicioActualizacionJugadoresEnSalaClient(contexto);
                        }
                        catch (RegresarAInicioSesionException)
                        {
                            RedirigirAInicioSesion();
                        }
                    }
                }
                else
                {
                    try
                    {
                        ClienteJugadoresEnSala.EliminarSala(SalaActual.Codigo);

                        MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
                        this.Close();
                        ventanaMenuPrincipal.ShowDialog();
                    }
                    catch (TimeoutException)
                    {
                        MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (CommunicationException)
                    {
                        try
                        {
                            ManejadorExcepciones.ManejarCommunicationException(_clienteCuenta);
                            MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                            InstanceContext contexto = new InstanceContext(this);
                            ClienteJugadoresEnSala = new ServicioActualizacionJugadoresEnSalaClient(contexto);
                        }
                        catch (RegresarAInicioSesionException)
                        {
                            RedirigirAInicioSesion();
                        }
                    }
                }
            }
        }

        public void MostrarDesconexionJugador(string nombreJugador)
        {
            int posicionJugadorDesconectado = 0;

            for (int i = 1; i < SalaActual.Jugadores.Count ; i++)
            {
                if (_jugadoresEnSala[i].JugadorEnLaSala.NombreUsuario.Equals(nombreJugador))
                {
                    posicionJugadorDesconectado = i;
                    break;
                }
            }

            SalaActual.Jugadores.Remove(nombreJugador);

            LimpiarGrids();
            switch(posicionJugadorDesconectado)
            {
                case 1:
                    EliminarJugadorEnSalaEnSegundaPosicion();
                    break;
                
                case 2:
                    EliminarJugadorEnSalaEnTerceraPosicion();
                    break;

                case 3:
                    EliminarJugadorEnSalaEnCuartaPosicion();
                    break;
            }

            if (SalaActual.Jugadores.Count < 2)
            {
                btnIniciarPartida.IsEnabled = false;
            }
        }

        private void EliminarJugadorEnSalaEnSegundaPosicion()
        {
            if (SalaActual.Jugadores.Count == 3)
            {
                _jugadoresEnSala[1] = _jugadoresEnSala[2];
                _jugadoresEnSala[2] = _jugadoresEnSala[3];
                _jugadoresEnSala[3] = null;

                _gridsJugadores[1].Children.Add(_jugadoresEnSala[1]);
                _gridsJugadores[2].Children.Add(_jugadoresEnSala[2]);
            }
            else if (SalaActual.Jugadores.Count == 2)
            {
                _jugadoresEnSala[1] = _jugadoresEnSala[2];
                _jugadoresEnSala[2] = null;

                _gridsJugadores[1].Children.Add(_jugadoresEnSala[1]);
            }
        }

        private void EliminarJugadorEnSalaEnTerceraPosicion()
        {
            if (SalaActual.Jugadores.Count == 3)
            {
                _jugadoresEnSala[2] = _jugadoresEnSala[3];
                _jugadoresEnSala[3] = null;

                _gridsJugadores[1].Children.Add(_jugadoresEnSala[1]);
                _gridsJugadores[2].Children.Add(_jugadoresEnSala[2]);
            }
            else
            {
                _jugadoresEnSala[2] = null;
                _jugadoresEnSala[3] = null;

                _gridsJugadores[1].Children.Add(_jugadoresEnSala[1]);
            }
        }

        private void EliminarJugadorEnSalaEnCuartaPosicion()
        {
            _jugadoresEnSala[3] = null;

            _gridsJugadores[1].Children.Add(_jugadoresEnSala[1]);
            _gridsJugadores[2].Children.Add(_jugadoresEnSala[2]);
        }

        private void LimpiarGrids()
        {
            _gridsJugadores[3].Children.Clear();
            _gridsJugadores[2].Children.Clear();
            _gridsJugadores[1].Children.Clear();
        }

        public void ExpulsarAMenúPrincipal(string motivo)
        {
            MessageBox.Show(motivo, Properties.Resources.tituloExpulsadoSala, MessageBoxButton.OK, MessageBoxImage.Information);
            MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
            _ventanaSocial?.Close();
            _ventanaEstaAbierta = false;
            this.Close();
            ventanaMenuPrincipal.ShowDialog();
        }

        private void MostrarAmigos(object sender, RoutedEventArgs e)
        {
            Social ventanaAmigos = new Social(this);
            _ventanaSocial = ventanaAmigos;

            this.Hide();
            if (ventanaAmigos.EstaAbierta)
            {
                ventanaAmigos.ShowDialog();

                if (_ventanaEstaAbierta)
                {
                    this.Show();
                }
            }
        }

        public void ActualizarEstadoAmistad(string nombreJugadorEmisor)
        {
            try
            {
                for (int i = 0; i < SalaActual.Jugadores.Count; i++)
                {
                    if (_jugadoresEnSala[i].JugadorEnLaSala.NombreUsuario.Equals(nombreJugadorEmisor))
                    {
                        ServicioAmistadClient clienteAmistad = new ServicioAmistadClient();
                        Amistad amistad = clienteAmistad.RecuperarAmistad(SingletonJugador.Instance.Jugador.IdJugador, _jugadoresEnSala[i].JugadorEnLaSala.IdJugador);

                        _jugadoresEnSala[i].ActualizarOpcionesDeMenuPopupCallback(amistad);
                        break;
                    }
                }
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                try
                {
                    ManejadorExcepciones.ManejarCommunicationException(_clienteCuenta);
                    MessageBox.Show("No hay internet", Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (RegresarAInicioSesionException)
                {
                    RedirigirAInicioSesion();
                }
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.msgExcepcionGeneral, Properties.Resources.tituloExcepcionGeneral, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RedirigirAInicioSesion()
        {
            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            this.Close();
            EstaAbierta = false;
            ventanaIniciarSesion.Show();
        }
    }
}