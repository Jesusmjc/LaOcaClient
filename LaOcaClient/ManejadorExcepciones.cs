using LaOcaClient.LaOcaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LaOcaClient
{
    internal class ManejadorExcepciones
    {
        public static void ManejarCommunicationException(ICommunicationObject cliente)
        {
            if (VerificarCausaDeError().Equals("El servidor está caído o no responde."))
            {
                MessageBox.Show(Properties.Resources.msgComunnicationEx, Properties.Resources.globalTituloError, MessageBoxButton.OK, MessageBoxImage.Error);
                throw new RegresarAInicioSesionException(Properties.Resources.msgComunnicationEx);
            }
            else
            {
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
            }
        }

        public static string VerificarCausaDeError()
        {
            if (TieneConexionInternet())
            {
                return "El servidor está caído o no responde.";
            }
            else
            {
                return "No hay conexión a Internet.";
            }
        }

        private static bool TieneConexionInternet()
        {
            try
            {
                // Hacer ping al DNS público de Google (8.8.8.8) como prueba de conexión.
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send("8.8.8.8", 3000); // Timeout de 3 segundos
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false; // No se pudo conectar, no hay Internet.
            }
        }
    }

    public class RegresarAInicioSesionException : Exception
    {
        public RegresarAInicioSesionException(string message) : base(message) { }
    }
}