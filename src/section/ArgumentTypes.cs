internal enum ArgumentType
{
    StringPattern,  // Определённая строка
    StringValue,    // Любые строковые данные
    Number,         // Число
    Directory,      // Расположение
    File,           // Файл
    Custom          // Тип с пользовательской проверкой валидности
}