# DevCon School 2016: IoT-интенсив 

# Лабораторная 6: Распознаватель эмоций

В этом тьюториале нам предлагается сделать облачное решение, которое будет накапливать в облаке наши позитивные
и негативные эмоции и предоставлять аналитику.

## Скачиваем и запускаем приложение, определяющее человеческие эмоции

Приложение находится в этом репозитории GitHub. Для начала необходимо его скачать, скомпилировать и запустить.

1. Откройте файл FaceRecogitionTracker.sln в Visual Studio.
   Вы также можете использовать пункт меню "Open from source control..." в Visual Studio...
2. Получите ключ для использования Emotion API в Microsoft Cognitive Services. Зайдите на 
   https://www.microsoft.com/cognitive-services/, выберите Emotions API -> Get Started for Free, и получите
   ключ.
3. В файле Config.cs замените ключ на тот, который вы только что получили.

Запустите проект - вы должны увидеть, что лицо правильно распознается, а в окне "Вывод" показываются сообщения с 
эмоциями в формате JSON.

### Конфигурируем IoT Hub

Для приема сообщений в облаке мы будем использовать IoT Hub. Откроем панель управления Azure Portal http://portal.azure.com.

Для начала создаем в облаке свой IoT Hub (раздел "Интернет вещей"). После создания хаба скопируйте строку подключения:

![Get Access Key](images/IoTHub_AccessKeys.PNG)

### Подключаемся к IoT-хабу и создаем строку подключения для устройства

Используйте и установите [Device Explorer](https://github.com/Azure/azure-iot-sdks/blob/master/tools/DeviceExplorer/doc/how_to_use_device_explorer.md). В нем введите
строку подключения к IoT-хабу:

![Device Explorer 1](images/DeviceExplorer1.PNG)

После этого перейдите на вкладку "Management" и добавьте новое устройство. Затем правой кнопкой нажмите на строку с устройством и выберите "Copy Connection String".

![Device Explorer 2](images/DeviceExplorer2.PNG)

## Получаем код для работы с IoT-хабом

Очень хорошая страничка [Getting Started](https://azure.microsoft.com/ru-ru/develop/iot/get-started/) есть в MSDN. 
Выбираете своё устройство, язык программирования и т.д. - и получаете фрагмент кода.

В нашем случае выбираем Raspberry Pi 2 -> Windows -> C#. (Код для Raspberry Pi подойдет и для настольного UWP-приложения). 
При добавлении кода в проект, необходимо также добавить ссылку на NuGet-пакет
`Microsoft.Azure.Devices.Client`. В полученном коде не забудьте исправить строку подключения на ту, 
которая была получена на предыдущем шаге.

### Отправка данных в IoT Hub

Для отправки данных используется следующий код:

```
iothub = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
await iothub.OpenAsync();
...
var b = Encoding.UTF8.GetBytes(s);
await iothub.SendEventAsync(new Message(b));
```
Здесь `s` - это строка, содержащая JSON-код для эмоций.

## Настраиваем Stream Analytics

Для передачи данных из IoT Hub в систему хранения можно использовать Stream Analytics.

Для начала, настроим Stream Analytics для передачи данных из IoT Hub в PowerBI. Заодно можно осуществить усреднение данных по температуре за какой-то интервал времени,
например, 5 секунд.

Для этого необходимо создать объект Stream Analytics, сконфигурировать в нем входные и выходные потоки данных, и задать запрос. 

В качестве входных данных используем IoT Hub, назовем входные данные `InHub`. 
В качестве выходных данных - Power BI, назовём их `OutBI`. 
Обратите внимание, что в текущей версии портала Azure для конфигурирования PowerBI необходимо использовать старый портал http://manage.windowsazure.com.

В качестве простейшего запроса можно оставить исходный запрос, поставив правильные названия источника и приемника данных:

```
SELECT * INTO [OutBI] FROM [InHub]
```

Для усреднения данных за 5 секунд используем такой запрос:

```
SELECT
    AVG(Happiness) as AVHappiness,
    AVG(Surprise) as AVSurpive,
    ...
    MAX(Time) as EndTime, MIN(Time) as BeginTime
INTO [OutBI]
FROM [InHub] TIMESTAMP BY Time
GROUP BY TumblingWindow(Duration(second,5))
```

Вам может пригодится [прекрасный документ с набором примеров запросов](https://azure.microsoft.com/en-us/documentation/articles/stream-analytics-stream-analytics-query-patterns/).

После конфигурирования задания Stream Analytics необходимо запустить задание на выполнение.

## Настраиваем отчёт в PowerBI

После того, как Stream Analytics будет запущено, вы должны увидеть в панели PowerBI доступные данные:

![PowerBI](images/PowerBI.PNG)
