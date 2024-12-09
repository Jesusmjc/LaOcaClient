using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ServiceModel;

namespace LaOcaClient
{
    public partial class RecuperarContraseña : Window
    {
        private readonly IServicioCuenta _servicioCuenta;

        public RecuperarContraseña()
        {
            InitializeComponent();
            _servicioCuenta = new ServicioCuentaClient();
        }

        private void btnEnviarCodigoRestablecimiento_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreoElectronico.Text;

            if (string.IsNullOrEmpty(correo))
            {
                MessageBox.Show(Properties.Resources.msgCorreoNoIngresado, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Utilidad.ValidarCorreoElectronico(correo))
            {
                MessageBox.Show(Properties.Resources.lbIngreseCorreoValido, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _servicioCuenta.EnviarCodigoVerificacion(correo);
                MessageBox.Show(Properties.Resources.msgCodigoEnviado, "", MessageBoxButton.OK, MessageBoxImage.Information);
                ActualizarVentanaCodigoVerificacion();
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.globalErrorServidor, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int idCuenta;

        private void btnVerificarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreoElectronico.Text;
            string codigoIngresado = tbCodigoRestablecimiento.Text;

            if (string.IsNullOrEmpty(codigoIngresado))
            {
                MessageBox.Show(Properties.Resources.msgCodigoNoIngresado, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int cuentaId = _servicioCuenta.VerificarCodigoRecuperarContraseña(correo, codigoIngresado);

                if (cuentaId > 0)
                {
                    idCuenta = cuentaId;
                    MessageBox.Show(Properties.Resources.msgCodigoCorrecto, "", MessageBoxButton.OK, MessageBoxImage.Information);
                    ActualizarVentanaRestablecerContrasena();
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgCodigoIncorrecto, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.globalErrorServidor, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarVentanaCodigoVerificacion()
        {
            spCorreo.Visibility = Visibility.Collapsed;
            spCodigo.Visibility = Visibility.Visible;
        }

        private void ActualizarVentanaRestablecerContrasena()
        {
            spCodigo.Visibility = Visibility.Collapsed;
            spNuevaContraseña.Visibility = Visibility.Visible;
        }

        private void btnRestablecerContrasena_Click(object sender, RoutedEventArgs e)
        {
            string nuevaContrasena = pbNuevaContrasena.Password;
            string confirmarContrasena = pbConfirmarContrasena.Password;

            if (string.IsNullOrEmpty(nuevaContrasena) || string.IsNullOrEmpty(confirmarContrasena))
            {
                MessageBox.Show(Properties.Resources.camposVaciosLogin, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Utilidad.ValidarContrasena(nuevaContrasena))
            {
                MessageBox.Show(Properties.Resources.lbCaracteristicasContraseñaValida, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (nuevaContrasena != confirmarContrasena)
            {
                MessageBox.Show(Properties.Resources.globalContraseñasNoCoinciden, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _servicioCuenta.ModificarContraseña(idCuenta, Utilidad.HashearConSha256(nuevaContrasena));
                MessageBox.Show(Properties.Resources.msgContraseñaRestablecida, "", MessageBoxButton.OK, MessageBoxImage.Information);
                IniciarSesion ventanaIniciarSesion = new IniciarSesion();
                ventanaIniciarSesion.Show();
                this.Close();
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.globalErrorServidor, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            ventanaIniciarSesion.Show();
            this.Close();
        }
    }
}
