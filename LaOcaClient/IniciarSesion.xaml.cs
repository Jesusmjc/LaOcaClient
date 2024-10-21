using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using System.Windows.Shapes;
using LaOcaClient.LaOcaService;

namespace LaOcaClient
{
    public partial class IniciarSesion : Window
    {
        LaOcaService.ServicioInicioSesionClient cliente;

        public IniciarSesion()
        {
            InitializeComponent();
            cliente = new LaOcaService.ServicioInicioSesionClient();
        }

        private void ManejarFocoTextBox(object remitente, RoutedEventArgs e)
        {
            TextBox cuadroTexto = remitente as TextBox;
            if (cuadroTexto.Text == cuadroTexto.Tag.ToString())
            {
                cuadroTexto.Text = "";
                cuadroTexto.Foreground = Brushes.Black;
            }
        }

        private void ManejarPerdidaFocoTextBox(object remitente, RoutedEventArgs e)
        {
            TextBox cuadroTexto = remitente as TextBox;
            if (string.IsNullOrWhiteSpace(cuadroTexto.Text))
            {
                cuadroTexto.Text = "";
                cuadroTexto.Foreground = Brushes.Gray;
            }
        }

        private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
        {
            string correoElectronico = tbxCorreoElectronico.Text.ToString();
            string contrasena = pwbContrasena.Password.ToString();

            OcultarMensajesError();

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
                    lbCorreoInvalido.Visibility = Visibility.Visible;
                    lbCaracteristicasCorreoValido.Visibility = Visibility.Visible;
                }
                else
                {
                    lbContrasenaInvalida.Visibility = Visibility.Visible;
                    lbCaracteristicasContrasenaValida.Visibility = Visibility.Visible;
                }
            }
            else
            {
                lbCamposVacios.Visibility = Visibility.Visible;
            }
        }

        private void IniciarSesionEnServidor(Cuenta cuentaInicioSesion)
        {
            Jugador jugadorInicioSesion;

            try
            {
                jugadorInicioSesion = cliente.IniciarSesion(cuentaInicioSesion);

                SingletonJugador.Instance.IdJugador = jugadorInicioSesion.IdJugador;
                SingletonJugador.Instance.NombreJugador = jugadorInicioSesion.NombreUsuario;
                SingletonJugador.Instance.CorreoElectronico = tbxCorreoElectronico.Text.ToString();
                SingletonJugador.Instance.EsInvitado = false;

                Chat ventanaChat = new Chat();
                this.Close();
                ventanaChat.ShowDialog();
            }
            catch (FaultException<InicioSesionException> ex)
            {
                MessageBox.Show(ex.Detail.Mensaje + ex.Reason, "Error al iniciar sesión", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void OcultarMensajesError()
        {
            lbCamposVacios.Visibility = Visibility.Hidden;
            lbCorreoInvalido.Visibility = Visibility.Hidden;
            lbCaracteristicasCorreoValido.Visibility = Visibility.Hidden;
            lbContrasenaInvalida.Visibility = Visibility.Hidden;
            lbCaracteristicasContrasenaValida.Visibility = Visibility.Hidden;
        }

        private void btnOlvideMiContraseña_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCrearCuentaNueva_Click(object sender, RoutedEventArgs e)
        {
            //CrearCuenta crearCuentaWindow = new CrearCuenta();
            //crearCuentaWindow.Show();
            this.Close();
        }

        private void btnJugarComoInvitado_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LimpiarTextoEjemplo(object sender, MouseButtonEventArgs e)
        {
            if (tbxCorreoElectronico.Text.ToString().Equals("Correo Electronico"))
            {
                tbxCorreoElectronico.Text = "";
            }
        }
    }
}
