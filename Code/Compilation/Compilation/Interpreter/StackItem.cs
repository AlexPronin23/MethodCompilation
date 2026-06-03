namespace Compilation.Interpreter.Interpreter;

/// <summary>Вид элемента в магазине интерпретатора.</summary>
public enum StackKind
{
    IntValue,      // числовое значение
    VarRef,        // ссылка на переменную (индекс в VarTable)
    ArrayRef,      // ссылка на массив (индекс-переменная — сам массив хранится в среде)
    ArrayElemRef,  // ссылка на элемент массива: (arrayName, index)
    StrValue,      // строковое значение (результат вычисления)
}

public class StackItem
{
    public StackKind Kind { get; }
    public int IntVal { get; }
    public string StrVal { get; }
    public int ArrIndex  { get; }   // для ArrayElemRef — индекс элемента

    public static StackItem FromInt(int v) =>
        new(StackKind.IntValue, v, "", 0);
    public static StackItem FromStr(string s) =>
        new(StackKind.StrValue, 0, s, 0);
    public static StackItem VarReference(int varIdx) =>
        new(StackKind.VarRef, varIdx, "", 0);
    public static StackItem ArrayReference(int varIdx) =>
        new(StackKind.ArrayRef, varIdx, "", 0);
    public static StackItem ArrayElemReference(int arrVarIdx, int elemIdx) =>
        new(StackKind.ArrayElemRef, arrVarIdx, "", elemIdx);

    private StackItem(StackKind kind, int intVal, string strVal, int arrIndex)
    {
        Kind = kind;
        IntVal = intVal;
        StrVal = strVal;
        ArrIndex = arrIndex;
    }

    public override string ToString() => Kind switch
    {
        StackKind.IntValue      => IntVal.ToString(),
        StackKind.StrValue      => $"\"{StrVal}\"",
        StackKind.VarRef        => $"VarRef[{IntVal}]",
        StackKind.ArrayRef      => $"ArrRef[{IntVal}]",
        StackKind.ArrayElemRef  => $"ArrElem[{IntVal}][{ArrIndex}]",
        _                       => "?"
    };
}
