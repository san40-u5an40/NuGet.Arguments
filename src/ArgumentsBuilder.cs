namespace san40_u5an40.Arguments;

/// <summary>
/// Класс для создания обработчика аргументов командной строки
/// </summary>
public class ArgumentsBuilder
{
    private const string SINGLE = "single";

    private readonly string[] args;
    private Dictionary<string, Section> sections = [];
    private bool isSingleSectionMode = false;

    private bool isBuilded = false;

    /// <summary>
    /// Конструктор создания обработчика аргументов командной строки
    /// </summary>
    /// <param name="args">Аргументы командной строки</param>
    public ArgumentsBuilder(string[] args) =>
        this.args = args;

    /// <summary>
    /// Конструктор создания обработчика аргументов командной строки
    /// </summary>
    public ArgumentsBuilder() =>
        this.args = Environment.GetCommandLineArgs()[1..];

    /// <summary>
    /// Добавление секции/команды
    /// </summary>
    /// <param name="name">Название секции или значение команды</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    /// <exception cref="InvalidOperationException">Добавление секции при работе в режиме одиночной секции</exception>
    /// <exception cref="ArgumentException">Добавление секции с именем, которое уже существует</exception>
    public ArgumentsBuilder AddSection(string name)
    {
        if (isSingleSectionMode)
            throw new InvalidOperationException("При работе в режиме одиночной секции создание новых не поддерживается. Необходимо выбрать только один режим работы.");

        if (sections.ContainsKey(name))
            throw new ArgumentException("Секция с именем \"{name}\" уже существует.");

        var section = new Section(args)
            .AddStringPattern(name, string.Empty);

        sections.Add(name, section);
        return this;
    }

    /// <summary>
    /// Индексатор для обращения к созданным секциям/командам
    /// </summary>
    /// <param name="sectionName">Значение секции/команды</param>
    /// <returns>Созданная секция/команда</returns>
    /// <exception cref="ArgumentException">Обращение с несуществующей секции/команде</exception>
    public Section this[string sectionName]
    {
        get
        {
            if (sections.TryGetValue(sectionName, out Section? section))
                return section;
            else
                throw new ArgumentException($"Запрашиваемая секция с названием \"{sectionName}\" не была найдена. Прежде чем обратиться к секции, необходимо её создать.");
        }
    }

