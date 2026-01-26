/// <summary>
/// Класс, хранящий сведения об успешном вводе пользователем аргументов командной строки
/// </summary>
public class ArgumentsSuccess : IEnumerable<string>
{
    private readonly List<ArgumentBase> args;
    private readonly HashSet<string> optional;
    private readonly Dictionary<string, string> optionalPair;

    internal ArgumentsSuccess(string sectionName, List<ArgumentBase> args, HashSet<string> optional, Dictionary<string, string> optionalPair) =>
        (this.Section, this.args, this.optional, this.optionalPair) = (sectionName, args, optional, optionalPair);

    /// <summary>
    /// Секция/команда указанная пользователем
    /// </summary>
    public string Section { get; private init; }

    /// <summary>
    /// Обращение к введённым пользователем аргументам через индексатор
    /// </summary>
    /// <param name="index">Индекс аргумента</param>
    /// <returns>Значение аргумента</returns>
    /// <exception cref="ArgumentOutOfRangeException">Обращение к аргументу, который не был добавлен в секцию</exception>
    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= args.Count)
                throw new ArgumentOutOfRangeException($"В сценарии \"{Section}\" было указано только {args.Count} обязательных аргументов. Обращение по индексу {index} не возможно.");
            else
                return args[index].Arg!;
        }
    }

    /// <summary>
    /// Перебираемая коллекция аргументов
    /// </summary>
    /// <returns>Перечень аргументов, которые успешно прошли проверку</returns>
    public IEnumerator<string> GetEnumerator() =>
        args.Select(p => p.Arg).GetEnumerator()!;

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    /// <summary>
    /// Проверка на наличие опциональных аргументов
    /// </summary>
    /// <param name="pattern">Значение необязательного аргумента</param>
    /// <returns>Наличие необязательного аргумента</returns>
    public bool Contains(string pattern) => optional.Contains(pattern);

    /// <summary>
    /// Проверка на наличие опциональных парных аргументов
    /// </summary>
    /// <param name="pattern">Первое значение парного аргумента</param>
    /// <param name="value">Второе значение парного аргумента, которое ввёл пользователь (null при отсутствии)</param>
    /// <returns>Наличие необязательного парного аргумента</returns>
    public bool ContainsPair(string pattern, out string? value) =>
        optionalPair.TryGetValue(pattern, out value);
}