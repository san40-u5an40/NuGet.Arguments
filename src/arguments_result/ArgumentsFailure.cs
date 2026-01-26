/// <summary>
/// Класс, хранящий сведенья об ошибочном вводе пользователем аргументов
/// </summary>
public class ArgumentsFailure
{
    internal ArgumentsFailure(string errorMessage, ArgumentsFailureType type) => 
        (Message, Type) = (errorMessage, type);

    /// <summary>
    /// Сведения об ошибке ввода аргументов
    /// </summary>
    public string Message { get; private init; }

    /// <summary>
    /// Тип ошибки ввода аргументов
    /// </summary>
    public ArgumentsFailureType Type { get; private init; }

    // Для упрощённого вывода ошибки в консоль
    public override string ToString() => Message;
}