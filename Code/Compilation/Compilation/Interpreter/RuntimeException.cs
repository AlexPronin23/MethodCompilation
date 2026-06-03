namespace Compilation.Interpreter.Interpreter;

public class RuntimeException : Exception
{
    public RuntimeException(string message) : base($"Ошибка выполнения: {message}") { }
}
