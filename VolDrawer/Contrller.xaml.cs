using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.IO.Ports;

namespace VolDrawer;

public partial class Contrller : Window
{
    private SerialPort COM = null;
 

    
    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x80000;
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    public MainWindow _mainWindow = null;
    
    byte[] ReadForSize(SerialPort COM, int Size)
    {
        byte[] Buffer = new byte[Size];
        int ReadedSize = 0;

        do
        {
            int sc = COM.Read(Buffer, ReadedSize, Size - ReadedSize);
            ReadedSize += sc;
        } while (ReadedSize < Size);

        return Buffer;
    }

    double Get()
    {
        byte[] Zero = new []{ (byte)1};
        //COM5.DiscardInBuffer();
        COM.Write(Zero, 0, 1);
        var Buffer = ReadForSize(COM,2);

        return BitConverter.ToUInt16(Buffer) / 4096d * 3.3d;
    }

    void Set(double value)
    {
        byte[] Ote = new byte[] { 0, 0, 0};
        byte[] Dta = BitConverter.GetBytes((UInt16)(value / 3.3d * 4095d));
        Ote[1] = Dta[0];
        Ote[2] = Dta[1];
        
        
        COM.Write(Ote, 0, 3);
    }

    public Contrller()
    {
        InitializeComponent();
        Loaded += (e, v) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
        //    SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            foreach (var c in SerialPort.GetPortNames())
            {
                Listx.Items.Add(c);
            }

            Listx.Text = "COM12";
        };
    }

    public bool CanClose = false;
    private void Contrller_OnClosing(object? sender, CancelEventArgs e)
    {
        if (!CanClose)
            e.Cancel = true;
    }

    private void Contrller_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Task.Run(async () =>
        {
            try
            {
                string wwc = "";
                await Dispatcher.BeginInvoke(() =>
                {  
                    wwc = Listx.Text;
                });
                COM = new SerialPort(wwc, 15000000);
                COM.Open();
                await Dispatcher.BeginInvoke(() =>
                {  
                    Start__.IsEnabled = false;
                });
                for (int i = 0; i < 1920; i++)
                {
                    double vol = i / 1920d * 3.3d;

                    Set(vol);
                    int a = (int)((Get() / 3.3d) * 1080d);
                
                    _mainWindow.Line1[i] = a;
                    for (int j = i + 1; j < 1920; j++)
                    {

                        _mainWindow.Line1[j] = -1;
                    }

                }
                await Dispatcher.BeginInvoke(() =>
                {
                    Start__.IsEnabled = true;
                });
                COM.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }
        });
    }
}