/// <summary>
/// Результат обработки аргументов командной строки
/// </summary>
public class ResultArguments
{
    private readonly SuccessArguments? successArguments;
    private readonly FailureArguments? failureArguments;

    private ResultArguments(bool isValid, SuccessArguments? successArguments, FailureArguments? failureArguments) =>
        (this.IsValid, this.successArguments, this.failureArguments) = (isValid, successArguments, failureArguments);

    /// <summary>
    /// Валидность указанных пользователем аргументов
    /// </summary>
    public bool IsValid { get; private init; }

    /// <summary>
    /// Информация о не валидных аргументах
    /// </summary>
    public FailureArguments Error => 
        IsValid ? throw new InvalidOperationException() : failureArguments!;

    /// <summary>
    /// Информация о валидных аргументах
    /// </summary>
    public SuccessArguments Value =>
        IsValid ? successArguments! : throw new InvalidOperationException();

    internal static ResultArguments CreateSuccess(string sectionName, List<ArgumentBase> args, HashSet<string> optional, Dictionary<string, string> optionalPair) =>
        new(true, new SuccessArguments(sectionName, args, optional, optionalPair), null);

    internal static ResultArguments CreateFailure(string errorMessage, FailureArgumentsType type) =>
        new(false, null, new FailureArguments(errorMessage, type));
}