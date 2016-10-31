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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lab4.Reaction_to_Cloud
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string Id = "shwars";
        private const string DeviceConnectionString = "HostName=devconhub.azure-devices.net;DeviceId=shwars;SharedAccessKey=wr1V/lNlUTPd8jfn8edsnBuZfW1hY8tT9OYLUyjTY0c=";
        private const string Nick = "shwars";
        private const int Age = 42;
        private const string Sex = "M";
        private const int Dizz = 0;
        // 0 - нормальное состояние
        // 1 - 5 поворотов вокруг своей оси
        // 2 - 10 поворотов вокруг своей оси

        private int DeviceType = 0;
        // 0 - Raspberry Pi
        // 1 - PC UWP

        GpioPin led_pin = null;
        GpioPin input_pin;

        Stopwatch sw = new Stopwatch();

        DeviceClient iothub;

        public MainPage()
        {
            this.InitializeComponent();

            // Инициализируем пины вывода
            var gpio = GpioController.GetDefault();
            led_pin = gpio?.OpenPin(17);
            led_pin?.SetDriveMode(GpioPinDriveMode.Output);

            // Инициализируем пин ввода
            input_pin = gpio?.OpenPin(26);
            if (input_pin != null)
            {
                input_pin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                input_pin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                input_pin.ValueChanged += ButtonPressed;
            }
            DeviceType = gpio == null ? 1 : 0;

            Window.Current.CoreWindow.KeyDown += KeyPressed;

            iothub = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

            DoWork();

        }

        private void KeyPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            sw.Stop();
        }

        private void LED(bool state)
        {
            vled.Fill = new SolidColorBrush(state ? Colors.Red : Colors.LightGray);
            if (led_pin != null)
            {
                led_pin.Write(state ? GpioPinValue.High : GpioPinValue.Low);
            }
        }

        Random Rnd = new Random();
        int PreWait;
        int noexp = 0;
        private async Task DoWork()
        {
            while (true)
            {
                LED(false);
                await Task.Delay(PreWait=Rnd.Next(1000, 5000));
                LED(true);
                sw.Restart();
                while (sw.IsRunning) await Task.Delay(100);
                txt.Text = sw.ElapsedTicks.ToString();
                await SendData(sw.ElapsedTicks, noexp, PreWait);
                noexp++;
            }
        }

        private async Task SendData(long ticks, int noexp, int pw)
        {
            var d = new ReactionData(Id, Nick, Age, Sex, Dizz, ticks, DeviceType, noexp, pw);
            var s = Newtonsoft.Json.JsonConvert.SerializeObject(d);
            var b = Encoding.UTF8.GetBytes(s);
            await iothub.SendEventAsync(new Message(b));
        }

        private void ButtonPressed(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge==GpioPinEdge.RisingEdge)
            {
                sw.Stop();
            }
        }

        private void vled_Tapped(object sender, TappedRoutedEventArgs e)
        {
            sw.Stop();
        }
    }
}
