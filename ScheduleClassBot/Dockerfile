# Используем официальный образ для .NET 8 SDK для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Указываем рабочую директорию внутри контейнера
WORKDIR /app

# Копируем файлы проекта в рабочую директорию контейнера
COPY . .

# Восстанавливаем зависимости
RUN dotnet restore

# Собираем приложение
RUN dotnet publish -c Release -o /app/publish

# Используем более легкий образ для работы приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Указываем рабочую директорию внутри контейнера
WORKDIR /app

# Копируем опубликованные файлы из предыдущего этапа
COPY --from=build /app/publish .

# Открываем порт, который будет использовать приложение
EXPOSE 80

# Указываем команду для запуска приложения
ENTRYPOINT ["dotnet", "ScheduleClassBot.dll"]
