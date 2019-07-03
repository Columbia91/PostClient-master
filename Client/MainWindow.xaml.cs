using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void SendMessageFromSocket(object port)
        {
            try
            {
                // Буфер для входящих данных
                byte[] bytes = new byte[1024];

                // Соединяемся с удаленным устройством
                // Устанавливаем удаленную точку для сокета
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, (int)port);

                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Соединяем сокет с удаленной точкой
                sender.Connect(ipEndPoint);
                string request = "";
                Dispatcher.Invoke(() =>
                {
                    request = indexTextBox.Text;
                    indexTextBox.Text = string.Empty;
                });

                byte[] msg = Encoding.UTF8.GetBytes(request);
                
                // Отправляем данные через сокет
                int bytesSent = sender.Send(msg);

                // Получаем ответ от сервера
                int bytesRec = sender.Receive(bytes);
                string str = Encoding.UTF8.GetString(bytes, 0, bytesRec);
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i] == '.')
                        str = str.Insert(i+1, ", ");
                }

                Dispatcher.Invoke(() =>
                {
                    streetTextBox.Text = str;
                });

                // Освобождаем сокет
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(indexTextBox.Text) && !string.IsNullOrWhiteSpace(indexTextBox.Text))
            {
                try
                {
                    WaitCallback workItem = new WaitCallback(SendMessageFromSocket);
                    ThreadPool.QueueUserWorkItem(workItem, 11000);
                }
                catch (ArgumentNullException ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void IndexTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter &&
                !string.IsNullOrEmpty(indexTextBox.Text) && !string.IsNullOrWhiteSpace(indexTextBox.Text))
            {
                FindButton_Click(this, null);
            }
        }
    }
}
