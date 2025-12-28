internal class ArgumentCustom : ArgumentBase
{
    private readonly Predicate<string> predicate;

    internal ArgumentCustom(string? arg, Predicate<string> predicate, string errorMessage) : base(arg, ArgumentType.Custom, errorMessage) =>
        this.predicate = predicate;

    internal override bool IsValid =>
        Arg == null ? false : predicate(Arg);
}