    /// <summary>
    /// Основной метод обработки полученных значений командной строки
    /// </summary>
    /// <returns>
    /// Возвращает ResultArguments
    /// </returns>
    /// <exception cref="InvalidOperationException">Обработчик уже был собран, или отсутствуют сценарии для обработки аргументов командной строки</exception>
    /// <exception cref="FormatException">Не уникальность опциональных паттернов или наличие секций, в которых есть опциональные паттерны, и которые заканчиваются на обязательном аргументе произвольного текста (такой кейс может вызвать неоднозначность)</exception>
    public ArgumentsResult Build()
    {
        // Сначала идут стандартные проверки:
        // Compile-Time Exceptions
        // На наличие аргументов

        ThrowIfInvalid();

        isBuilded = true;

        if (args.Length == 0)
            return ArgumentsResult.CreateFailure("Аргументы не были указаны", ArgumentsFailureType.Zero);

        // Затем определяется секция и её имя
        // Если это режим одиночной секции, то имя одно и уже известно
        // Если режим нескольких секций, то название берётся из первого аргумента, и проверяется на существование
        // Если его нет, то возвращается соответствующий FailureArguments

        string sectionName;
        Section? section;
        
        if (isSingleSectionMode)
            (sectionName, section) = (SINGLE, sections[SINGLE]);
        else
        {
            sectionName = args[0];

            if (!sections.TryGetValue(sectionName, out section))
                return ArgumentsResult.CreateFailure($"Сценарий для '{sectionName}' не был найден", ArgumentsFailureType.Section);

            section.Binding.RemoveAt(0);
        }

        // Проверка обязательных аргументов:
        // 
        // Сначала проверяется хватает ли аргументов
        // Затем их валидность
        // Если они не валидны, возвращается информация о первой ошибке

        var bindings = section.Binding;
        if (bindings.Any(p => p.Arg == null))
            return ArgumentsResult.CreateFailure("Отсутствуют некоторые обязательные аргументы", ArgumentsFailureType.Lack);
        if (bindings.Any(p => p.IsValid == false))
            return ArgumentsResult.CreateFailure(section.Binding.First(p => p.IsValid == false).ErrorMessage, ArgumentsFailureType.BindingMistake);

        // Проверка опциональных пар
        // 
        // Сначала в целом проверяется содержат ли опциональные пары перебираемый паттерн (аргумент из числа оставшихся)
        // После чего уже проверяется валидность, если следующий аргумент не проходит проверку по предикату, возвращается соответствующий результат
        // Ну и если всё валидно, то найденная пара добавляется во временную коллекцию, а перебираемые аргументы удаляются из списка оставшихся аргументов

        var remainderArguments = section.RemainderArguments;
        var tempOptionalPair = new Dictionary<string, string>();

        for (int i = 0; i < remainderArguments.Count - 1; i++)
        {
            var optionalPair = section.OptionalPair;

            string currentArgument = remainderArguments[i];
            string nextArgument = remainderArguments[i + 1];

            if (!optionalPair.ContainsKey(currentArgument))
                continue;

            if (!optionalPair[currentArgument].IsValid(nextArgument))
            {
                string errorMessage = FormatErrorIfValid(optionalPair[currentArgument].ErrorMessage, nextArgument);
                return ArgumentsResult.CreateFailure(errorMessage, ArgumentsFailureType.OptionalPairMistake);
            }
            
            tempOptionalPair.Add(currentArgument, nextArgument);
            remainderArguments.RemoveAt(i + 1);
            remainderArguments.RemoveAt(i);
            i--;
        }

        // Проверка опциональных аргументов
        // 
        // Пересекаются список оставшихся аргументов и опциональных (добавленных разработчиком)
        // И проверяем остаток в аргументах, удаляя оттуда все опциональные
        // Если их не осталось, то всё гуд, возвращаем валидный результат
        // Если не гуд, возвращаем ошибку, с указанием лишних аргументов

        var tempOptional = remainderArguments.Intersect(section.Optional).ToHashSet();

        remainderArguments = remainderArguments.Except(tempOptional).ToList();
        if (remainderArguments.Count == 0)
            return ArgumentsResult.CreateSuccess(sectionName, section.Binding, tempOptional, tempOptionalPair);
        else
            return ArgumentsResult.CreateFailure($"Указаны лишние аргументы: '{string.Join("', '", remainderArguments)}'", ArgumentsFailureType.Excess);
    }

    // Стандартные проверки:
    // - Был ли уже собран обработчик
    // - На наличие созданных сценариев
    // - На наличие секций, где пересекаются паттерны опциональных аргументов с паттернами опциональных пар
    // - На корректность сценариев (по признаку окончания на произвольное строковое значение и наличие опциональных аргументов, такой кейс может вызвать неоднозначность)
    private void ThrowIfInvalid()
    {
        if (isBuilded)
            throw new InvalidOperationException("Данный обработчик аргументов уже был собран. Допускается только одноразовая сборка.");

        if (sections.Count == 0)
            throw new InvalidOperationException("Для корректной работы \"ArgumentsBuilder.Build()\" необходимо создать сценарии. Со справочными сведениями можно ознакомиться по ссылке: https://github.com/san40-u5an40/NuGet.Arguments");

        if (sections.Values.Any(p => p.OptionalPair.Keys.Intersect(p.Optional).Any()))
            throw new FormatException("Для корректной работы \"ArgumentsBuilder.Build()\" не должно быть пересечений паттернов в опциональных парах и просто опциональных аргументах.");

        if (sections.Values.Where(p => p.Binding.LastOrDefault()?.Type == ArgumentType.StringValue).Any(p => p.OptionalPair.Count > 0 || p.Optional.Count > 0))
            throw new FormatException(
                "В сценариях ArgumentsBuilder есть случаи, которые заканчиваются на вводе произвольного строкового значения, и имеют опциональные аргументы. " +
                "Это может привести к некорректной работе библиотеки. " +
                "Чтобы устранить эту проблему либо поставьте последним аргументом иной тип, либо уберите из этих сценариев опциональные значения.");
    }

