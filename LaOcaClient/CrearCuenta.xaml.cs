using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
using System.IO;
using LaOcaClient.LaOcaService;
using System.ServiceModel;
using System.Windows.Threading;
using System.Resources;
using System.Globalization;
using System.ServiceModel.Security;
using System.Security.Permissions;

namespace LaOcaClient
{
    public enum ModoCuenta
    {
        Crear,
        Modificar
    }

    public partial class CrearCuenta : Window
    {
        private readonly IServicioCuenta _servicioCuenta;
        private readonly IServicioCuenta _servicioJugador;
        private readonly IServicioCuenta _servicioAspecto; 
        private string _imagenPerfilSeleccionada;
        private DispatcherTimer _timer;
        private int _tiempoRestante;
        private readonly ModoCuenta _modo;

        public CrearCuenta(ModoCuenta modo)
        {
            InitializeComponent();
            _servicioCuenta = new ServicioCuentaClient();
            _servicioJugador = new ServicioCuentaClient();
            _servicioAspecto = new ServicioCuentaClient();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _modo = modo;
            AjustarInterfazSegunModo();
            ResourceManager _resourceManager = new ResourceManager("LaOcaClient.Resources", typeof(CrearCuenta).Assembly);
            _servicioCuenta.SincronizarAspectos(referenciaToIdMap);

        }

        public CrearCuenta(ModoCuenta modo, int idCuenta, int idJugador) : this(modo)
        {
            if (modo == ModoCuenta.Modificar)
            {
                this.Title = Properties.Resources.lbModificarCuenta;
                globalCrearCuenta.Content = Properties.Resources.lbModificarCuenta;
                btnSiguienteModificarCuenta.Content = Properties.Resources.btnGuardarCambios;
                CargarDatosJugador(idCuenta, idJugador);
            }
        }

        private void AjustarInterfazSegunModo()
        {
            if (_modo == ModoCuenta.Crear)
            {
                btnSiguienteCrearCuenta.Visibility = Visibility.Visible;
                btnSiguienteModificarCuenta.Visibility = Visibility.Collapsed;
            }
            else if (_modo == ModoCuenta.Modificar)
            {
                btnSiguienteCrearCuenta.Visibility = Visibility.Collapsed;
                btnSiguienteModificarCuenta.Visibility = Visibility.Visible;

                tbContraseña.Visibility = Visibility.Collapsed;
                tbConfirmarContraseña.Visibility = Visibility.Collapsed;
                btnCambiarContraseña.Visibility = Visibility.Visible;
                globalCorreo.Margin = new Thickness(78, 440, 0, 0);
                tbCorreo.Margin = new Thickness(78, 468, 0, 0);
                btnCambiarContraseña.Margin = new Thickness(78, 353, 0, 0);
            }
        }

        private void BtnSiguienteCrear_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormularioCrear())
            {
                return;
            }

            int idFotoPerfil;

