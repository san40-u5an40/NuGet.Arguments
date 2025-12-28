namespace san40_u5an40.Arguments;

/// <summary>
/// Тип ошибки ввода аргументов
/// </summary>
public enum FailureArgumentsType : ushort
{
    Zero                 = 0, // Отсутствие всех аргументов
    Section              = 1, // Указана неверная секция/команда
    Lack                 = 2, // Отсутствие одного из обязательных аргументов
    BindingMistake       = 3, // Пользовательская ошибка в обязательных аргументах
    OptionalPairMistake  = 4, // Пользовательская ошибка в парных опциональных аргументах
    Excess               = 5, // Указан лишний аргумента
}