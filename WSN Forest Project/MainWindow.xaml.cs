using Microsoft.Maps.MapControl.WPF;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using WebEye.Controls.Wpf;
using Microsoft.Research.DynamicDataDisplay;
using System.IO;
using System.Threading;
using ULTRON_2016;
using System.Threading.Tasks;

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

        DataTable table = new DataTable("WSNProject");
        DataTable tableToSave = new DataTable("WSNProject");

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
        //private bool kirimFlag = false;

        public int counterPushPin = 0;
        public double goalLat = 0;
        public double goalLong = 0;
        public string dirgambar;

        public double latlongtomet = 111000;
        public double jarak;

        public string logName = Path.Combine(Environment.CurrentDirectory, DateTime.Now.Day.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString() + " " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ".txt");

        MapPolyline myPolyline = new MapPolyline();

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

            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 0);
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Start();

            sourceYaw.SetXYMapping(p => p);
            sourcePitch.SetXYMapping(p => p);
            sourceRoll.SetXYMapping(p => p);
            sourceElevasi.SetXYMapping(p => p);
            sourceTinggi.SetXYMapping(p => p);

            dataYaw.SetXYMapping(p => p);
            dataPitch.SetXYMapping(p => p);
            dataRoll.SetXYMapping(p => p);
            dataElevasi.SetXYMapping(p => p);
            dataTinggi.SetXYMapping(p => p);

            AccPlot.AddLineGraph(sourceYaw, new Pen(Brushes.Red, 2), new PenDescription("Yaw (deg/s)"));
            AccPlot.AddLineGraph(sourcePitch, new Pen(Brushes.Yellow, 2), new PenDescription("Pitch (deg/s)"));
            AccPlot.AddLineGraph(sourceRoll, new Pen(Brushes.Green, 2), new PenDescription("Roll (deg/s)"));

            GyroPlot.AddLineGraph(sourceElevasi, new Pen(Brushes.Blue, 2), new PenDescription("Elevasi sudut (deg)"));

            TinggiPlot.AddLineGraph(sourceTinggi, new Pen(Brushes.Purple, 2), new PenDescription("Ketinggian (m)"));


            AccPlot.Viewport.FitToView();
            GyroPlot.Viewport.FitToView();
            TinggiPlot.Viewport.FitToView();

            ObjectForScriptingHelper helper = new ObjectForScriptingHelper(this);

            mySensorLog.No = 0;

            InitializeDataTable();

            //var matrix = model.Transform.Value;
            //matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), -90));
            //model.Transform = new MatrixTransform3D(matrix);
            //Viewport3D.Camera.LookAt(new Point3D(150, 175, 190), new Vector3D(-1, -1, -1), 0);
        }

        #region button
        private void Buttonexit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
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

        private void visualButton_Click(object sender, RoutedEventArgs e)
        {
            tabVisual.Visibility = Visibility.Visible;
            tabLog.Visibility = Visibility.Hidden;
            visualButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF100D0D")));
            logButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF444444")));
        }

        private void logButton_Click(object sender, RoutedEventArgs e)
        {
            tabVisual.Visibility = Visibility.Hidden;
            tabLog.Visibility = Visibility.Visible;
            visualButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF444444")));
            logButton.Background = ((Brush)(new BrushConverter().ConvertFrom("#FF100D0D")));
        }

        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //konekin.tulis("s");
                //terminalText.AppendText("terkirim 's'");

                if (!captureFlag)
                {
                    table = new DataTable("WSN Flying Project");
                    InitializeDataTable();
                    stopwatch.Reset();
                    stopwatch.Start();
                    ClearLog();
                    mySensorLog.No = 0;

                    captureFlag = true;
                    btnCapture.Content = "Stop Capture";
                    datagridLog.ItemsSource = table.AsDataView();
                }
                else
                {
                    captureFlag = false;
                    stopwatch.Stop();
                    intervalCount = 0;
                    btnCapture.Content = "Capture";
                    //SaveLog();
                }
            }
            catch
            {
                terminalText.AppendText("gagal kirim 's'");
            }
            //if (!captureFlag)
            //{
            //    table = new DataTable("Komurindo2015");
            //    InitializeDataTable();

            //    stopwatch.Reset();
            //    stopwatch.Start();
            //    ClearLog();
            //    mySensorLog.No = 0;

            //    captureFlag = true;
            //    btnCapture.Style = (Style)this.Resources["button2"];
            //    datagridLog.ItemsSource = table.AsDataView();
            //}
            //else
            //{
            //    captureFlag = false;
            //    stopwatch.Stop();
            //    intervalCount = 0;
            //    btnCapture.Style = (Style)this.Resources["button1"];
            //    //SaveLog();
            //}
        }

        private void btnPutus_Click(object sender, RoutedEventArgs e)
        {

            if (captureFlag) btnCapture_Click(sender, e);

            konekin.NewSerialDataReceived -= konekin_NewSerialDataReceived;
            konekin.komSerial.Close();
            //konekin.serialLaunch.Close();

            //Button_Click_3(sender, e);
            terminalText.Text = "";
            //InitializeEverything();


        }

        private void btnPutus_MouseEnter(object sender, MouseEventArgs e)
        {
            btnPutus.Content = "Putuskan";
        }

        private void btnPutus_MouseLeave(object sender, MouseEventArgs e)
        {
            btnPutus.Content = "Terhubung ke " + Komunikasi.Default.PortName;
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

        private void initPortLaunch()
        {
            foreach (String s in SerialPort.GetPortNames())
            {
                if (s != "")
                {
                    portLauncher.Items.Add("COM" + s.Substring(3));
                    portLauncher.SelectedIndex = 0;
                }
                else
                {
                    portLauncher.Items.Add("Unknown");
                    portLauncher.SelectedIndex = 0;
                }
            }
            if (Komunikasi.Default.PortName != "")
            {
                portCombo.SelectedIndex = 0;
            }
        }

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

        string FormatTime(int number)
        {
            if (number < 10) return "0" + number;
            else return number.ToString();
        }

        void datagrid()
        {
            table.Rows.Add(
                           mySensorLog.No,
                           yaw,
                          pitch,
                          roll,
                          float.Parse(data[4]),
                          float.Parse(data[5]),
                          float.Parse(data[6]),
                          float.Parse(data[7]),
                          float.Parse(data[8]));
  
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


        private async void OnImageButtonClick(object sender, RoutedEventArgs e)
        {

            if (comboPicture.SelectedIndex != 0)
            {
                //try
                //{
                terminalText.AppendText("start capturing..." + Environment.NewLine);
                string path = Path.Combine(Environment.CurrentDirectory, @"ImgLog\");
                for (int t = 1; t <= comboPicture.SelectedIndex; t++)
                {
                    string logimgFileName = "Img " + DateTime.Now.Date.ToString("dd-MM-yyyy ") + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString() + ".jpg";
                    Directory.CreateDirectory(path);
                    string imgpath = Path.Combine(Environment.CurrentDirectory, @"ImgLog\", logimgFileName);
                    webCameraControl.GetCurrentImage().Save(imgpath);
                    terminalText.AppendText(t + ". " + logimgFileName + " captured" + Environment.NewLine);
                    //Thread.Sleep(1000 / comboPicture.SelectedIndex);
                    await Task.Delay(1000 / comboPicture.SelectedIndex);
                }
                terminalText.AppendText("file saved on " + path + Environment.NewLine);
                //}
                //   catch
                //{

                //}
            }
            else
            {
                MessageBox.Show("Pilih banyak capture/s", "Important Message");
            }

        }
        //private void dirButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new System.Windows.Forms.FolderBrowserDialog();
        //    var dialog = new FolderBrowserDialog();
        //    dirgambar = dialog.FileName;

        //}

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (stopwatch.IsRunning && stopwatch.ElapsedMilliseconds / interval > intervalCount)
            {
                intervalCount = Convert.ToInt32(stopwatch.ElapsedMilliseconds / interval);

                if (konekin.statusKoneksi() && captureFlag)
                {
                    try
                    {
                        table.Rows.Add(
                            mySensorLog.No,
                            float.Parse(lblKetinggian.Content.ToString()),
                            float.Parse(lblTemperatur.Content.ToString()),
                            float.Parse(lblTekanan.Content.ToString()),
                            float.Parse(lblElevasi.Content.ToString()),
                            //float.Parse(lblLatitude.Content.ToString()),
                            //float.Parse(lblLongitude.Content.ToString()),
                            float.Parse(lblYaw.Content.ToString()),
                            float.Parse(lblPitch.Content.ToString()),
                            float.Parse(lblRoll.Content.ToString()));
                        SaveLog();
                        datagrid();
                        grafikBebas();

                        if (datagridLog.Items.Count > 0)
                        {
                            var border = VisualTreeHelper.GetChild(datagridLog, 0) as Decorator;

                            if (border != null)
                            {
                                var scroll = border.Child as ScrollViewer;
                                if (scroll != null) scroll.ScrollToEnd();
                            }
                        }

                        mySensorLog.No += (float)interval / 1000;
                    }
                    catch { }
                }
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
                    e.Handled = true;
                    konekin.tulis(e.Key.ToString().ToLower());
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
         
                if (data[0] == "u")
                {
                    try
                    {
                        this.Dispatcher.Invoke(DispatcherPriority.Render, new Action(delegate ()
                        {
                            try
                            {
                                bar();
                                timer();
                                terminalText.AppendText("u" + " " + lblYaw.Content + " " + lblPitch.Content + " " + lblRoll.Content + " " + lblKetinggian.Content + " " + data[5] + " " + data[6] + " " + data[7] + " " + data[8] + Environment.NewLine);
                                // terminalText.AppendText("u" + " " + q0 + " " + q1 + " " + q2 + " " + q3  + Environment.NewLine);

                                terminalText.SelectionStart = terminalText.Text.Length;
                                terminalText.CaretIndex = terminalText.Text.Length;
                                terminalText.ScrollToEnd();

 

                                map();
                                putar3d();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initBaudList();
            initComPortList();
            initDatabitsList();
            initStopbitsList();
            initParityList();
            initHandshakeList();
            initDelay();

            logFileName = "WSN Log " + DateTime.Now.Date.ToString("dd-MM-yyyy ") + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString() + "." + DateTime.Now.Second.ToString() + ".xlsx";
        }

  

        #endregion

        #region visualisasi
        void timer()
        {
            lblTimer.Content = FormatTime(stopwatch.Elapsed.Hours) + ":" + FormatTime(stopwatch.Elapsed.Minutes) + ":" + FormatTime(stopwatch.Elapsed.Seconds);

        }


        void bar()
        {
            //mySensorLog.q0 = float.Parse(data[9], System.Globalization.CultureInfo.InvariantCulture);
            mySensorLog.q1 = float.Parse(data[1], System.Globalization.CultureInfo.InvariantCulture);
            mySensorLog.q2 = float.Parse(data[2], System.Globalization.CultureInfo.InvariantCulture);
            mySensorLog.q3 = float.Parse(data[3], System.Globalization.CultureInfo.InvariantCulture);
            //q0 = mySensorLog.q0;
            q1 = mySensorLog.q1;
            q2 = mySensorLog.q2;
            q3 = mySensorLog.q3;

            //roll = (180 / Math.PI) * Math.Atan2(2 * (q1 * q0 + q2 * q3), 1 - 2 * (q1 * q1 - q3 * q3));
            //pitch = -(180 / Math.PI) * Math.Asin(2 * (q0 * q2 - q3 * q1));
            //yaw = -(180 / Math.PI) * Math.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (q2 * q2 + q3 * q3));
            yaw = q1;
            pitch = q2;
            roll = q3;
            // mySensorLog.Yaw = float.Parse(yaw, System.Globalization.CultureInfo.InvariantCulture);
            lblYaw.Content = Math.Round(yaw, 2);
            barYaw.Width = 110 * (yaw + 180) / 360;
            //yawLog.Add(mySensorLog.Yaw);

            //lblYaw.Content = data[1];// + " deg";
            //mySensorLog.Yaw = float.Parse(data[1]);
            //yawLog.Add(mySensorLog.Yaw);

            // mySensorLog.Pitch = float.Parse(data[3], System.Globalization.CultureInfo.InvariantCulture);
            lblPitch.Content = Math.Round(pitch, 2);
            barPitch.Width = 110 * (pitch + 180) / 360;
            //pitchLog.Add(mySensorLog.Pitch);
            //lblPitch.Content = data[2];// + " deg";
            //mySensorLog.Pitch = float.Parse(data[2]);
            //pitchLog.Add(mySensorLog.Pitch);

            //mySensorLog.Roll = float.Parse(data[1], System.Globalization.CultureInfo.InvariantCulture);
            lblRoll.Content = Math.Round(roll, 2);
            barRoll.Width = 110 * (roll + 180) / 360;
            //rollLog.Add(mySensorLog.Roll);

            mySensorLog.Tinggi = float.Parse(data[4], System.Globalization.CultureInfo.InvariantCulture);
            lblKetinggian.Content = Math.Round(mySensorLog.Tinggi, 2);
            barKetinggian.Width = 110 * mySensorLog.Tinggi / 50;
            //tinggiLog.Add(mySensorLog.Ti  nggi);



            /*     mySensorLog.Percepatan = float.Parse(data[9], System.Globalization.CultureInfo.InvariantCulture);
                 lblPercepatan.Content = Math.Round(mySensorLog.Percepatan, 2);
                 barPercepatan.Width = 110 * mySensorLog.Percepatan / 50;
                 //tinggiLog.Add(mySensorLog.Tinggi);*/

            mySensorLog.Tekanan = float.Parse(data[8], System.Globalization.CultureInfo.InvariantCulture);
            lblTekanan.Content = Math.Round(mySensorLog.Tekanan, 2);
            barTekanan.Width = 110 * mySensorLog.Tekanan / 100;
            //tekananLog.Add(mySensorLog.Tekanan);

            mySensorLog.Suhu = float.Parse(data[7], System.Globalization.CultureInfo.InvariantCulture);
            lblTemperatur.Content = Math.Round(mySensorLog.Suhu, 2);
            barTemperatur.Width = 110 * mySensorLog.Suhu / 100;
            //suhuLog.Add(mySensorLog.Suhu);

            lblElevasi.Content = 90 - Math.Abs(float.Parse(data[2], System.Globalization.CultureInfo.InvariantCulture));
            mySensorLog.Elevasi = float.Parse(lblElevasi.Content.ToString());
            barElevasi.Width = 110 * (mySensorLog.Elevasi + 180) / 360;
            //elevasiLog.Add(mySensorLog.Elevasi);

            mySensorLog.Latitude = float.Parse(data[5], System.Globalization.CultureInfo.InvariantCulture).ToString();
            mySensorLog.Longitude = float.Parse(data[6], System.Globalization.CultureInfo.InvariantCulture).ToString();

            //jarak = 
        }
        void map()
        {
            double latitude = Convert.ToDouble(data[5]);
            double longitude = Convert.ToDouble(data[6]);
            location = new Location(latitude, longitude);
            if (dataPertama)
            {
                previousLocation = location;
                previousI = i;
                dataPertama = false;
            }
            MyPushPin.Location = location;
            myPolyline.Locations.Add(new Location(latitude, longitude));
            MyMap.Children.Add(myPolyline);
            MyMap.Center = location;
            lblLatitude.Content = mySensorLog.Latitude;
            lblLongitude.Content = mySensorLog.Longitude;


        }

        private void grafikBebas()
        {
            Point pYaw = new Point(mySensorLog.No, mySensorLog.Yaw);
            Point pPitch = new Point(mySensorLog.No, mySensorLog.Pitch);
            Point pRoll = new Point(mySensorLog.No, mySensorLog.Roll);
            Point pElev = new Point(mySensorLog.No, mySensorLog.Elevasi);
            Point pTinggi = new Point(mySensorLog.No, mySensorLog.Tinggi);

            try
            {
                dataYaw.Collection.Add(pYaw);
                sourceYaw.Collection.Add(pYaw);
                if (sourceYaw.Collection.Count >= 20)
                {
                    sourceYaw.Collection.RemoveAt(0);
                }

                dataPitch.Collection.Add(pPitch);
                sourcePitch.Collection.Add(pPitch);
                if (sourcePitch.Collection.Count >= 20)
                {
                    sourcePitch.Collection.RemoveAt(0);
                }

                dataRoll.Collection.Add(pRoll);
                sourceRoll.Collection.Add(pRoll);
                if (sourceRoll.Collection.Count >= 20)
                {
                    sourceRoll.Collection.RemoveAt(0);
                }

                dataElevasi.Collection.Add(pElev);
                sourceElevasi.Collection.Add(pElev);
                if (sourceElevasi.Collection.Count >= 20)
                {
                    sourceElevasi.Collection.RemoveAt(0);
                }

                dataTinggi.Collection.Add(pTinggi);
                sourceTinggi.Collection.Add(pTinggi);
                if (sourceTinggi.Collection.Count >= 20)
                {
                    sourceTinggi.Collection.RemoveAt(0);
                }
            }
            catch { }
        }

        void putar3d()
        {
            double yaw = Math.Ceiling(mySensorLog.Yaw);
            double pitch = Math.Ceiling(mySensorLog.Pitch);
            double roll = Math.Ceiling(mySensorLog.Roll);

            Vector3D axisYaw = new Vector3D(0, 0, -1);
            Vector3D axisPitch = new Vector3D(1, 0, 0);
            Vector3D axisRoll = new Vector3D(0, 1, 0);

            Transform3DGroup group = new Transform3DGroup();

            QuaternionRotation3D r;

            try
            {
                r = new QuaternionRotation3D(new Quaternion(axisYaw, pitch));//(new Vector3D(0, 1, 0), yaw));
                group.Children.Add(new RotateTransform3D(r));
                r = new QuaternionRotation3D(new Quaternion(axisPitch, -yaw));//(new Vector3D(1, 0, 0), pitch));
                group.Children.Add(new RotateTransform3D(r));
                r = new QuaternionRotation3D(new Quaternion(axisRoll, roll));//(new Vector3D(0, 0, 1), roll));
                group.Children.Add(new RotateTransform3D(r));
                rocket3D.Transform = group;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

        }
        #endregion

        #region data
        void SaveLog()
        {
            try
            {
                tableToSave = new DataTable();
                tableToSave = table.Copy();
                CreateExcelFile.CreateExcelDocument(tableToSave, logFileName);
                using (StreamWriter writer = File.AppendText(logName))
               
                {
                    writer.WriteLine("u" + " " + lblYaw.Content + " " + lblPitch.Content + " " + lblRoll.Content + " " + lblKetinggian.Content + " " + data[5] + " " + data[6] + " " + data[7] + " " + data[8] + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membuat file excel.\r\nPesan kesalahan: " + ex.Message);
                return;
            }
        }

        void InitializeDataTable()
        {

            table.Columns.Add("Detik");
            table.Columns.Add("Yaw");
            table.Columns.Add("Pitch");
            table.Columns.Add("Roll");
            table.Columns.Add("Altitude");
            table.Columns.Add("Longitude");
            table.Columns.Add("Latitude");
            table.Columns.Add("Temperature");
            table.Columns.Add("Tekanan"); //belum fix
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
