using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace LED_Blink
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        GpioPin pin = null;
        bool state = false;

        public MainPage()
        {
            this.InitializeComponent();
            var gpio = GpioController.GetDefault();

            // Если приложение будет запущено на компьютере без IoT,
            // то gpio==null. Поэтому добавляем проверки на null,
            // чтобы приложение работало и на desktop/mobile
            pin = gpio?.OpenPin(17);
            pin?.SetDriveMode(GpioPinDriveMode.Output);

            DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            dt.Tick += Blink;
            dt.Start();
        }

        private void Blink(object sender, object e)
        {
            state = !state;
            LED(state);
        }

        private void LED(bool state)
        {
            vled.Fill = new SolidColorBrush(state ? Colors.Red : Colors.LightGray);
            if (pin != null)
            {
                pin.Write(state ? GpioPinValue.High : GpioPinValue.Low);
            }
        }
    }
}
