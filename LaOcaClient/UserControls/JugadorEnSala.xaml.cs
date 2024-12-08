using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LaOcaClient.UserControls
{
    public partial class JugadorEnSala : UserControl
    {
        public Jugador jugadorEnSala;

        private ServicioAmistadClient _clienteAmistad;
        private Amistad _amistad;
        private string _codigoSala;
        private bool _esJugadorActualHost = false;

        public JugadorEnSala(Jugador jugadorEnSala, string codigoSala)
        {
            InitializeComponent();

            lbNombreJugador.Content = jugadorEnSala.NombreUsuario;

            string rutaFotoPerfil = FotoPerfilUtils.ObtenerRutaFotoPerfil(jugadorEnSala.IdFotoPerfil);
            imgFotoPerfil.Source = new BitmapImage(new Uri(rutaFotoPerfil, UriKind.RelativeOrAbsolute));
        }

        private void RecuperarAmistadConJugador()
        {
            try
            {
                _amistad = _clienteAmistad.RecuperarAmistad(SingletonJugador.Instance.Jugador.IdJugador, jugadorEnSala.IdJugador);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloInvitacionNoEnviada, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarMenuPopup()
        {
            switch (_amistad.Estado)
            {
                case "Solicitud":
                    CargarOpcionBloquear();
                    break;

                case "Amigos":
                    CargarOpcionBloquear();
                    break;

                case "Bloqueo":
                    CargarOpcionDesbloquear();
                    break;

                default:
                    CargarOpcionEnviarSolicitudAmistad();
                    CargarOpcionBloquear();
                    break;
            }
        }

        private void CargarOpcionEnviarSolicitudAmistad()
        {
            MenuItem opcionEnviarSolicitudAmistad = new MenuItem { Header = Properties.Resources.lbEnviarSolicitudAmistad };
            opcionEnviarSolicitudAmistad.Click += (s, args) => EnviarSolicitudAmistad();
            imgMasOpciones_MenuContextual.Items.Add(opcionEnviarSolicitudAmistad);
        }

        private void CargarOpcionBloquear()
        {
            MenuItem opcionBloquearJugador = new MenuItem { Header = Properties.Resources.lbBloquear };
            opcionBloquearJugador.Click += (s, args) => BloquearJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionBloquearJugador);

            _esJugadorActualHost = true;
        }

        private void CargarOpcionDesbloquear()
        {
            MenuItem opcionDesbloquearJugador = new MenuItem { Header = Properties.Resources.lbDesbloquear };
            opcionDesbloquearJugador.Click += (s, args) => DesbloquearJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionDesbloquearJugador);
        }

        public void CargarOpcionExpulsar()
        {
            MenuItem opcionExpulsar = new MenuItem { Header = Properties.Resources.lbExpulsar };
            opcionExpulsar.Click += (s, args) => ExpulsarJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionExpulsar);
        }

        private void MostrarMenuPopup(object sender, MouseButtonEventArgs e)
        {
            imgMasOpciones_MenuContextual.IsOpen = true;
        }

        private void EnviarSolicitudAmistad()
        {
            try
            {
                _clienteAmistad.EnviarSolicitudAmistad(SingletonJugador.Instance.Jugador, jugadorEnSala);
                ActualizarOpcionesDeMenúPopup(EstadoAmistad.SOLICITUD);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloInvitacionNoEnviada, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BloquearJugador()
        {
            try
            {
                _clienteAmistad.ActualizarSolicitudAmistad(_amistad, EstadoAmistad.BLOQUEO);
                ActualizarOpcionesDeMenúPopup(EstadoAmistad.BLOQUEO);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloErrorBloqueo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExpulsarJugador()
        {
            ServicioExpulsionSalaClient clienteExpulsionSala = new ServicioExpulsionSalaClient();
            clienteExpulsionSala.ExpulsarJugador(_codigoSala, jugadorEnSala.NombreUsuario);
        }

        private void DesbloquearJugador()
        {
            try
            {
                _clienteAmistad.ActualizarSolicitudAmistad(_amistad, EstadoAmistad.RECHAZADA);
                ActualizarOpcionesDeMenúPopup(EstadoAmistad.RECHAZADA);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.tituloErrorBloqueo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.tituloTimeOut, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarOpcionesDeMenúPopup(string nuevoEstadoAmistad)
        {
            imgMasOpciones_MenuContextual.Items.Clear();
            _amistad.Estado = nuevoEstadoAmistad;
            CargarMenuPopup();

            if (_esJugadorActualHost)
            {
                CargarOpcionExpulsar();
            }
        }
    }
}
