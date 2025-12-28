internal class ArgumentFile : ArgumentBase
{
    internal ArgumentFile(string? arg, string? errorMessage = null) : base(arg, ArgumentType.File, errorMessage ?? DefaultErrorMessages.File) { }

    // Проверка на существование файла
    // 
    // Если путь был указан не абсолютный, файл может не существовать
    // Поэтому после первой проверки идёт вторая с добавлением к названию файла пути,
    // По которому работает приложение
    // Если и второй раз не существует,
    // То уже всё
    internal sealed override bool IsValid
    {
        get
        {
            if (File.Exists(Arg))
                return true;

            var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, Arg ?? string.Empty));
            if (file.Exists)
            {
                Arg = file.FullName;
                return true;
            }

            return false;
        }
    }
}