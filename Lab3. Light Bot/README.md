#DevCon School 2016: IoT-интенсив 

# Лабораторная 3: Light Bot.

В этой лабораторной работе мы используем Azure IoT Hub для установления двухсторонней связи с устройством. Мы реалзизуем
лампочку-светодиод, управляемый с помощью чат-бота.

## Создаём IoT-hub

Для передачи событий нам потребуется IoT Hub. Создайте его в своей облачной подписке (раздел "Интернет вещей").
После этого скопируйте строку подключения к IoT Hub (будем называть её строкой подключения хаба), соответствующую 
уровню доступа `iothubowner`.

![Get Access Key](../images/IoTHub_AccessKeys.PNG)

## Подключаемся к IoT-хабу и создаем строку подключения для устройства

Используйте и установите [Device Explorer](https://github.com/Azure/azure-iot-sdk-csharp/tree/master/tools/DeviceExplorer). В нем введите
строку подключения к IoT-хабу:

![Device Explorer 1](../images/DeviceExplorer1.PNG)

После этого перейдите на вкладку "Management" и добавьте новое устройство. Затем правой кнопкой нажмите на строку с устройством и выберите "Copy Connection String" 
(эту строку будем называть строкой подключения устройства).

![Device Explorer 2](../images/DeviceExplorer2.PNG)

## Получаем код для работы с IoT-хабом

Очень хорошая страничка [Getting Started](https://catalog.azureiotsuite.com/getstarted) есть в MSDN. 
Там много примеров работы с IoT Hub. 

В нашем случае, чтобы разобраться как посылать сообщения в IoT Hub можно воспользоваться пошаговым руководством из [этой](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-csharp-csharp-getstarted) статьи.

## Отправляем в IoT Hub данные о состоянии светодиода

Продолжаем использовать код от лабораторной работы 1 и добавляем код для подключения к IoT Hub в функции `OnNavigatedTo`:
```
iothub = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
await iothub.OpenAsync();
```

Затем при изменении состояния светодиода будем посылать в IoT Hub сообщения *on* или *off*:
```
var s = state ? "on" : "off";
var b = Encoding.UTF8.GetBytes(s);
await iothub.SendEventAsync(new Message(b));
```

## Принимаем данные из IoT Hub
Команды от бота для зажигания/гашения светодиода мы также будем получать из IoT Hub. Для приёма сообщений из 
IoT Hub можно использовать следующий код:
```
        private async Task Receive()
        {
            while (true)
            {
                var msg = await iothub.ReceiveAsync();
                if (msg != null)
                {
                    var s = Encoding.ASCII.GetString(msg.GetBytes());
                    // Сделать что-то с полученным сообщением, например, зажечь светодиод
                    LED(s=="on");
                    await iothub.CompleteAsync(msg);
                }
            }
        }
```

Для запуска постоянного цикла мониторинга достаточно вызывать функцию `Receive` после открытия IoT Hub, но без использования
`await`, чтобы мы не ожидали завершения бесконечного цикла (игнорируйте предупреждение Visual Studio).

## Создаём чат-бота 

Процесс создания бота описан [вот здесь](https://github.com/evangelism/ModernAI/tree/master/SimpleCommandBot) 
или [в этой статье](http://blog.soshnikov.com/2016/04/12/hello-bot-%D1%87%D0%B0%D1%82-%D0%B1%D0%BE%D1%82%D1%8B%D1%81%D0%BB%D0%B5%D0%B4%D1%83%D1%8E%D1%89%D0%B5%D0%B5-%D0%BF%D0%BE%D0%BA%D0%BE%D0%BB%D0%B5%D0%BD%D0%B8%D0%B5-%D0%BF%D1%80%D0%B8%D0%BB/).

Установите Bot Application Template, создайте новый проект бота. После этого останется добавить логику отправления
и приёма сообщений из IoT Hub.

Для работы с IoT Hub на стороне сервера служит пакет `Microsoft.Azure.Devices`, который надо подключить с помощью NuGet.

Для отправки сообщения в IoT Hub используется следующий код:

```
var hub = ServiceClient.CreateFromConnectionString(HubConnString);
await hub.SendAsync(DEVICE_ID, new Message(Encoding.UTF8.GetBytes("Hello")));
```

Здесь `HubConnString` - строка подключения к IoT Hub (не к конкретному устройству, а к хабу в целом), `DEVICE_ID` -
идентификатор устройства, которому надо послать сообщение, `"Hello"` - посылаемое сообщение.

Это позволит нам посылать в хаб сообщения "on" и "off", управляя тем самым зажиганием лампочки.

Приём сообщений с IoT Hub несколько сложнее, но он хорошо описан [в этой статье](https://blogs.windows.com/buildingapps/2015/12/09/windows-iot-core-and-azure-iot-hub-putting-the-i-in-iot/#XaVrtzWBatUCpe0B.97)

Вам необходимо подключить пакет `WindowsAzure.ServiceBus`, поскольку общаться с IoT Hub мы будем как с `Event Hub` - 
на самом деле эти две облачных сущности весьма похожи.

Вот как выглядит код для приёма сообщений из IoT Hub:

```
    EventHubClient cli = EventHubClient.CreateFromConnectionString(HubConnString, "messages/events");

    var runtimeInfo = await cli.GetRuntimeInformationAsync();
    foreach (var p in runtimeInfo.PartitionIds)
    {
        var rec = await cli.GetDefaultConsumerGroup().CreateReceiverAsync(p);
        Func<Task> f = async () =>
        {
            while (true)
            {
                var x = await rec.ReceiveAsync();
                var s = Encoding.UTF8.GetString(x.GetBytes());
                Console.WriteLine(s); // ИЛИ ЧТО-ТО ЕЩЁ
            }
        };
        f();
    }
```

Здесь используется очень хитрый способ создания параллельных потоков. На самом деле, для каждого раздела IoT Hub
создается своя асинхронная функция для чтения потока, которая запускается без ожидания завершения (для этого слово 
`await` пропущено), и продолжает работу в течение всего времени работы программы.

Чтобы бот мог послать сообщение пользователю, ему нужно знать имя пользователя и канал связи. В нашем демо-примере
при каждом обращении к боту (в методе `Post` в `MessagesController`) мы будем запоминать `activity` и `connector` в
некотором статическом классе, и потом использовать их для отправки сообщения.
