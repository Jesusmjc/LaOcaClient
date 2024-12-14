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
using System.Windows.Shapes;

namespace LaOcaClient
{
    public partial class CambiarContraseña : Window
    {
        private readonly IServicioCuenta _servicioCuenta;
        private Cuenta _cuenta;

        public CambiarContraseña(int idCuenta)
        {
            InitializeComponent();
            _servicioCuenta = new ServicioCuentaClient();
            CargarDatosCuenta(idCuenta);
        }

        private void CargarDatosCuenta(int idCuenta)
        {
            try
            {
                _cuenta = _servicioCuenta.ObtenerCuentaPorId(idCuenta);
                if (_cuenta == null)
                {
                    MessageBox.Show(Properties.Resources.msgCuentaNoEncontrada, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.globalErrorServidor, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void BtnActualizarContraseña(object sender, RoutedEventArgs e)
        {
            string contraseñaActual = pbContraseñaActual.Password;
            string nuevaContraseña = pbNuevaContraseña.Password;
            string confirmarNuevaContraseña = pbConfirmarNuevaContraseña.Password;

            if (string.IsNullOrEmpty(contraseñaActual) || string.IsNullOrEmpty(nuevaContraseña) || string.IsNullOrEmpty(confirmarNuevaContraseña))
            {
                MessageBox.Show(Properties.Resources.camposVaciosLogin, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Utilidad.ValidarContrasena(nuevaContraseña))
            {
                MessageBox.Show(Properties.Resources.lbCaracteristicasContraseñaValida, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (nuevaContraseña != confirmarNuevaContraseña)
            {
                MessageBox.Show(Properties.Resources.globalContraseñasNoCoinciden, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                bool esContraseñaCorrecta = _servicioCuenta.VerificarContraseñaActual(_cuenta.IdCuenta, Utilidad.HashearConSha256(contraseñaActual));
                if (esContraseñaCorrecta)
                {
                    _cuenta.Contrasena = Utilidad.HashearConSha256(nuevaContraseña);
                    _servicioCuenta.ModificarCuenta(_cuenta);
                    MessageBox.Show(Properties.Resources.msgContraseñaRestablecida, "", MessageBoxButton.OK, MessageBoxImage.Information);
                    CerrarSesion();
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgContraseñaActualIncorrecta, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void CerrarSesion()
        {
            LaOcaService.ServicioJugadoresEnLineaClient clienteJugadoresEnLinea = new LaOcaService.ServicioJugadoresEnLineaClient();

            try
            {
                clienteJugadoresEnLinea.EliminarJugadorDesconectado(SingletonJugador.Instance.Jugador);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(Properties.Resources.msgTimeoutEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CommunicationException)
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IniciarSesion ventanaIniciarSesion = new IniciarSesion();
            this.Close();
            ventanaIniciarSesion.ShowDialog();
        }

        private void BtnVolver(object sender, RoutedEventArgs e)
        {
            int idCuenta = SingletonJugador.Instance.Jugador.IdCuenta;
            int idJugador = SingletonJugador.Instance.Jugador.IdJugador;
            CrearCuenta ventanaCrearCuenta = new CrearCuenta(ModoCuenta.Modificar, idCuenta, idJugador);
            ventanaCrearCuenta.ActualizarVentanaModificar(ModoCuenta.Modificar);
            ventanaCrearCuenta.Show();
            this.Close();
        }
    }
}
