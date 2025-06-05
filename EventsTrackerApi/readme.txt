----------MIGRATIONS-----------
Para agregar nuevas migrations, los pasos son los siguientes
 1- Tener el repo actualizado
 2- Usar el comando : dotnet ef migrations add AddNewNameForMigrations
 3- Una vez generado el archivo migrations, actualizamos el database -> dotnet ef database update


--------------------------------