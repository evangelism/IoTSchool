using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace Lab2.Measure_Reaction
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        GpioPin led_pin = null;
        GpioPin input_pin;

        Stopwatch sw = new Stopwatch();

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

            Window.Current.CoreWindow.KeyDown += KeyPressed;

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

        private async Task DoWork()
        {
            while (true)
            {
                LED(false);
                await Task.Delay(Rnd.Next(1000, 5000));
                LED(true);
                sw.Restart();
                while (sw.IsRunning) await Task.Delay(100);
                txt.Text = sw.ElapsedTicks.ToString();
            }
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
