using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Configuration;
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

namespace LaOcaClient
{
    public partial class IniciarSesion : Window
    {
        LaOcaService.ServicioInicioSesionClient _clienteInicioSesion;

        public IniciarSesion()
        {
            InitializeComponent();
            _clienteInicioSesion = new LaOcaService.ServicioInicioSesionClient();
        }

        private void CbIdioma_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbIdioma.SelectedItem is ComboBoxItem selectedItem)
            {
                string cultureCode = selectedItem.Tag.ToString();
                CambiarIdioma(cultureCode);
            }
        }

        private void CambiarIdioma(string cultureCode)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);

            IniciarSesion nuevaVentana = new IniciarSesion();
            nuevaVentana.Show();
            this.Close();
        }

        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            string correoElectronico = tbxCorreoElectronico.Text.ToString();
            string contrasena = pwbContrasena.Password.ToString();


            if (!string.IsNullOrWhiteSpace(correoElectronico) && !string.IsNullOrWhiteSpace(contrasena))
            {
                if (Utilidad.ValidarCorreoElectronico(correoElectronico) && Utilidad.ValidarContrasena(contrasena))
                {
                    contrasena = Utilidad.HashearConSha256(contrasena);

                    Cuenta cuentaInicioSesion = new Cuenta
                    {
                        CorreoElectronico = correoElectronico,
                        Contrasena = contrasena,
                    };

                    IniciarSesionEnServidor(cuentaInicioSesion);
                }
                else if (!Utilidad.ValidarCorreoElectronico(correoElectronico))
                {
                    MessageBox.Show(Properties.Resources.lbIngreseCorreoValido, Properties.Resources.tituloCorreoInvalido, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(Properties.Resources.lbContraseñaValida, Properties.Resources.tituloContraseñaInvalida, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(Properties.Resources.camposVaciosLogin, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void IniciarSesionEnServidor(Cuenta cuentaInicioSesion)
        {
            Jugador jugadorInicioSesion;

            try
            {
                jugadorInicioSesion = _clienteInicioSesion.IniciarSesion(cuentaInicioSesion);
                jugadorInicioSesion.EsInvitado = false;

                LaOcaService.ServicioJugadoresEnLineaClient clienteJugadoresEnLinea = new LaOcaService.ServicioJugadoresEnLineaClient();
                clienteJugadoresEnLinea.AgregarJugadorConectado(jugadorInicioSesion);

                SingletonJugador.Instance.Jugador = jugadorInicioSesion;

                MenuPrincipal menuPrincipalWindow = new MenuPrincipal();
                menuPrincipalWindow.Show();
                this.Close();
            }
            catch (FaultException<InicioSesionException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + "\n" + ex.Reason, Properties.Resources.globalErrorLogin, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show(ex.InnerException.Message, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOlvideMiContraseña_Click(object sender, RoutedEventArgs e)
        {
            RecuperarContraseña ventanaRecuperarContraseña = new RecuperarContraseña();
            ventanaRecuperarContraseña.Show();
            this.Close();
        }

        private void BtnCrearCuentaNueva_Click(object sender, RoutedEventArgs e)
        {
            CrearCuenta crearCuentaWindow = new CrearCuenta(ModoCuenta.Crear);
            crearCuentaWindow.Show();
            this.Close();
        }

        private void LimpiarTextoEjemplo(object sender, MouseButtonEventArgs e)
        {
            if (tbxCorreoElectronico.Text.ToString().Equals(Properties.Resources.globalCorreo))
            {
                tbxCorreoElectronico.Text = "";
            }
        }

        private void EntrarComoInvitado(object sender, RoutedEventArgs e)
        {
            Jugador jugadorInvitado = new Jugador
            {
                EsInvitado = true,
                NombreUsuario = "Invitado" + DateTime.Now.Second.ToString("D2") + DateTime.Now.Minute.ToString("D2")
                                + DateTime.Now.Hour.ToString("D2") + DateTime.Now.Day.ToString("D2")
            };
            SingletonJugador.Instance.Jugador = jugadorInvitado;
            
            try
            {
                LaOcaService.ServicioJugadoresEnLineaClient clienteJugadoresEnLinea = new LaOcaService.ServicioJugadoresEnLineaClient();
                clienteJugadoresEnLinea.AgregarJugadorConectado(jugadorInvitado);

                MenuPrincipal menuPrincipalWindow = new MenuPrincipal();
                menuPrincipalWindow.Show();
                this.Close();
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
    }
}
