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
tomorrowpmi - расписание на завтра ПМИ-120
sessionpmi - расписание сессии ПМИ-120
todaypri - расписание на сегодня ПРИ-121
tomorrowpri - расписание на завтра ПРИ-121
sessionpri - расписание сессии ПРИ-121
```

## Описание
```bash
Я могу показать расписание занятий таких групп: ПМИ-120 и ПРИ-121!

Для просмотра расписания необходимо выбрать группу и день недели, также я расскажу числитель или знаменатель сейчас!
```
# Команды для сборки бота
## Команда для сборки под Linux 
```bash
dotnet publish -c Release -r linux-x64
```
## Команда для сборки под Windows 
```bash
dotnet publish -c Release -r win-x64
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
pscp YOURPATH USERNAME:/tmp
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
# Скрипт для автоматического обновления и запуска бота на сервере
```bash
#!/bin/bash

CurrDir=$(pwd)

echo stop service
systemctl stop schedule-bot.service

echo clean
cd /opt/src/ScheduleClassBot
git clean -ffxd
git reset --hard HEAD
git pull

echo build
dotnet publish /opt/src/ScheduleClassBot/ScheduleClassBot/ScheduleClassBot/ScheduleClassBot.csproj -c Release -r linux-x64 --self-contained

echo bakup
mkdir -p /opt/schedule-bot/bak
cp /opt/schedule-bot/linux-x64/ListUsers.txt /opt/schedule-bot/bak/ListUsers.txt
cp /opt/schedule-bot/linux-x64/nlog.config /opt/schedule-bot/bak/nlog.config
cp /opt/schedule-bot/linux-x64/appsettings.json /opt/schedule-bot/bak/appsettings.json

echo copy
rm -rf /opt/schedule-bot/linux-x64
mkdir -p /opt/schedule-bot/linux-x64
cp -R  /opt/src/ScheduleClassBot/ScheduleClassBot/ScheduleClassBot/bin/Release/net7.0/linux-x64/publish/* /opt/schedule-bot/linux-x64

echo restore from bakup
cp /opt/schedule-bot/bak/ListUsers.txt /opt/schedule-bot/linux-x64/ListUsers.txt
cp /opt/schedule-bot/bak/nlog.config /opt/schedule-bot/linux-x64/nlog.config
cp /opt/schedule-bot/bak/appsettings.json /opt/schedule-bot/linux-x64/appsettings.json
rm -rf /opt/schedule-bot/bak

echo run service
systemctl start schedule-bot.service

echo check service
systemctl status schedule-bot.service

cd $CurrDir
```