    // Интерполяция с помощью string.Format, если строка для этого подходит
    private string FormatErrorIfValid(string errorMessage, string argument)
    {
        if (Regex.IsMatch(errorMessage, @"^[^{}]*\{0\}[^{}]*$", RegexOptions.Compiled))
            return string.Format(errorMessage, argument);

        return errorMessage;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //                                       Последующие методы необходимы для поддержки режима одиночной секции                                       //
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Добавление произвольного строкового аргумента
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddStringValue(string? errorMessage = null) =>
        AddArgument(p => p.AddStringValue(errorMessage));

    /// <summary>
    /// Добавление строкового аргумента с конкретным паттерном
    /// </summary>
    /// <param name="pattern">Строковое значение, запрашиваемое в аргументах</param>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddStringPattern(string pattern, string? errorMessage = null) =>
        AddArgument(p => p.AddStringPattern(pattern, errorMessage));

    /// <summary>
    /// Добавление числового аргумента
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddNumber(string? errorMessage = null) =>
        AddArgument(p => p.AddNumber(errorMessage));

    /// <summary>
    /// Добавление числового аргумента с указанием максимального значения
    /// </summary>
    /// <param name="max">Максимальное значение</param>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddNumber(int max, string? errorMessage = null) =>
        AddArgument(p => p.AddNumber(max, errorMessage));

    /// <summary>
    /// Добавление числового аргумента с указанием минимального и максимального значения
    /// </summary>
    /// <param name="min">Минимальное значение</param>
    /// <param name="max">Максимальное значение</param>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddNumber(int min, int max, string? errorMessage = null) =>
        AddArgument(p => p.AddNumber(min, max, errorMessage));

    /// <summary>
    /// Добавление аргумента в виде расположения
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddDirectory(string? errorMessage = null) =>
        AddArgument(p => p.AddDirectory(errorMessage));

    /// <summary>
    /// Добавление аргумента в виде файла
    /// </summary>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddFile(string? errorMessage = null) =>
        AddArgument(p => p.AddFile(errorMessage));

    /// <summary>
    /// Добавление аргумента с гибкой логикой проверки
    /// </summary>
    /// <param name="predicate">Делегат для обработки значения, введённого пользователем</param>
    /// <param name="errorMessage">Сообщение об ошибке, возвращаемое при неверном вводе аргумента</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddCustom(Predicate<string> predicate, string errorMessage) =>
        AddArgument(p => p.AddCustom(predicate, errorMessage));

    /// <summary>
    /// Добавление вариативного аргумента
    /// </summary>
    /// <param name="pattern">Строковый паттерн аргумента</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddOptional(string pattern) =>
        AddArgument(p => p.AddOptional(pattern));

    /// <summary>
    /// Добавление вариативного парного аргумента
    /// </summary>
    /// <param name="pattern">Строковый паттерн аргумента</param>
    /// <param name="predicate">Значение, введённое пользователем</param>
    /// <returns>Тот же экземпляр ArgumentsBuilder</returns>
    public ArgumentsBuilder AddOptionalPair(string pattern, Predicate<string> predicate, string errorMessage) =>
        AddArgument(p => p.AddOptionalPair(pattern, predicate, errorMessage));

    // При обращении к методам добавления аргументов по объекту ArgumentsBuilder
    // Он проверяет наличие других секций
    // И в случае их отсутствия включает режим одиночной секции
    // Иначе выдает ошибку, т.к. при создании секций необходимо с ними работать через индексатор
    private ArgumentsBuilder AddArgument(Action<Section> sectionOperation)
    {
        if (!isSingleSectionMode && sections.Count == 0)
        {
            sections.Add(SINGLE, new Section(args));
            isSingleSectionMode = true;
        }

        if (isSingleSectionMode)
            sectionOperation(sections[SINGLE]);

        if (!isSingleSectionMode)
            throw new InvalidOperationException("При создании секций работа в режиме одной секции не поддерживается. Необходимо выбрать только один режим работы.");

        return this;
    }
}