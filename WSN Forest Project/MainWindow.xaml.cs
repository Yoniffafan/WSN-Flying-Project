using Microsoft.Maps.MapControl.WPF;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ULTRON_2016;
using WebEye.Controls.Wpf;

namespace WSN_Forest_Project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Koneksi konekin = new Koneksi();

        int interval = 500;
        KoleksiData.Sensor mySensorLog = new KoleksiData.Sensor();

        DataTable table = new DataTable("Komurindo2016");
        DataTable tableToSave = new DataTable("Komurindo2016");

        ObservableDataSource<Point> sourceYaw = new ObservableDataSource<Point>();
        ObservableDataSource<Point> sourcePitch = new ObservableDataSource<Point>();
        ObservableDataSource<Point> sourceRoll = new ObservableDataSource<Point>();
        ObservableDataSource<Point> sourceElevasi = new ObservableDataSource<Point>();
        ObservableDataSource<Point> sourceTinggi = new ObservableDataSource<Point>();

        ObservableDataSource<Point> dataYaw = new ObservableDataSource<Point>();
        ObservableDataSource<Point> dataPitch = new ObservableDataSource<Point>();
        ObservableDataSource<Point> dataRoll = new ObservableDataSource<Point>();
        ObservableDataSource<Point> dataElevasi = new ObservableDataSource<Point>();
        ObservableDataSource<Point> dataTinggi = new ObservableDataSource<Point>();

        private List<double> tinggiLog = new List<double>();
        private List<double> tekananLog = new List<double>();
        private List<double> kecepatanLog = new List<double>();
        private List<double> suhuLog = new List<double>();
        private List<double> elevasiLog = new List<double>();
        private List<float> yawLog = new List<float>();
        private List<float> pitchLog = new List<float>();
        private List<float> rollLog = new List<float>();

        public List<string> mainDb = new List<string>();

        public Model3D CurrentModel;

        AxisAngleRotation3D psiAxis = new AxisAngleRotation3D();
        AxisAngleRotation3D thetaAxis = new AxisAngleRotation3D();
        AxisAngleRotation3D phiAxis = new AxisAngleRotation3D();

        public int previousI, i;
        public Location location;
        public Location previousLocation;
        public Boolean dataPertama = true;
        string[] data;

        double yaw, pitch, roll, q0, q1, q2, q3;
        //private bool GPSstatus = true;

        private bool captureFlag = false;
        private bool kirimFlag = false;

        public int counterPushPin = 0;
        public double goalLat = 0;
        public double goalLong = 0;

        public string logName;

        //MapPolyline myPolyline = new MapPolyline();

        string logFileName = String.Empty;
        //private string Latlat = "0";
        //private string Longlon = "0";
        Microsoft.Maps.MapControl.WPF.Location koorLokasi = new Microsoft.Maps.MapControl.WPF.Location(0, 0);

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        int intervalCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeComboBox();
        }

        #region button
        private void Buttonexit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void yprButton_Click(object sender, RoutedEventArgs e)
        {
            yprButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF1A60AB")));
            elevasiButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF3A3A3A")));
            ketinggianButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF3A3A3A")));

            grafikLabel.Content = "Yaw Pitch Roll";

            AccPlot.Visibility = Visibility.Visible;
            GyroPlot.Visibility = Visibility.Hidden;
            TinggiPlot.Visibility = Visibility.Hidden;
        }

        private void elevasiButton_Click(object sender, RoutedEventArgs e)
        {
            yprButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF3A3A3A")));
            elevasiButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF1A60AB")));
            ketinggianButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF3A3A3A")));

            grafikLabel.Content = "Elevasi Sudut";

            AccPlot.Visibility = Visibility.Hidden;
            GyroPlot.Visibility = Visibility.Visible;
            TinggiPlot.Visibility = Visibility.Hidden;
        }

        private void ketinggianButton_Click(object sender, RoutedEventArgs e)
        {
            yprButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF3A3A3A")));
            elevasiButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF3A3A3A")));
            ketinggianButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF1A60AB")));

            grafikLabel.Content = "Ketinggian";

            AccPlot.Visibility = Visibility.Hidden;
            GyroPlot.Visibility = Visibility.Hidden;
            TinggiPlot.Visibility = Visibility.Visible;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            initBaudList();
            initComPortList();
            initDatabitsList();
            initStopbitsList();
            initParityList();
            initHandshakeList();
            initDelay();
        }
        #endregion

        #region init
        private void initBaudList()
        {
            string[] bauds = { "110", "300", "600", "1200", "2400", "4800", "9600", "14400", "19200", "38400", "56000", "57600", "115200" };
            foreach (string baud in bauds)
            {
                baudCombo.Items.Add(baud);
            }
            baudCombo.SelectedIndex = 11;
        }

        //private void initPortLaunch()
        //{
        //    foreach (String s in SerialPort.GetPortNames())
        //    {
        //        if (s != "")
        //        {
        //            portLauncher.Items.Add("COM" + s.Substring(3));
        //            portLauncher.SelectedIndex = 0;
        //        }
        //        else
        //        {
        //            portLauncher.Items.Add("Unknown");
        //            portLauncher.SelectedIndex = 0;
        //        }
        //    }
        //    if (Komunikasi.Default.PortName != "")
        //    {
        //        portCombo.SelectedIndex = 0;
        //    }
        //}

        private void initComPortList()
        {
            foreach (String s in SerialPort.GetPortNames())
            {
                if (s != "")
                {
                    portCombo.Items.Add("COM" + s.Substring(3));
                    portCombo.SelectedIndex = 0;
                }
                else
                {
                    portCombo.Items.Add("Unknown");
                    portCombo.SelectedIndex = 0;
                }
            }
            if (Komunikasi.Default.PortName != "")
            {
                portCombo.SelectedIndex = 0;
            }
        }

        private void initDatabitsList()
        {
            for (int i = 5; i <= 9; i++)
            {
                databitCombo.Items.Add(i);
            }
            databitCombo.SelectedIndex = 3;
        }

        private void initStopbitsList()
        {
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                if (s != "None")
                {
                    stopbitCombo.Items.Add(s);
                }
            }
            stopbitCombo.SelectedIndex = 0;
        }

        private void initParityList()
        {
            foreach (String s in Enum.GetNames(typeof(Parity)))
            {
                parityCombo.Items.Add(s);
            }
            parityCombo.SelectedIndex = 0;
        }

        private void initHandshakeList()
        {
            foreach (String s in Enum.GetNames(typeof(Handshake)))
            {
                handshakeCombo.Items.Add(s);
            }
            handshakeCombo.SelectedIndex = 0;
        }

        private void initDelay()
        {
            delayCombo.Items.Add("100  ms");
            delayCombo.Items.Add("200  ms");
            delayCombo.Items.Add("500  ms");
            delayCombo.Items.Add("1000 ms");
            delayCombo.SelectedIndex = 0;
        }
        #endregion


        #region camera
        private void InitializeComboBox()
        {
            comboBox.ItemsSource = webCameraControl.GetVideoCaptureDevices();

            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedItem = comboBox.Items[0];
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            startButton.IsEnabled = e.AddedItems.Count > 0;
        }

        private void OnStartButtonClick(object sender, RoutedEventArgs e)
        {
       
                    var cameraId = (WebCameraId)comboBox.SelectedItem;
                    webCameraControl.StartCapture(cameraId);

        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            webCameraControl.StopCapture();
        }

        private void OnImageButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "Bitmap Image|*.bmp" };
            if (dialog.ShowDialog() == true)
            {
                webCameraControl.GetCurrentImage().Save(dialog.FileName);
            }
        }

        #endregion

        #region terminal
        private void terminalText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!e.Key.Equals(Key.Return))
                {
                    //e.Handled = true;
                    //konekin.tulis(e.Key.ToString().ToLower());
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region koneksi
        private void konekButton_Click(object sender, RoutedEventArgs e)
        {
            Komunikasi.Default.BaudRate = Convert.ToInt32(baudCombo.SelectedItem);
            Komunikasi.Default.PortName = Convert.ToString(portCombo.SelectedItem);
            Komunikasi.Default.DataBits = Convert.ToUInt16(databitCombo.SelectedItem);
            Komunikasi.Default.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopbitCombo.SelectedItem.ToString());
            Komunikasi.Default.Handshake = (Handshake)Enum.Parse(typeof(Handshake), handshakeCombo.SelectedItem.ToString());
            Komunikasi.Default.Parity = (Parity)Enum.Parse(typeof(Parity), parityCombo.SelectedItem.ToString());
            Komunikasi.Default.Launcher = Convert.ToString(portLauncher.SelectedItem);
            interval = Convert.ToInt32(Convert.ToString(delayCombo.SelectedItem).Remove(5));

            //if (!Komunikasi.Default.terkoneksi)
            //{
            try
            {
                konekin.NewSerialDataReceived += konekin_NewSerialDataReceived;
                konekin.buka();
                if (Komunikasi.Default.terkoneksi)
                {
                    //btnPutus.Content = "Terhubung ke " + Komunikasi.Default.PortName;


                    stopwatch.Reset();
                    ClearLog();
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            //}

            //gridUtama.Visibility = Visibility.Visible;
            //gridKoneksi.Visibility = Visibility.Hidden;
        }

        void konekin_NewSerialDataReceived(object sender, Koneksi.SerialDataEventArgs e)
        {
            string pesan = e.Data;

            try
            {
                data = pesan.Split(' ');
                /*
                if(data[0] == "1.")
                {
                    terminalText.AppendText("\n 1. Latitude: ");
                }
                if (data[0] == "2.")
                {
                    terminalText.AppendText("\n 1. Longitude: ");
                }
                if (data[0] == "3.")
                {
                    terminalText.AppendText(p;
                } */
                //int jumlData = data.Length;
                //ULTRON yw pitch roll alti temp lati longi speed jarak
                if (data[0] == "u")// && kirimFlag) //&& jumlData == 7 && kirimFlag)
                {
                    try
                    {
                        this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(delegate ()
                        {
                            try
                            {
                                terminalText.AppendText("u" + " " + lblRoll.Content + " " + lblPitch.Content + " " + lblYaw.Content + " " + lblKetinggian.Content + " " + data[5] + " " + data[6] + " " + data[7] + " " + data[8] + Environment.NewLine);
                                // terminalText.AppendText("u" + " " + q0 + " " + q1 + " " + q2 + " " + q3  + Environment.NewLine);

                                terminalText.SelectionStart = terminalText.Text.Length;
                                terminalText.CaretIndex = terminalText.Text.Length;
                                terminalText.ScrollToEnd();

                                //timer();
                                //bar();
                                //map();
                                //putar_Rocket3D();

                                /*  dataLat = float.Parse(mySensorLog.Latitude);
                                  dataLong = float.Parse(mySensorLog.Longitude);
                                  //MyMap.Center.Latitude = dataLat;
                                  lblLatitude.Content = dataLat;

                                  //MyMap.Center.Longitude = dataLong;
                                  lblLongitude.Content = dataLong;
                                  /*   recentLocation = new Location(dataLat, dataLong);
                                     locationLog.Add(recentLocation);

                                     MyMap.Center = recentLocation;
                                     MyMap.Children.Clear();
                                     MyMap.Children.Add(polyline);
                                     MyMap.Children.Add(pushpin);
                                     pushpin.Location = recentLocation;
                                     */

                            }
                            catch (Exception) { }
                        }));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                else
                {
                    terminalText.AppendText(pesan + Environment.NewLine);
                    terminalText.SelectionStart = terminalText.Text.Length;
                    terminalText.CaretIndex = terminalText.Text.Length;
                    terminalText.ScrollToEnd();
                }
            }
            catch (Exception) { }
        }

        void ClearLog()
        {
            sourceYaw.Collection.Clear();
            sourcePitch.Collection.Clear();
            sourceRoll.Collection.Clear();
            sourceElevasi.Collection.Clear();
            sourceTinggi.Collection.Clear();
            dataYaw.Collection.Clear();
            dataPitch.Collection.Clear();
            dataRoll.Collection.Clear();
            dataElevasi.Collection.Clear();
            dataTinggi.Collection.Clear();
            tinggiLog.Clear();
            tekananLog.Clear();
            kecepatanLog.Clear();
            suhuLog.Clear();
            elevasiLog.Clear();
        }
        #endregion
    }
}
