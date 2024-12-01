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
    /// <summary>
    /// Interaction logic for JugadorEnSala.xaml
    /// </summary>
    public partial class JugadorEnSala : UserControl
    {
        public Jugador jugadorEnSala;

        private IVentanaSala _ventanaPadre;

        private ServicioAmistadClient _clienteAmistad;
        private Amistad _amistad;
        private bool _esJugadorActualHost = false;

        public JugadorEnSala(Jugador jugadorEnSala, IVentanaSala ventanaSala)
        {
            InitializeComponent();

            this.jugadorEnSala = jugadorEnSala;

            lbNombreJugador.Content = jugadorEnSala.NombreUsuario;

            _clienteAmistad = new ServicioAmistadClient();
            _ventanaPadre = ventanaSala;

            if (SingletonJugador.Instance.Jugador.Equals(jugadorEnSala))
            {
                imgMasOpciones.Visibility = Visibility.Hidden;
            }
            else
            {
                AjustarMenuPopupSegunAmistad();
            }


            if (SingletonJugador.Instance.Jugador.Equals(_ventanaPadre.SalaActual.NombreHost))
            {
                _esJugadorActualHost = true;
            }
        }

        public void AjustarMenuPopupSegunAmistad()
        {
            try
            {
                _amistad = _clienteAmistad.RecuperarAmistad(SingletonJugador.Instance.Jugador.IdJugador, jugadorEnSala.IdJugador);
                CargarMenuPopup();
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, "Error al enviar la solicitud.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarMenuPopup()
        {
            imgMasOpciones_MenuContextual.Items.Clear();

            switch (_amistad.Estado)
            {
                case "Solicitud":
                case "Amigos":
                    CargarOpcionBloquear();
                    break;

                case "Bloqueo":
                    if (_amistad.IdJugadorReceptor != SingletonJugador.Instance.Jugador.IdJugador)
                    {
                        CargarOpcionDesbloquear();
                    }
                    break;

                default: // "Rechazada" o No existe amistad
                    CargarOpcionEnviarSolicitudAmistad();
                    CargarOpcionBloquear();
                    break;
            }

            if (_esJugadorActualHost)
            {
                CargarOpcionExpulsar();
            }
        }

        private void CargarOpcionEnviarSolicitudAmistad()
        {
            MenuItem opcionEnviarSolicitudAmistad = new MenuItem { Header = "Enviar Solicitud de Amistad" };
            opcionEnviarSolicitudAmistad.Click += (s, args) => EnviarSolicitudAmistad();
            imgMasOpciones_MenuContextual.Items.Add(opcionEnviarSolicitudAmistad);
        }

        private void CargarOpcionBloquear()
        {
            MenuItem opcionBloquearJugador = new MenuItem { Header = "Bloquear" };
            opcionBloquearJugador.Click += (s, args) => BloquearJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionBloquearJugador);
        }

        private void CargarOpcionDesbloquear()
        {
            MenuItem opcionDesbloquearJugador = new MenuItem { Header = "Desbloquear" };
            opcionDesbloquearJugador.Click += (s, args) => DesbloquearJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionDesbloquearJugador);
        }

        private void CargarOpcionExpulsar()
        {
            MenuItem opcionExpulsar = new MenuItem { Header = "Expulsar" };
            opcionExpulsar.Click += (s, args) => ExpulsarJugador();
            imgMasOpciones_MenuContextual.Items.Add(opcionExpulsar);
        }

        private void MostrarMenuPopup(object sender, MouseButtonEventArgs e)
        {
            if (_amistad.Estado == EstadoAmistad.BLOQUEO && _amistad.IdJugadorReceptor == SingletonJugador.Instance.Jugador.IdJugador)
            {
                MessageBox.Show("Parece que el jugador te ha bloqueado.", "Estás bloqueado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                imgMasOpciones_MenuContextual.IsOpen = true;
            }
        }

        private void EnviarSolicitudAmistad()
        {
            try
            {
                if (_amistad.IdAmistad == 0)
                {
                    _clienteAmistad.EnviarSolicitudAmistad(SingletonJugador.Instance.Jugador, jugadorEnSala);
                }
                else
                {
                    _clienteAmistad.ActualizarSolicitudAmistad(_amistad, EstadoAmistad.SOLICITUD);
                }
                
                _ventanaPadre.ClienteJugadoresEnSala.NotificarCambioEnAmistad(_ventanaPadre.SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario, jugadorEnSala.NombreUsuario);
                ActualizarOpcionesDeMenuPopupLocal(EstadoAmistad.SOLICITUD);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, "Error al enviar la solicitud.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde." + ex.Message, "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BloquearJugador()
        {
            try
            {
                if (_amistad.IdJugadorReceptor.Equals(SingletonJugador.Instance.Jugador.IdJugador))
                {
                    _amistad.IdJugadorReceptor = _amistad.IdJugadorSolicitante;
                    _amistad.IdJugadorSolicitante = SingletonJugador.Instance.Jugador.IdJugador;
                }

                _clienteAmistad.ActualizarSolicitudAmistad(_amistad, EstadoAmistad.BLOQUEO);
                _ventanaPadre.ClienteJugadoresEnSala.NotificarCambioEnAmistad(_ventanaPadre.SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario, jugadorEnSala.NombreUsuario);
                ActualizarOpcionesDeMenuPopupLocal(EstadoAmistad.BLOQUEO);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, "Error al procesar el bloqueo.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExpulsarJugador()
        {
            _ventanaPadre.ClienteJugadoresEnSala?.ExpulsarJugador(_ventanaPadre.SalaActual.Codigo, jugadorEnSala.NombreUsuario);
        }

        private void DesbloquearJugador()
        {
            try
            {
                _clienteAmistad.ActualizarSolicitudAmistad(_amistad, EstadoAmistad.RECHAZADA);
                _ventanaPadre.ClienteJugadoresEnSala.NotificarCambioEnAmistad(_ventanaPadre.SalaActual.Codigo, SingletonJugador.Instance.Jugador.NombreUsuario, jugadorEnSala.NombreUsuario);
                ActualizarOpcionesDeMenuPopupLocal(EstadoAmistad.RECHAZADA);
            }
            catch (FaultException<AmistadException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, "Error al procesar el bloqueo.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("El servidor ha tardado demasiado en responder.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show("Ha ocurrido un error al intentar conectar con el Servidor. Por favor intente de nuevo más tarde.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ActualizarOpcionesDeMenuPopupLocal(string nuevoEstadoAmistad)
        {
            imgMasOpciones_MenuContextual.Items.Clear();
            _amistad.Estado = nuevoEstadoAmistad;

            CargarMenuPopup();
        }

        public void ActualizarOpcionesDeMenuPopupCallback(string nuevoEstadoAmistad)
        {
            imgMasOpciones_MenuContextual.Items.Clear();
            _amistad = _clienteAmistad.RecuperarAmistad(SingletonJugador.Instance.Jugador.IdJugador, jugadorEnSala.IdJugador);

            CargarMenuPopup();
        }
    }
}
