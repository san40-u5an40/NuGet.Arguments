internal class ArgumentString : ArgumentBase
{
    private readonly string? pattern;

    internal ArgumentString(string? arg, string? pattern = null, string? errorMessage = null) : base(arg, pattern == null ? ArgumentType.StringValue : ArgumentType.StringPattern, errorMessage ?? DefaultErrorMessages.String) =>
        this.pattern = pattern;

    // Если паттерн не указан, то просто проверка на пустую строку и пробелы
    // Если указан, то на соответствие этому паттерну
    internal sealed override bool IsValid
    {
        get
        {
            if (pattern == null)
                return !string.IsNullOrWhiteSpace(Arg);
            else
                return Arg == pattern;
        }
    }
}