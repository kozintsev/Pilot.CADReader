# Pilot.CADReader

Плагин для интеграции системы Pilot-ICE с КОМПАС-3D.

Плагин позволяет автоматически получать состав изделия из документов КОМПАС-3D, а так же формировать вторичное представление в формате PDF.

Для работы плагина нужно установить конфигурацию [Configuration-pilot-dd.pilotcfg](https://www.dropbox.com/s/6ed7shh4phem4cv/Configuration-pilot-dd.pilotcfg?dl=0) или подключить базу данных доступную по ссылке.

## Возможности:

- автоматическое формирование pdf документов из исходных данных (для чертежей и 3D моделей, используется API КОМПАС, требуется лицензия)
- автоматичесое формирование xps документа из исходных данных (для спецификации, используется API КОМПАС, требуется лицензия)
- получение информации из чертежа (атрибуты основной надписи, листы и их свойства)
- получение состава изделия из простой спецификации ГОСТ 2.106-96.

## Технические требования:

- Pilot-ICE версия 16.0.32
- КОМПАС-График или КОМПАС-3D V16, V17

## Планы

- Переработка машиностроительной конфигурации (использование абстрактных типов из Компас: Чертёж, Спецификация, Деталь, Сборочная единица, потому что именно данные документу участвуют в документообороте)
- Печать в XPS всех документов
- Оптимизация обновления дерева документов, в данный момент дерево не строится, только список объектов
- Обновление дерева документов с помощью ссылок на исходные файлы
- Построение дерева документов на основе спецификации (вычисление родителей и потомков, определение корневого объекта)

## История версии

19.12.2017
- Автоматическая печать спецификации в xps
- Автоматическое заполнение карточки спецификации
- Получение информации из чертежа
- Актуализация Pilot SDK
- Поддержка КОМПАС 3D V17

30.03.2016
- выход пре-релиза, исправление ошибок и оптимизация

19.03.2016
- автоматическое формирование pdf документов из исходных данных (используется API КОМПАС, требуется лицензия)

18.03.2016
- получение состава изделия из простой спецификации ГОСТ 2.106-96.

## Если Вам понравился проект, то вы его можете поддержать:

- Yandex.Money: [Donate](https://money.yandex.ru/to/410015409987387)

[Страница для сбора пожертвований](http://yasobe.ru/na/pilotkompas "http://yasobe.ru/na/pilotkompas - Страница для сбора пожертвований") 


## Ссылки:

[http://pilotems.com/ru/](http://pilotems.com/ru/ "Pilot-ICE — система нового поколения для управления проектной организацией") - Pilot-ICE — система нового поколения для управления проектной организацией

[http://kompas.ru/](http://kompas.ru/ "Официальный сайт САПР КОМПАС") - Официальный сайт САПР КОМПАС

[http://ascon.ru/](http://ascon.ru/) - официальный сайт компании АСКОН, разработчика КОМПАС и Pilot-ICE

[Configuration-pilot-dd.pilotcfg](https://www.dropbox.com/s/6ed7shh4phem4cv/Configuration-pilot-dd.pilotcfg?dl=0) -  специализированная конфигурация для Pilot-ICE.

[pilot-dd.zip](https://www.dropbox.com/s/nbob9lq8v7rwu2x/pilot-dd.zip?dl=0 "pilot-dd.zip") - база данных для системы Pilot-ICE (содержит специализированную конфигурацию и плагин)

[Promo.avi](https://www.dropbox.com/s/58crpyphmoxcl2r/Promo.avi?dl=0 "Promo.avi") - видеоролик демонстрирующий работу

[Ascon.Pilot.SDK.SpwReader.zip](https://www.dropbox.com/s/nhrl9g14fe5wbw1/Ascon.Pilot.SDK.SpwReader.zip?dl=0) - плагин

