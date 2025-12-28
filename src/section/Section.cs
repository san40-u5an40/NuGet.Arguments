namespace san40_u5an40.Arguments;

public class Section
{
    private List<string> args;

    internal Section(string[] args) =>
        this.args = new List<string>(args);

    internal List<ArgumentBase> Binding { get; } = [];
    internal HashSet<string> Optional { get; } = [];
    internal Dictionary<string, (Predicate<string> IsValid, string ErrorMessage)> OptionalPair { get; } = [];

    internal List<string> RemainderArguments => args;

    /// <summary>
    /// Добавление строкового аргумента с конкретным паттерном
    /// </summary>
    /// <param name="pattern">Строковое значение, запрашиваемое в аргументах</param>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddStringPattern(string pattern, string? errorMessage = null)
    {
        ThrowIfContainsOptional();
        var arg = GetArgument();
        Binding.Add(new ArgumentString(arg, pattern, errorMessage));

        return this;
    }

    /// <summary>
    /// Добавление произвольного строкового аргумента
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddStringValue(string? errorMessage = null)
    {
        ThrowIfContainsOptional();
        var arg = GetArgument();
        Binding.Add(new ArgumentString(arg, null, errorMessage));

        return this;
    }

    /// <summary>
    /// Добавление числового аргумента
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddNumber(string? errorMessage = null)
    {
        ThrowIfContainsOptional();
        var arg = GetArgument();
        Binding.Add(new ArgumentNumber(arg, errorMessage));

        return this;
    }

    /// <summary>
    /// Добавление числового аргумента с указанием максимального значения
    /// </summary>
    /// <param name="max">Максимальное значение</param>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddNumber(int max, string? errorMessage = null)
    {
        ThrowIfContainsOptional();
        var arg = GetArgument();
        Binding.Add(new ArgumentNumber(arg, max, errorMessage));

        return this;
    }

    /// <summary>
    /// Добавление числового аргумента с указанием минимального и максимального значения
    /// </summary>
    /// <param name="min">Минимальное значение</param>
    /// <param name="max">Максимальное значение</param>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddNumber(int min, int max, string? errorMessage = null)
    {
        ThrowIfContainsOptional();
        var arg = GetArgument();
        Binding.Add(new ArgumentNumber(arg, min, max, errorMessage));

        return this;
    }

    /// <summary>
    /// Добавление аргумента в виде расположения
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddDirectory(string? errorMessage = null)
    {
        ThrowIfContainsOptional();
        var arg = GetArgument();
        Binding.Add(new ArgumentDirectory(arg, errorMessage));

        return this;
    }

    /// <summary>
    /// Добавление аргумента в виде файла
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddFile(string? errorMessage = null)
    {
        ThrowIfContainsOptional();
        var arg = GetArgument();
        Binding.Add(new ArgumentFile(arg, errorMessage));

        return this;
    }

    /// <summary>
    /// Добавление аргумента с гибкой логикой проверки
    /// </summary>
    /// <param name="predicate">Делегат для обработки значения, введённого пользователем</param>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddCustom(Predicate<string> predicate, string errorMessage)
    {
        ThrowIfContainsOptional();
        var arg = GetArgument();
        Binding.Add(new ArgumentCustom(arg, predicate, errorMessage));

        return this;
    }

    private void ThrowIfContainsOptional()
    {
        if (Optional.Count > 0 || OptionalPair.Count > 0)
            throw new InvalidOperationException("Опциональные аргументы могут быть расположены только после обязательных");
    }

    private string? GetArgument()
    {
        string? arg = null;

        if (args.Count > 0)
        {
            arg = args[0];
            args.RemoveAt(0);
        }

        return arg;
    }

    /// <summary>
    /// Добавление вариативного аргумента
    /// </summary>
    /// <param name="pattern">Строковый паттерн аргумента</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddOptional(string pattern)
    {
        Optional.Add(pattern);
        return this;
    }

    /// <summary>
    /// Добавление вариативного парного аргумента
    /// </summary>
    /// <param name="pattern">Строковый паттерн аргумента</param>
    /// <param name="predicate">Значение, введённое пользователем</param>
    /// <returns>Тот же экземпляр секции (fluent API)</returns>
    public Section AddOptionalPair(string pattern, Predicate<string> predicate, string errorMessage)
    {
        OptionalPair.Add(pattern, (predicate, errorMessage));
        return this;
    }
}