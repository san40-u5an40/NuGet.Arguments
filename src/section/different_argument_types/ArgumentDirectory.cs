internal class ArgumentDirectory : ArgumentBase
{
    internal ArgumentDirectory(string? arg, string? errorMessage = null) : base(arg, ArgumentType.Directory, errorMessage ?? DefaultErrorMessages.Directory) { }

    // Проверка на существование папки
    // 
    // Если путь был указан не абсолютный, папка может не существовать
    // Поэтому после первой проверки идёт вторая с добавлением к названию папки пути,
    // По которому работает приложение
    // Если и второй раз не существует,
    // То уже всё
    internal sealed override bool IsValid
    {
        get
        {
            if (Directory.Exists(Arg))
                return true;

            var dir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, Arg ?? string.Empty));
            if (dir.Exists)
            {
                Arg = dir.FullName;
                return true;
            }

            return false;
        }
    }
}