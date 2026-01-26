/// <summary>
/// Результат обработки аргументов командной строки
/// </summary>
public class ArgumentsResult
{
    private readonly ArgumentsSuccess? successArguments;
    private readonly ArgumentsFailure? failureArguments;

    private ArgumentsResult(bool isValid, ArgumentsSuccess? successArguments, ArgumentsFailure? failureArguments) =>
        (this.IsValid, this.successArguments, this.failureArguments) = (isValid, successArguments, failureArguments);

    /// <summary>
    /// Валидность указанных пользователем аргументов
    /// </summary>
    public bool IsValid { get; private init; }

    /// <summary>
    /// Информация о не валидных аргументах
    /// </summary>
    public ArgumentsFailure Error => 
        IsValid ? throw new InvalidOperationException() : failureArguments!;

    /// <summary>
    /// Информация о валидных аргументах
    /// </summary>
    public ArgumentsSuccess Value =>
        IsValid ? successArguments! : throw new InvalidOperationException();

    internal static ArgumentsResult CreateSuccess(string sectionName, List<ArgumentBase> args, HashSet<string> optional, Dictionary<string, string> optionalPair) =>
        new(true, new ArgumentsSuccess(sectionName, args, optional, optionalPair), null);

    internal static ArgumentsResult CreateFailure(string errorMessage, ArgumentsFailureType type) =>
        new(false, null, new ArgumentsFailure(errorMessage, type));
}