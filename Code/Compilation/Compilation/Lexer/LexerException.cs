namespace Compilation.Interpreter.Lexer;

public class LexerException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public LexerException(int line, int column, char ch, string? extra = null)
        : base(BuildMessage(line, column, ch, extra))
    {
        Line = line;
        Column = column;
    }

    private static string BuildMessage(int line, int col, char ch, string? extra)
    {
        string sym = ch == '\0' ? "EOF" : $"'{ch}'";
        string msg = $"Лексическая ошибка: строка {line}, символ {col} — недопустимый символ {sym}";
        if (extra != null) msg += $". {extra}";
        return msg;
    }
}
