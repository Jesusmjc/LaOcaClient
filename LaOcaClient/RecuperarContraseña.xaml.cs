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
        private IServicioCuenta _servicioCuenta;
        private IServicioCodigo _servicioCodigo;
        private IniciarSesion _iniciarSesion = new IniciarSesion();

        public RecuperarContraseña()
        {
            InitializeComponent();
            _servicioCuenta = new ServicioCuentaClient();
            _servicioCodigo = new ServicioCodigoClient();
        }

        private void BtnEnviarCodigoRestablecimiento(object sender, RoutedEventArgs e)
        {
            string correo = tbxCorreoElectronico.Text;

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
                _servicioCodigo.EnviarCodigoVerificacion(correo);
                MessageBox.Show(Properties.Resources.msgCodigoEnviado, "", MessageBoxButton.OK, MessageBoxImage.Information);
                ActualizarVentanaCodigoVerificacion();
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

        private int _idCuenta;

        private void BtnVerificarCodigo(object sender, RoutedEventArgs e)
        {
            string correo = tbxCorreoElectronico.Text;
            string codigoIngresado = tbxCodigoRestablecimiento.Text;

            if (string.IsNullOrEmpty(codigoIngresado))
            {
                MessageBox.Show(Properties.Resources.msgCodigoNoIngresado, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int cuentaId = _servicioCodigo.VerificarCodigoRecuperarContraseña(correo, codigoIngresado);

                if (cuentaId > 0)
                {
                    _idCuenta = cuentaId;
                    MessageBox.Show(Properties.Resources.msgCodigoCorrecto, "", MessageBoxButton.OK, MessageBoxImage.Information);
                    ActualizarVentanaRestablecerContraseña();
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgCodigoIncorrecto, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ActualizarVentanaCodigoVerificacion()
        {
            spCorreo.Visibility = Visibility.Collapsed;
            spCodigo.Visibility = Visibility.Visible;
        }

        private void ActualizarVentanaRestablecerContraseña()
        {
            spCodigo.Visibility = Visibility.Collapsed;
            spContraseña.Visibility = Visibility.Visible;
        }

        private void BtnRestablecerContrasena(object sender, RoutedEventArgs e)
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
                _servicioCuenta.ModificarContraseña(_idCuenta, Utilidad.HashearConSha256(nuevaContrasena));
                MessageBox.Show(Properties.Resources.msgContraseñaRestablecida, "", MessageBoxButton.OK, MessageBoxImage.Information);
                IniciarSesion ventanaIniciarSesion = new IniciarSesion();
                ventanaIniciarSesion.Show();
                this.Close();
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

        private void BtnVolver(object sender, RoutedEventArgs e)
        {
            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            ventanaIniciarSesion.Show();
            this.Close();
        }
    }
}