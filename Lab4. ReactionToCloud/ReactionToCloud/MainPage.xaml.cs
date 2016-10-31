using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lab5.ReactionToCloud
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool send = true; // send data to cloud or just testing

        private const string Id = "RPi";
        private const string DeviceConnectionString = "HostName=samplehub13.azure-devices.net;DeviceId=RPi;SharedAccessKey=ZWnxUTH194FsiQrMrfsC8z5xW/oS3TfkAaVcIXLgZ1Q=";
        private const string Nick = "shwars";
        private const int Age = 41;
        private const string Sex = "M";
        private const int Dizz = 0;
        // 0 - нормальное состояние
        // 1 - 5 поворотов вокруг своей оси
        // 2 - 10 поворотов вокруг своей оси

        private int DeviceType = 0;
        // 0 - Raspberry Pi
        // 1 - PC UWP

        GpioPin output_pin;
        GpioPin input_pin;

        Stopwatch sw = new Stopwatch();

        private async Task SendData(long ticks,int noexp,int pw)
        {
            var d = new ReactionData(Id,Nick,Age,Sex,Dizz,ticks,DeviceType,noexp,pw);
            var s = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            var b = Encoding.UTF8.GetBytes(s);
            if (send) await iothub.SendEventAsync(new Message(b));
        }

        private DeviceClient iothub;

        public MainPage()
        {
            this.InitializeComponent();
            // Инициализируем пины вывода
            var gpio = GpioController.GetDefault();
            if (gpio != null)
            {
                output_pin = gpio.OpenPin(17);
                output_pin.SetDriveMode(GpioPinDriveMode.Output);

                // Инициализируем пин ввода
                input_pin = gpio.OpenPin(26);
                input_pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                // pin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                // pin.ValueChanged += ButtonPressed;
            }
            iothub = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
            DoWork();
        }

        Random Rnd = new Random();

        private async Task DoWork()
        {
            var noexp = 0;
            int PreWait;
            while (true)
            {
                output_pin.Write(GpioPinValue.Low);
                await Task.Delay(PreWait=Rnd.Next(2000, 5000));
                output_pin.Write(GpioPinValue.High);
                sw.Restart();
                if (input_pin.Read() == GpioPinValue.Low) continue; // if the button is pre-pressed, ignore
                while (input_pin.Read()==GpioPinValue.High);
                sw.Stop();
                output_pin.Write(GpioPinValue.Low);
                txt.Text = sw.ElapsedTicks.ToString();
                await SendData(sw.ElapsedTicks,noexp,PreWait);
                await Task.Delay(1000);
                noexp++;
            }
        }
    }
}
