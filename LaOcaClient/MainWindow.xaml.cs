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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LaOcaClient.LaOcaService;

namespace LaOcaClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ProbarConexión(object sender, RoutedEventArgs e)
        {
            LaOcaService.LoginServiceClient cliente = new LaOcaService.LoginServiceClient();
            int resultado = cliente.GetUser("pepito");

            MessageBox.Show("Resultado de la prueba (debería ser 0): " + resultado);
        }
    }
}