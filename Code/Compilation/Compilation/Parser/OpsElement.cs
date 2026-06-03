namespace Compilation.Interpreter.Parser;

public enum OpsType
{
    TYPE_VAR,        // ссылка на переменную
    TYPE_CONST,      // ссылка на числовую константу
    TYPE_STR_CONST,  // ссылка на строковую константу
    TYPE_LABEL,      // метка (адрес перехода)
    TYPE_OP          // операция
}

public enum OpCode
{
    OP_ADD,    // +
    OP_SUB,    // -
    OP_MUL,    // *
    OP_DIV,    // /
    OP_NEG,    // унарный минус -'
    OP_ASSIGN, // :=
    OP_I,      // индексирование массива
    OP_LT,     // <
    OP_GT,     // >
    OP_LE,     // <=
    OP_GE,     // >=
    OP_EQ,     // =
    OP_NE,     // <>
    OP_JF,     // условный переход по false
    OP_J,      // безусловный переход
    OP_R,      // read
    OP_W,      // write число
    OP_WS,     // write строка
    OP_ARRAY,  // создать массив (размер)
}

public class OpsElement
{
    public OpsType Type { get; }
    public int Value { get; set; }  // индекс в таблице или OpCode

    public OpsElement(OpsType type, int value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return Type switch
        {
            OpsType.TYPE_OP    => ((OpCode)Value).ToString(),
            OpsType.TYPE_LABEL => $"L{Value}",
            _                  => $"{Type}[{Value}]"
        };
    }
}
