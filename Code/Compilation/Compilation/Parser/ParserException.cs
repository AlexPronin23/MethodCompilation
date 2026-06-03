using Compilation.Interpreter.Lexer.Models;

namespace Compilation.Interpreter.Parser;

public class ParserException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public ParserException(Token token, string expected)
        : base($"Синтаксическая ошибка: строка {token.Line}, символ {token.Column} — " +
               $"ожидалось {expected}, получено [{token.Type} \"{token.Value}\"]")
    {
        Line = token.Line;
        Column = token.Column;
    }

    public ParserException(Token token, string message, bool raw)
        : base($"Синтаксическая ошибка: строка {token.Line}, символ {token.Column} — {message}")
    {
        Line = token.Line;
        Column = token.Column;
    }
}