            try
            {
                idFotoPerfil = ObtenerIdAspectoPorReferencia(_imagenPerfilSeleccionada);
            }
            catch (ArgumentException)
            {
                MessageBox.Show(Properties.Resources.msgRefImagenInvalida, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var jugador = new Jugador
            {
                NombreUsuario = tbNombreUsuario.Text,
                IdFotoPerfil = idFotoPerfil
            };

            try
            {
                if (_servicioCuenta.NombreUsuarioExisteCrear(jugador.NombreUsuario))
                {
                    MessageBox.Show(Properties.Resources.msgNombreUsuarioExiste, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _servicioCuenta.EnviarCodigoVerificacion(tbCorreo.Text);
                MessageBox.Show(Properties.Resources.msgCodigoEnviadoCrearCuenta, "", MessageBoxButton.OK, MessageBoxImage.Information);
                ActualizarVentanaCrearCuenta();
                IniciarTemporizador();
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

        private void BtnSiguienteModificar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormularioModificar())
            {
                return;
            }

            var cuenta = new Cuenta
            {
                IdCuenta = SingletonJugador.Instance.Jugador.IdCuenta,
                CorreoElectronico = tbCorreo.Text,
                Contrasena = tbContraseña.Password
            };

            int idFotoPerfil;
            try
            {
                idFotoPerfil = ObtenerIdAspectoPorReferencia(_imagenPerfilSeleccionada);
            }
            catch (ArgumentException)
            {
                MessageBox.Show(Properties.Resources.msgRefImagenInvalida, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var jugador = new Jugador
            {
                IdJugador = SingletonJugador.Instance.Jugador.IdJugador,
                NombreUsuario = tbNombreUsuario.Text,
                IdFotoPerfil = idFotoPerfil
            };

            try
            {
                if (_servicioCuenta.NombreUsuarioExisteModificar(jugador.NombreUsuario, jugador.IdJugador))
                {
                    MessageBox.Show(Properties.Resources.msgNombreUsuarioExiste, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _servicioCuenta.ModificarCuenta(cuenta);
                _servicioJugador.ModificarJugador(jugador);
                MessageBox.Show(Properties.Resources.msgCuentaModificada, "", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarDatosJugador(SingletonJugador.Instance.Jugador.IdCuenta, SingletonJugador.Instance.Jugador.IdJugador);
                MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
                ventanaMenuPrincipal.Show();
                this.Close();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(Properties.Resources.msgErrorModificarJugador, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
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

        private bool ValidarFormularioCrear()
        {
            return ValidarCamposComunes() && ValidarContrasena() && ValidarCorreo() && ValidarImagenPerfil();
        }

        private bool ValidarFormularioModificar()
        {
            return ValidarCamposComunes() && ValidarImagenPerfil();
        }

        private bool ValidarCamposComunes()
        {
            string nombreUsuario = tbNombreUsuario.Text;

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                MessageBox.Show(Properties.Resources.camposVaciosLogin, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Utilidad.ValidarNombreJugador(nombreUsuario))
            {
                MessageBox.Show(Properties.Resources.msgCaracteristicasNombreUsuario, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool ValidarContrasena()
        {
            string contrasena = tbContraseña.Password;
            string confirmarContrasena = tbConfirmarContraseña.Password;

            if (string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(confirmarContrasena))
            {
                MessageBox.Show(Properties.Resources.camposVaciosLogin, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Utilidad.ValidarContrasena(contrasena))
            {
                MessageBox.Show(Properties.Resources.lbCaracteristicasContraseñaValida, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (contrasena != confirmarContrasena)
            {
                MessageBox.Show(Properties.Resources.globalContraseñasNoCoinciden, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool ValidarCorreo()
        {
            string correo = tbCorreo.Text;

            if (string.IsNullOrWhiteSpace(correo))
            {
                MessageBox.Show(Properties.Resources.camposVaciosLogin, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Utilidad.ValidarCorreoElectronico(correo))
            {
                MessageBox.Show(Properties.Resources.lbCaracteristicasCorreoValido, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (_servicioCuenta.CorreoExiste(correo))
            {
                MessageBox.Show(Properties.Resources.msgCorreoYaExiste, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool ValidarImagenPerfil()
        {
            if (string.IsNullOrEmpty(_imagenPerfilSeleccionada))
            {
                MessageBox.Show(Properties.Resources.msgImagenNoSeleccionada, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void BtnVerificarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreo.Text;
            string codigoIngresado = tbCodigoVerificacion.Text;

            try
            {
                bool esCodigoCorrecto = _servicioCuenta.VerificarCodigoCrearCuenta(correo, codigoIngresado);
                if (esCodigoCorrecto)
                {
                    MessageBox.Show(Properties.Resources.msgCodigoCorrecto, "", MessageBoxButton.OK, MessageBoxImage.Information);

                    var cuenta = new Cuenta
                    {
                        CorreoElectronico = tbCorreo.Text,
                        Contrasena = Utilidad.HashearConSha256(tbContraseña.Password)
                    };

                    var jugador = new Jugador
                    {
                        NombreUsuario = tbNombreUsuario.Text,
                        IdFotoPerfil = ObtenerIdAspectoPorReferencia(_imagenPerfilSeleccionada)
                    };

                    try
                    {
                        _servicioCuenta.CrearCuenta(cuenta, jugador, _imagenPerfilSeleccionada);
                        MessageBox.Show(Properties.Resources.msgCuentaCreada, "", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void BtnReenviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreo.Text;

            try
            {
                _servicioCuenta.EnviarCodigoVerificacion(correo);
                MessageBox.Show(Properties.Resources.msgCodigoReenviado, "", MessageBoxButton.OK, MessageBoxImage.Information);
                IniciarTemporizador();
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            _tiempoRestante--;
            if (_tiempoRestante <= 0)
            {
                _timer.Stop();
                btnReenviarCodigo.IsEnabled = true;
                lbReenviarCodigo.Content = Properties.Resources.btnReenviarCodigo;
            }
            else
            {
                lbReenviarCodigo.Content = Properties.Resources.btnReenviarCodigo + $" ({_tiempoRestante}s)";
            }
        }

        private void IniciarTemporizador()
        {
            _tiempoRestante = 60;
            btnReenviarCodigo.IsEnabled = false;
            _timer.Start();
        }

        private static readonly Dictionary<string, int> referenciaToIdMap = new Dictionary<string, int>
        {
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaDeportista.jpg", 1 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaDesastrosa.jpg", 2 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaIngeniera.jpg", 3 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaProgramadora.jpg", 4 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaRockstar.jpg", 5 },
            { "pack://application:,,,/LaOcaClient;component/Recursos/OcaUniversitaria.jpg", 6 }
        };

        public void SincronizarAspectosConServidor()
        {
            _servicioCuenta.SincronizarAspectos(referenciaToIdMap);
        }

        private static int ObtenerIdAspectoPorReferencia(string referencia)
        {
            if (referenciaToIdMap.TryGetValue(referencia, out int id))
            {
                return id;
            }
            else
            {
                throw new ArgumentException(Properties.Resources.msgRefImagenInvalida);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            CancelarCrearModificar(_modo);
        }

        private void CancelarCrearModificar(ModoCuenta modo)
        {
            if (modo == ModoCuenta.Crear && (MessageBoxResult.Yes == MessageBox.Show(Properties.Resources.msgCancelarCreacionCuenta, Properties.Resources.tituloCancelarCrearCuenta, MessageBoxButton.YesNo, MessageBoxImage.Warning)))
            {
                IniciarSesion ventanaIniciarSesion = new IniciarSesion();
                ventanaIniciarSesion.Show();
                this.Close();
            }
            else if (modo == ModoCuenta.Modificar && (MessageBoxResult.Yes == MessageBox.Show(Properties.Resources.msgCancelarModificacionCuenta, Properties.Resources.tituloCancelarModificacionCuenta, MessageBoxButton.YesNo, MessageBoxImage.Warning)))
            {
                MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
                ventanaMenuPrincipal.Show();
                this.Close();
            }
        }

        private void ImagenPerfil_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image imagenSeleccionada = sender as Image;
            _imagenPerfilSeleccionada = imagenSeleccionada.Source.ToString();

            foreach (var child in wpImagenesPerfil.Children)
            {
                if (child is Image img)
                {
                    img.Opacity = img == imagenSeleccionada ? 1.0 : 0.5;
                }
            }
        }

        private void ActualizarVentanaCrearCuenta()
        {
            this.Height = 400;
            this.Width = 600;
            tbCodigoVerificacion.Visibility = Visibility.Visible;
            btnVerificarCodigo.Visibility = Visibility.Visible;
            lbIngresarCodigo.Visibility = Visibility.Visible;
            btnReenviarCodigo.Visibility = Visibility.Visible;
            lbReenviarCodigo.Visibility = Visibility.Visible;
            btnVolverAtras.Visibility = Visibility.Visible;

            globalCrearCuenta.Visibility = Visibility.Collapsed;
            globalNombreUsuario.Visibility = Visibility.Collapsed;
            tbNombreUsuario.Visibility = Visibility.Collapsed;
            globalContraseña.Visibility = Visibility.Collapsed;
            tbContraseña.Visibility = Visibility.Collapsed;
            globalConfirmarContraseña.Visibility = Visibility.Collapsed;
            tbConfirmarContraseña.Visibility = Visibility.Collapsed;
            globalCorreo.Visibility = Visibility.Collapsed;
            tbCorreo.Visibility = Visibility.Collapsed;
            btnSiguienteCrearCuenta.Visibility = Visibility.Collapsed;
            btnCancelar.Visibility = Visibility.Collapsed;
            lbSeleccionarImagenPerfil.Visibility = Visibility.Collapsed;
            wpImagenesPerfil.Visibility = Visibility.Collapsed;
        }

        private void BtnVolverAtras_Click(object sender, RoutedEventArgs e)
        {
            this.Height = 600;
            this.Width = 900;
            tbCodigoVerificacion.Visibility = Visibility.Collapsed;
            btnVerificarCodigo.Visibility = Visibility.Collapsed;
            lbIngresarCodigo.Visibility = Visibility.Collapsed;
            btnReenviarCodigo.Visibility = Visibility.Collapsed;
            lbReenviarCodigo.Visibility = Visibility.Collapsed;
            btnVolverAtras.Visibility = Visibility.Collapsed;

            globalCrearCuenta.Visibility = Visibility.Visible;
            globalNombreUsuario.Visibility = Visibility.Visible;
            tbNombreUsuario.Visibility = Visibility.Visible;
            globalContraseña.Visibility = Visibility.Visible;
            tbContraseña.Visibility = Visibility.Visible;
            globalConfirmarContraseña.Visibility = Visibility.Visible;
            tbConfirmarContraseña.Visibility = Visibility.Visible;
            globalCorreo.Visibility = Visibility.Visible;
            tbCorreo.Visibility = Visibility.Visible;
            btnSiguienteCrearCuenta.Visibility = Visibility.Visible;
            btnCancelar.Visibility = Visibility.Visible;
            lbSeleccionarImagenPerfil.Visibility = Visibility.Visible;
            wpImagenesPerfil.Visibility = Visibility.Visible;
        }

        private void CargarDatosJugador(int idCuenta, int idJugador)
        {
            try
            {
                var cuenta = _servicioCuenta.ObtenerCuentaPorId(idCuenta);
                if (cuenta == null)
                {
                    MessageBox.Show(Properties.Resources.msgCuentaNoEncontrada, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var jugador = _servicioJugador.ObtenerJugadorPorId(idJugador);
                if (jugador == null)
                {
                    MessageBox.Show(Properties.Resources.msgJugadorNoEncontrado, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                tbNombreUsuario.Text = jugador.NombreUsuario;
                tbCorreo.Text = cuenta.CorreoElectronico;
                tbContraseña.Password = cuenta.Contrasena;
                tbConfirmarContraseña.Password = cuenta.Contrasena;

                var aspecto = _servicioAspecto.ObtenerAspectoPorId(jugador.IdFotoPerfil);
                if (aspecto != null)
                {
                    _imagenPerfilSeleccionada = aspecto.Referencia;
                    foreach (var child in wpImagenesPerfil.Children)
                    {
                        if (child is Image img && img.Source.ToString() == _imagenPerfilSeleccionada)
                        {
                            img.Opacity = 1.0;
                        }
                        else if (child is Image img2)
                        {
                            img2.Opacity = 0.5;
                        }
                    }
                }
                else
                {
                    MessageBox.Show(Properties.Resources.msgImagenPerfilNoEncontrada, Properties.Resources.globalErrorValidacion, MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void ActualizarVentanaModificar(ModoCuenta modo)
        {
            if (modo == ModoCuenta.Modificar)
            {
                globalContraseña.Visibility = Visibility.Collapsed;
                tbContraseña.Visibility = Visibility.Collapsed;
                globalConfirmarContraseña.Visibility = Visibility.Collapsed;
                tbConfirmarContraseña.Visibility = Visibility.Collapsed;
                btnCambiarContraseña.Visibility = Visibility.Visible;
                tbCorreo.IsEnabled = false;
            }
        }

        private void BtnCambiarContraseña_Click(object sender, RoutedEventArgs e)
        {
            CambiarContraseña ventanaCambiarContraseña = new CambiarContraseña(SingletonJugador.Instance.Jugador.IdCuenta);
            ventanaCambiarContraseña.Show();
            this.Close();
        }
    }
}