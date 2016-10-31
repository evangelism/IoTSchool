#DevCon School 2016: IoT-интенсив 

# Лабораторная 4: Посылаем время реакции в облако

В этом упражнении мы соберем время нашей реакции, измеренное приложением из лабораторной 2, и пошлем его в единый IoT-хаб в облаке.

## Подключаемся к IoT Hub и создаем запись для своего устройства

В Device Explorer задаём следующую строку подключения к IoT Hub:
```
HostName=devconhub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=GSzUSKj0XAQ5jHJF8+nz97ysfo65hbM0VIic8oSb4RU=
```

После этого добавьте своё устройство в разделе *Management* и скопируйте его строку подключения.

## Получите приложение из GitHub и исправьте константы

Чтобы исключить разницу от программного кода, мы все будем использовать одну и ту же версию приложения, 
загруженную с GitHub. Получите
приложение Lab4 и исправьте в коде константы в соответствии со своими данными:

```
        string Id = "rpi3";
        string DeviceConnectionString = "[YOUR CONNECTION STRING]";
        string Nick = "shwars";
        int Age = 42;
        string Sex = "M";
        int Dizz = 0;
        // 0 - нормальное состояние
        // 1 - 5 поворотов вокруг своей оси
        // 2 - 10 поворотов вокруг своей оси

        private int DeviceType = 0;
        // 0 - Raspberry Pi
        // 1 - PC UWP

```

Затем скомпилируйте приложение, загрузите его на устройство, и измерьте время реакции в трех случаях:

  * В нормальном состоянии после запуска (сделайте минимум 10 замеров)
  * После 10 поворотов вокруг свой оси (сделайте минимум 3 подхода по 3 замера)
  * После 20 поворотов вокруг свое оси (3 подхода по 3 замера)

Между замерами останавливайте приложение и меняйте в коде соответствующие константы, чтобы в IoT Hub были записаны правильные данные.

## Создаём задание Stream Analytics

Для того, чтобы использовать данные из IoT Hub, проще всего использовать технологию **Stream Analytics**, которая
будет агрегировать данные и/или перебрасывать их в какое-то постоянное хранилище.

Для начала, настроим Stream Analytics для передачи данных своего устройства из IoT Hub в PowerBI. Для этого 
необходимо создать объект Stream Analytics, сконфигурировать в нем входные и выходные потоки данных и задать запрос. 

В качестве входных данных используем IoT Hub, назовем входные данные `InHub`, укажем данные подключения:

  * IoT Hub: devconhub
  * Endpoint: messaging
  * Shared Access Policy name: iothubowner
  * Shared Accesss Policy Key: GSzUSKj0XAQ5jHJF8+nz97ysfo65hbM0VIic8oSb4RU=

В качестве формата данных надо будет указать JSON, поскольку Stream Analytics умеет понимать структуру данных
и использовать конкретные поля в запросе.
 
В качестве выходных данных укажем Power BI, назовём их `OutBI`. 

Для выборки данных со своего устройства используем такой запрос:
```
SELECT
    *
INTO
    [OutBI]
FROM
    [InHub]
WHERE Id='MyDeviceID'
```

Если мы хотим также различать первый эксперимент от последующих (добавить соответствующую вычисляемую колонку), плюс
запоминать время измерений, то можем модифицировать запрос следующим образом:

```
SELECT
    *, System.Timestamp as [TimeStamp],
    CASE 
      WHEN NoExperiment=0 THEN 'First'
      ELSE 'Consecutive'
    END as ExperiementKind
INTO
    [OutBI]
FROM
    [InHub]
WHERE Id='MyDeviceID'
```

Для усреднения времени реакции за каждые 5 минут, и получения данных по всем участникам, можно использовать такой запрос:
```
SELECT
    Id, AVG(Reaction) as React, 
    MAX(Time) as EndTime, MIN(Time) as BeginTime
INTO [OutBI]
FROM [InHub] TIMESTAMP BY Time
GROUP BY Id, TumblingWindow(Duration(minute,5))
```

Вам может пригодится [прекрасный документ с набором примеров запросов](https://azure.microsoft.com/en-us/documentation/articles/stream-analytics-stream-analytics-query-patterns/).

После конфигурирования задания Stream Analytics необходимо запустить задание на выполнение, и смотреть на появившиеся
данные в PowerBI.

