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
        private readonly ResourceManager _resourceManager;

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
            _resourceManager = new ResourceManager("LaOcaClient.Resources", typeof(CrearCuenta).Assembly);

        }

        public CrearCuenta(ModoCuenta modo, int idCuenta, int idJugador) : this(modo)
        {
            if (modo == ModoCuenta.Modificar)
            {
                globalCrearCuenta.Content = "Modificar cuenta";
                btnSiguienteModificarCuenta.Content = "Guardar cambios";
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

        private void btnSiguienteCrear_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormularioCrear())
            {
                return;
            }

            var cuenta = new Cuenta
            {
                CorreoElectronico = tbCorreo.Text,
                Contrasena = Utilidad.HashearConSha256(tbContraseña.Password)
            };

            int idFotoPerfil;

            try
            {
                idFotoPerfil = ObtenerIdAspectoPorReferencia(_imagenPerfilSeleccionada);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("La referencia de la imagen seleccionada no es válida.");
                return;
            }

            var jugador = new Jugador
            {
                NombreUsuario = tbNombreUsuario.Text,
                IdFotoPerfil = idFotoPerfil
            };

            try
            {
                _servicioCuenta.EnviarCodigoVerificacion(tbCorreo.Text);
                MessageBox.Show("Se han guardado los datos de tu cuenta. Por favor revisa el código de verificación que se envió a tu correo electrónico.");
                ActualizarVentanaCrearCuenta();
                IniciarTemporizador();
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"Error de comunicación al enviar el código de verificación: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"El envío del código de verificación ha superado el tiempo de espera: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al enviar el código de verificación: {ex.Message}");
            }
        }

        private void btnSiguienteModificar_Click(object sender, RoutedEventArgs e)
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
                MessageBox.Show("La referencia de la imagen seleccionada no es válida.");
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
                _servicioCuenta.ModificarCuenta(cuenta);
                _servicioJugador.ModificarJugador(jugador);
                MessageBox.Show("Cuenta modificada exitosamente.");
                CargarDatosJugador(SingletonJugador.Instance.Jugador.IdCuenta, SingletonJugador.Instance.Jugador.IdJugador);
                MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
                ventanaMenuPrincipal.Show();
                this.Close();
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"Error de comunicación al modificar la cuenta: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"La modificación de la cuenta ha superado el tiempo de espera: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al modificar la cuenta: {ex.Message}");
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
                MessageBox.Show("Todos los campos son obligatorios.");
                return false;
            }

            if (!Utilidad.ValidarNombreJugador(nombreUsuario))
            {
                MessageBox.Show("El nombre de usuario debe tener al menos 6 caracteres.");
                return false;
            }

            if (_servicioCuenta.NombreUsuarioExiste(nombreUsuario) &&
               (_modo == ModoCuenta.Crear || nombreUsuario != SingletonJugador.Instance.Jugador.NombreUsuario))
            {
                MessageBox.Show("El nombre de usuario ya está en uso. Por favor, elija otro nombre.");
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
                MessageBox.Show("Todos los campos son obligatorios.");
                return false;
            }

            if (!Utilidad.ValidarContrasena(contrasena))
            {
                MessageBox.Show("La contraseña no cumple con los requisitos. Debe tener entre 8 y 16 caracteres, incluir al menos una letra mayúscula, una letra minúscula, un número y un carácter especial.");
                return false;
            }

            if (contrasena != confirmarContrasena)
            {
                MessageBox.Show("Las contraseñas no coinciden.");
                return false;
            }

            return true;
        }

        private bool ValidarCorreo()
        {
            string correo = tbCorreo.Text;

            if (string.IsNullOrWhiteSpace(correo))
            {
                MessageBox.Show("Todos los campos son obligatorios.");
                return false;
            }

            if (!Utilidad.ValidarCorreoElectronico(correo))
            {
                MessageBox.Show("El correo electrónico no es válido. Debe ser un correo de gmail, outlook o hotmail.");
                return false;
            }

            if (_servicioCuenta.CorreoExiste(correo))
            {
                MessageBox.Show("El correo electrónico ya está registrado.");
                return false;
            }

            return true;
        }

        private bool ValidarImagenPerfil()
        {
            if (string.IsNullOrEmpty(_imagenPerfilSeleccionada))
            {
                MessageBox.Show("Debe seleccionar una imagen de perfil.");
                return false;
            }

            return true;
        }

        private void btnVerificarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreo.Text;
            string codigoIngresado = tbCodigoVerificacion.Text;

            try
            {
                bool esCodigoCorrecto = _servicioCuenta.VerificarCodigoCrearCuenta(correo, codigoIngresado);
                if (esCodigoCorrecto)
                {
                    MessageBox.Show("Código de verificación correcto. Creando cuenta...");

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
                        MessageBox.Show("Cuenta creada exitosamente.");
                        IniciarSesion ventanaIniciarSesion = new IniciarSesion();
                        ventanaIniciarSesion.Show();
                        this.Close();
                    }
                    catch (CommunicationException ex)
                    {
                        MessageBox.Show($"Error de comunicación al crear la cuenta: {ex.Message}");
                    }
                    catch (TimeoutException ex)
                    {
                        MessageBox.Show($"La creación de la cuenta ha superado el tiempo de espera: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error inesperado al crear la cuenta: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Código de verificación incorrecto.");
                }
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"{Properties.Resources.globalSala} {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"La verificación del código ha superado el tiempo de espera: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al verificar el código: {ex.Message}");
            }
        }

        private void btnReenviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            string correo = tbCorreo.Text;

            try
            {
                _servicioCuenta.EnviarCodigoVerificacion(correo);
                MessageBox.Show("Se ha reenviado el código de verificación a tu correo electrónico.");
                IniciarTemporizador();
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"Error de comunicación al reenviar el código de verificación: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"El reenvío del código de verificación ha superado el tiempo de espera: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al reenviar el código de verificación: {ex.Message}");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _tiempoRestante--;
            if (_tiempoRestante <= 0)
            {
                _timer.Stop();
                btnReenviarCodigo.IsEnabled = true;
                lbReenviarCodigo.Content = "Reenviar código";
            }
            else
            {
                lbReenviarCodigo.Content = $"Reenviar código ({_tiempoRestante}s)";
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

        private int ObtenerIdAspectoPorReferencia(string referencia)
        {
            if (referenciaToIdMap.TryGetValue(referencia, out int id))
            {
                return id;
            }
            else
            {
                throw new ArgumentException("Referencia no válida", nameof(referencia));
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            CancelarCrearModificar(_modo);
        }

        private void CancelarCrearModificar(ModoCuenta modo)
        {
            if (modo == ModoCuenta.Crear)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("¿Estás seguro de que deseas cancelar la creación de la cuenta?", "Cancelar creación de cuenta", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                {
                    IniciarSesion ventanaIniciarSesion = new IniciarSesion();
                    ventanaIniciarSesion.Show();
                    this.Close();
                }
            }
            else if(modo == ModoCuenta.Modificar)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("¿Estás seguro de que deseas cancelar la modificación de la cuenta?", "Cancelar modificación de cuenta", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                {
                    MenuPrincipal ventanaMenuPrincipal = new MenuPrincipal();
                    ventanaMenuPrincipal.Show();
                    this.Close();
                }
            }
        }

        private void ImagenPerfil_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image imagenSeleccionada = sender as Image;
            _imagenPerfilSeleccionada = imagenSeleccionada.Source.ToString();

            //MessageBox.Show($"Referencia de la imagen seleccionada: {_imagenPerfilSeleccionada}");

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

        private void btnVolverAtras_Click(object sender, RoutedEventArgs e)
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
                    MessageBox.Show("Cuenta no encontrada.");
                    return;
                }

                var jugador = _servicioJugador.ObtenerJugadorPorId(idJugador);
                if (jugador == null)
                {
                    MessageBox.Show("Jugador no encontrado.");
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
                    MessageBox.Show("Imagen de perfil no encontrada.");
                }
            }
            catch (CommunicationException ex)
            {
                MessageBox.Show($"Error de comunicación al cargar los datos del jugador: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"La carga de los datos del jugador ha superado el tiempo de espera: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al cargar los datos del jugador: {ex.Message}");
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
                //globalCorreo.Margin = new Thickness(78, 440, 0, 0);
                //tbCorreo.Margin = new Thickness(78, 468, 0, 0);
                tbCorreo.IsEnabled = false;
                //btnCambiarContraseña.Margin = new Thickness(78, 353, 0, 0);
            }
        }

        private void btnCambiarContraseña_Click(object sender, RoutedEventArgs e)
        {
            CambiarContraseña ventanaCambiarContraseña = new CambiarContraseña(SingletonJugador.Instance.Jugador.IdCuenta);
            ventanaCambiarContraseña.Show();
            this.Close();
        }
    }
}