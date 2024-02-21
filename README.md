# ВКР: Разработка системы управления пароля и хранения конфеденциальных данных
Многопользовательское Web-приложение, имеющее шифрование данных и содержащее в себе функционал:
1. Менеджер паролей - хранение и управление данными учетных записей сторонних сервисов.
![Менеджер паролей](https://github.com/astarte19/KeepPasswords/assets/72278018/76057281-e2fe-4fe6-9629-8531dfa1a872)

2. Менеджер текстовых записей - редактор текстовых записей
![Меннеджер текстовых записей](https://github.com/astarte19/KeepPasswords/assets/72278018/64f30d59-fbaa-441e-8ae6-113c6f87d638)

3. Менеджер фото - хранение фотографий с возможностью просмотра в галерее
![Менеджер фото](https://github.com/astarte19/KeepPasswords/assets/72278018/c58cbe9c-5b09-4f96-942b-5dabb707f0d2)

4. Календарь событий - планировщик расписания с возмодностью управления записями по датам
![Календарь событий](https://github.com/astarte19/KeepPasswords/assets/72278018/f1b540c4-03a1-4c92-9170-f9c70820c32e)

## Технологии
**Back-end:**
- C#
- .NET Core
- ASP.NET Core MVC
- Entity Framework Core
- SQLServer/SQLite
- ASP.NET Core Identity

**Front-end:**
- JS
- HTML
- CSS
- Bootstrap
- Ajax
- Jquery

## Развертывание
1. Приложение(с SQLServer) развернуто на удаленном сервере в docker-контейнере с реурсами RAM: 250MB | CPU: 0.25 vCPU
2. Приложение(с SQLite) можно запустить, заменив параметры в файлах
- В файле appsettings.json значение параметра DefaultConnection
`{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=KeepPasswordsService.db;"

  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
`
- В файле Program.cs options.UseSqlite на options.UseSqlServer
`
builder.Services.AddDbContext<ApplicationContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
`
## Демо
Приложение доступно по ссылке [a24725-6a37.u.d-f.pw](https://a24725-6a37.u.d-f.pw)
