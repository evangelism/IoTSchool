using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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

namespace LED_Blink
{

    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        const string HubConnectionString = "HostName=samplehub13.azure-devices.net;DeviceId=RPi;SharedAccessKey=ZWnxUTH194FsiQrMrfsC8z5xW/oS3TfkAaVcIXLgZ1Q=";

        public DeviceClient hub;

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
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            hub = DeviceClient.CreateFromConnectionString(HubConnectionString);
            await hub.OpenAsync();

            // Начинаем получать сообщения из IoT Hub
            // Здесь специально нет await, т.к. нам не надо ожидать окончания выполнения
            Receive();
        }

        private async void Blink(object sender, object e)
        {
            state = !state;
            LED(state);
            await Send(state ? "on" : "off");
        }

        private async Task Send(string s)
        {
            await hub.SendEventAsync(new Message(Encoding.UTF8.GetBytes(s)));
        }

        private void LED(bool state)
        {
            vled.Fill = new SolidColorBrush(state ? Colors.Red : Colors.LightGray);
            if (pin != null)
            {
                pin.Write(state ? GpioPinValue.High : GpioPinValue.Low);
            }
        }

        private async Task Receive()
        {
            await Task.Delay(1000);
            while (true)
            {
                var msg = await hub.ReceiveAsync();
                if (msg != null)
                {
                    var s = Encoding.ASCII.GetString(msg.GetBytes());
                    LED(s=="on");
                    await hub.CompleteAsync(msg);
                }
            }
        }
    }
}
