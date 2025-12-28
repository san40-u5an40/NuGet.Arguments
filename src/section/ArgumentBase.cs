// Основа для создания аргументов разных типов
internal abstract class ArgumentBase
{
    internal ArgumentBase(string? arg, ArgumentType type, string errorMessage) =>
        (this.Arg, this.Type, this.ErrorMessage) = (arg, type, errorMessage);

    internal abstract bool IsValid { get; }
    
    internal string? Arg { get; private protected set; }
    internal ArgumentType Type { get; private init; }

    // Свойство, хранящее информацию об ошибке ввода аргумента
    // 
    // Если аргумент отсутствует ( == null ), то возвращается стандартная ошибка для этого кейса
    // Если он есть, и с признаками форматирования, то в него интерполируется аргумент
    // Если он просто есть, то просто в таком виде и возвращается
    internal virtual string ErrorMessage
    {
        get
        {
            if (Arg == null)
                return DefaultErrorMessages.Void;

            if (Regex.IsMatch(field, @"^[^{}]*\{0\}[^{}]*$", RegexOptions.Compiled))
                return string.Format(field, Arg);

            return field;
        }
        private init;
    }
}