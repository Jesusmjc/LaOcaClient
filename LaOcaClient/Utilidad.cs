using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace LaOcaClient
{
    public static class Utilidad
    {
        public static string HashearConSha256(string entrada)
        {
            StringBuilder sb = new StringBuilder();

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytesEntrada = Encoding.UTF8.GetBytes(entrada);
                byte[] bytesHash = sha256.ComputeHash(bytesEntrada);

                for (int i = 0; i < bytesHash.Length; i++)
                {
                    sb.Append(bytesHash[i].ToString("x2"));
                }
            }

            return sb.ToString();
        }

        public static bool ValidarNombreJugador(string nombreJugador)
        {
            bool esNombreJugadorValido = false;
            string patronNombreJugador = @"^(?!.*[._-]{2})[a-zA-Z0-9](?:[a-zA-Z0-9._-]*[a-zA-Z0-9]){5,11}$";

            if (Regex.IsMatch(nombreJugador, patronNombreJugador))
            {
                esNombreJugadorValido = true;
            }

            return esNombreJugadorValido;
        }

        public static bool ValidarCorreoElectronico(string correoElectronico)
        {
            bool esCorreoValido = false;
            if (Regex.IsMatch(correoElectronico, @"^[a-zA-Z0-9](?:[a-zA-Z0-9._\-]*[a-zA-Z0-9])?@[a-zA-Z0-9](?:[a-zA-Z0-9\-]*[a-zA-Z0-9])?(?:\.[a-zA-Z]{2,})+$"))
            {
                esCorreoValido = true;
            }

            return esCorreoValido;
        }

        public static bool ValidarContrasena(string contrasena)
        {
            bool esContrasenaValida = false;
            if (Regex.IsMatch(contrasena, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[$@$!%*?&#.$($)\\-_])[A-Za-z\\d$@$!%*?&#.$($)\\-_]{8,16}$"))
            {
                esContrasenaValida = true;
            }

            return esContrasenaValida;
        }

        public static void ManejarCommunicationException(ICommunicationObject cliente)
        {
            MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);

            try
            {
                if (cliente.State == CommunicationState.Faulted)
                {
                    cliente.Abort();
                }
                else
                {
                    cliente.Close();
                }
            }
            catch
            {
                cliente.Abort();
            }

            throw new RegresarAInicioSesionException(Properties.Resources.msgComunnicationEx);
        }
    }

    public static class EstadoAmistad
    {
        public const string SOLICITUD = "Solicitud";
        public const string AMIGOS = "Amigos";
        public const string BLOQUEO = "Bloqueo";
        public const string RECHAZADA = "Rechazada";
    }

    public static class FotoPerfilUtils
    {
        private static readonly Dictionary<int, string> _FotoPerfilMap = new Dictionary<int, string>
        {
            {1, "pack://application:,,,/LaOcaClient;component/Recursos/OcaDeportista.jpg"},
            {2, "pack://application:,,,/LaOcaClient;component/Recursos/OcaDesastrosa.jpg"},
            {3, "pack://application:,,,/LaOcaClient;component/Recursos/OcaIngeniera.jpg"},
            {4, "pack://application:,,,/LaOcaClient;component/Recursos/OcaProgramadora.jpg"},
            {5, "pack://application:,,,/LaOcaClient;component/Recursos/OcaRockstar.jpg"},
            {6, "pack://application:,,,/LaOcaClient;component/Recursos/OcaUniversitaria.jpg"}
        };

        /// <summary>
        /// Obtiene la ruta de la foto de perfil para un IdFotoPerfil dado.
        /// </summary>
        /// <param name="idFotoPerfil">El ID de la foto de perfil.</param>
        /// <returns>La ruta de la imagen correspondiente o una ruta predeterminada si no existe el ID.</returns>
        public static string ObtenerRutaFotoPerfil(int idFotoPerfil)
        {
            return _FotoPerfilMap.ContainsKey(idFotoPerfil)
                ? _FotoPerfilMap[idFotoPerfil]
                : "../Recursos/icono_usuario.png";
        }
    }
}