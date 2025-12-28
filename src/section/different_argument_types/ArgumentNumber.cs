internal class ArgumentNumber : ArgumentBase
{
    private readonly int min;
    private readonly int max;

    internal ArgumentNumber(string? arg, string? errorMessage = null) : base(arg, ArgumentType.Number, errorMessage ?? DefaultErrorMessages.Number) =>
        (this.min, this.max) = (int.MinValue, int.MaxValue);

    internal ArgumentNumber(string? arg, int max, string? errorMessage = null) : base(arg, ArgumentType.Number, errorMessage ?? DefaultErrorMessages.Number) =>
        (this.min, this.max) = (int.MinValue, max);

    internal ArgumentNumber(string? arg, int min, int max, string? errorMessage = null) : base(arg, ArgumentType.Number, errorMessage ?? DefaultErrorMessages.Number) =>
        (this.min, this.max) = (min, max);

    internal sealed override bool IsValid =>
        int.TryParse(Arg, out int num) && num >= min && num <= max;
}