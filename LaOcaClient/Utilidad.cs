using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LaOcaClient
{
    internal class Utilidad
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
            if (nombreJugador.Length >= 6)
            {
                esNombreJugadorValido = true;
            }

            return esNombreJugadorValido;
        }

        public static bool ValidarCorreoElectronico(string correoElectronico)
        {
            bool esCorreoValido = false;
            if (Regex.IsMatch(correoElectronico, "^[a-zA-Z0-9\\-_]{5,20}@(gmail|outlook|hotmail)\\.com$"))
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
    }
}
