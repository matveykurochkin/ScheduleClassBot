# Работа со службой бота
## Создание службы
```bash
sc.exe create ".NET Shedule Bot" binpath="Path to your exe" start="demand"
```
## Удаление службы
```bash
sc.exe delete ".NET Shedule Bot"
```
# Настройки бота

## Команды
```bash
start - обновление бота 
listgroup - список групп
todaypmi - расписание на сегодня ПМИ-120
todaypri - расписание на сегодня ПРИ-121
```

## Описание
```bash
Я могу показать расписание занятий таких групп: ПМИ-120 и ПРИ-121!

Для просмотра расписания необходимо выбрать группу и день недели, также я расскажу числитель или знаменатель сейчас!
```
# Команды для сборки бота
```bash
dotnet publish -c Release -r linux-x64
```
# Команды для обновления бота на сервере
## Остановка службы бота на сервере
```bash
systemctl stop schedule-bot.service
```
## Прверяем корректность остановки службы
```bash
systemctl status schedule-bot.service
```
## Копирование файлов на сервер
```bash
pscp f:\publish.zip USERNAME:/tmp
```
## Делаем исполняемый файл
```bash
chmod +x ScheduleClassBot
```
## Запускаем службу
```bash
systemctl start schedule-bot
```
## Проверяем корректность запуска службы
```bash
systemctl status schedule-bot
```
