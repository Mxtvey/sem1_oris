# sem1_oris



# miniHttpServer



---

## Запуск проекта локально

### 1. Клонирование репозитория

git clone https://github.com/Mxtvey/sem1_oris


### 2. Восстановление зависимостей

dotnet restore

### 3. Сборка проекта

dotnet build

### 4. Запуск сервера

dotnet run

После запуска сервер будет доступен по адресу:

http://localhost:1235/tour

> Порт задаётся в settings.json или в коде сервера.

---

## Конфигурация

Файл настроек находится по пути:

Settings/settings.json

Пример содержимого:

{
  "Host": "http://localhost",
  "Port": 1235
}

Важно: папка Settings должна находиться рядом с исполняемым файлом.

---

##  Запуск через Docker

### 1. Сборка контейнера

docker build -t httpserver .

### 2. Запуск контейнера

docker run -p 1235:1235 httpserver

---

## Запуск через Docker Compose

### docker-compose.yml

services:
  api:
    build: .
    ports:
      - "1235:1235"

### Запуск

docker compose up --build

---



---


Если не получится через докер файл, можно востановить базу данных через pgAdmin:

1.Создаём новую пустую бд
2.Кликаем правой кнопкой мыши на неё и жмём restore

<img width="1025" height="780" alt="image" src="https://github.com/user-attachments/assets/7b971973-9189-496a-b034-6941c6a33641" />


3.в filename выбираем файл semester_db.backup и жмём restore
