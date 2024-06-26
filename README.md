# 🤖 Schedule Bot 👾

### Описание 
Этот бот представляет собой удобный инструмент для студентов групп ПМИ-120 и ПРИ-121, которые хотят всегда быть в курсе своего учебного расписания. Бот предоставляет доступ к актуальному расписанию занятий, помогая студентам организовать свое время.

### Основные функции

1. Просмотр расписания: бот предоставляет актуальное расписание занятий для обеих групп, ПМИ-120 и ПРИ-121. Пользователи могут узнать, какие предметы запланированы на текущую неделю.

2. Отображение текущей недели: бот отображает информацию о текущей неделе (Числитель или Знаменатель), чтобы пользователь всегда знал, на какой неделе обучения он находится.

3. Расписание на день: пользователь может запросить расписание на определенный день недели. Бот предоставляет информацию о занятиях, включая номер пары, время начала и окончания, название предмета, тип занятия (лекция, практика, лабраторная работа), а также номер аудитории и имя преподавателя.


# Настройка бота для личного использования 
## Настройки подключения к базе данных с помощью СУБД PostgreSQL
Если необходимо использовать бота с базой данных, тогда необходимо настроить строку подключения к БД в файле `appsettings.json`. Далее бот проверит правильность введенной строки подключения, и на основе этого сделает выбор, как работать, с файловым хранилищем, что является вариантом по умолчанию, или с БД. Бот работает с СУБД PostgreSQL. Для подключения и работы с БД используется [Npgsql](https://www.npgsql.org/).

Для получения структуры базы данных необходимо восстановить ее из .dump файла, который представлен в папке `DataBaseDump`, сделать это можно с помощью представленных ниже команд, также ожидается, что PostgreSQL установлен и база данных `schedulebotdb` создана:

#### Восстановление структуры БД для Linux

Эта команда изменяет разрешения (права доступа) к вашему файлу дампа базы данных.
```bash
sudo chmod +r /path/to/your/dump/file/schedulebotdb_dumpfile.dump
```

Эта команда выполняет переключение на пользователя postgres с правами суперпользователя.
```bash
sudo -i -u postgres
```

Эта команда выполняет импорт (восстановление) базы данных из файла дампа.
```bash
psql -U postgres -d schedulebotdb -a -f /path/to/your/dump/file/schedulebotdb_dumpfile.dump
```

#### Восстановление структуры БД для Windows

Эта команда подключится к PostgreSQL под пользователем postgres, выберет базу данных schedulebotdb и выполнит SQL-запросы из вашего дамп-файла, восстанавливая базу данных из него.
```bash
psql -U postgres -d schedulebotdb -a -f path\to\your\dump\file\schedulebotdb_dumpfile.dump
```

## Взаимодействие со службой Windows

Службы Windows обычно используются для выполнения фоновых операций, таких как обновление программного обеспечения, мониторинг системных ресурсов, взаимодействие с оборудованием и другие автоматизированные задачи. Бот обладает необходимым функционалом для взаимодействия со службой Windows.
 
* Создание службы
```bash
sc.exe create ".NET Shedule Bot" binpath="Path to your exe" start="demand"
```
* Удаление службы
```bash
sc.exe delete ".NET Shedule Bot"
```

## Настройки бота в меню [BotFather](https://t.me/BotFather "У этого бота можно получить токен, а также персонализировать своего бота!")

[BotFather](https://t.me/BotFather) - это официальный бот Telegram, который позволяет создавать и настраивать других ботов в Telegram, с его помощью было проделано все то, что описано ниже.

* Команды

Меню с командами обеспечивает удобный и интуитивно понятный способ управления ботом без необходимости запоминать или вводить сложные тексты. Список команд будет отображаться после нажатия на кнопку "Меню". Ниже представлены команды, которые используются на текущий момент в боте, а также их описание.

```bash
start - обновление бота 
todaypmi - расписание на сегодня ПМИ-120
tomorrowpmi - расписание на завтра ПМИ-120
sessionpmi - расписание сессии ПМИ-120
todaypri - расписание на сегодня ПРИ-121
tomorrowpri - расписание на завтра ПРИ-121
sessionpri - расписание сессии ПРИ-121
```

* Описание

Описание ботов в Telegram - это краткая информация, которую вы можете предоставить пользователям о вашем боте, чтобы они понимали его назначение и функциональность. Это своего рода виртуальная визитная карточка вашего бота. Ниже представлено описание бота, которое используются на текущий момент.

```bash
Я могу показать расписание занятий таких групп: ПМИ-120 и ПРИ-121!

Для просмотра расписания необходимо выбрать группу и день недели, также я расскажу числитель или знаменатель сейчас!
```
# Сборка бота, и работа с ним на сервере
* Команда для сборки под Linux 

Эта команда выполняет сборку бота под операционную систему Linux, используя конфигурацию Release.

```bash
dotnet publish -c Release -r linux-x64
```

* Команда для сборки под Windows 

Эта команда выполняет сборку бота под операционную систему Windows, используя конфигурацию Release.

```bash
dotnet publish -c Release -r win-x64
```

## Инструкция по обновлению бота на сервере вручную 

Ниже представлен алгоритм ручной сборки бота на сервере.

* Остановка службы бота на сервере

Эта команда останавливает службу бота на сервере, чтобы можно было безопасно обновить его.

```bash
systemctl stop schedule-bot.service
```

* Прверяем корректность остановки службы

Эта команда проверяет текущий статус службы бота, чтобы удостовериться, что она успешно остановлена.

```bash
systemctl status schedule-bot.service
```

* Копирование файлов на сервер

Эта команда копирует файлы бота с вашей локальной машины (YOURPATH) на сервер во временную директорию (/tmp) с использованием учетных данных пользователя (USERNAME). Это необходимо для обновления файлов бота на сервере.

```bash
pscp YOURPATH USERNAME@SERVERIP:/tmp
```

* Делаем исполняемый файл

Эта команда добавляет права исполнения для файла ScheduleClassBot, что позволяет запустить его как исполняемый файл на сервере.

```bash
chmod +x ScheduleClassBot
```
* Запускаем службу

Эта команда запускает службу бота на сервере после успешного обновления. Теперь бот снова активен и готов к использованию.

```bash
systemctl start schedule-bot
```

* Проверяем корректность запуска службы

Эта команда проверяет текущий статус службы бота после запуска, чтобы удостовериться, что служба работает корректно и без ошибок.

```bash
systemctl status schedule-bot
```
