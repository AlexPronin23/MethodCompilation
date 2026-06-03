
using Compilation.Interpreter.Parser;

namespace Compilation.Interpreter.Interpreter;

/// <summary>
/// Интерпретатор ОПС.
/// Выполняет линейный массив OpsElement, используя стек вычислений.
/// </summary>
public class Interpreter
{
    private readonly List<OpsElement> _ops;
    private readonly List<string>     _varTable;
    private readonly List<int>        _constTable;
    private readonly List<string>     _strTable;

    // Среда выполнения
    private readonly int[]     _vars;        // значения переменных
    private readonly int[][]   _arrays;      // массивы
    private readonly bool[]    _isArray;     // переменная — массив?
    private readonly bool[]    _varInited;   // переменная инициализирована?

    private readonly Stack<StackItem> _stack = new();

    public Interpreter(
        List<OpsElement> ops,
        List<string>     varTable,
        List<int>        constTable,
        List<string>     strTable)
    {
        _ops        = ops;
        _varTable   = varTable;
        _constTable = constTable;
        _strTable   = strTable;

        int n = varTable.Count;
        _vars      = new int[n];
        _arrays    = new int[n][];
        _isArray   = new bool[n];
        _varInited = new bool[n];
    }

    public void Run()
    {
        int pc = 0;
        while (pc < _ops.Count)
        {
            var elem = _ops[pc];

            switch (elem.Type)
            {
                case OpsType.TYPE_VAR:
                    _stack.Push(StackItem.VarReference(elem.Value));
                    pc++;
                    break;

                case OpsType.TYPE_CONST:
                    _stack.Push(StackItem.FromInt(_constTable[elem.Value]));
                    pc++;
                    break;

                case OpsType.TYPE_STR_CONST:
                    _stack.Push(StackItem.FromStr(_strTable[elem.Value]));
                    pc++;
                    break;

                case OpsType.TYPE_LABEL:
                    _stack.Push(StackItem.FromInt(elem.Value));
                    pc++;
                    break;

                case OpsType.TYPE_OP:
                    pc = ExecOp((OpCode)elem.Value, pc);
                    break;

                default:
                    throw new RuntimeException($"Неизвестный тип элемента ОПС: {elem.Type}");
            }
        }
    }

    // ─── выполнение операции ──────────────────────────────────────────────────

    private int ExecOp(OpCode op, int pc)
    {
        switch (op)
        {
            // Арифметика
            case OpCode.OP_ADD: { var b = PopInt(); var a = PopInt(); Push(a + b); break; }
            case OpCode.OP_SUB: { var b = PopInt(); var a = PopInt(); Push(a - b); break; }
            case OpCode.OP_MUL: { var b = PopInt(); var a = PopInt(); Push(a * b); break; }
            case OpCode.OP_DIV:
            {
                var b = PopInt();
                var a = PopInt();
                if (b == 0) throw new RuntimeException("Деление на ноль");
                Push(a / b);
                break;
            }
            case OpCode.OP_NEG: { var a = PopInt(); Push(-a); break; }

            // Сравнение
            case OpCode.OP_LT: { var b = PopInt(); var a = PopInt(); Push(a < b  ? 1 : 0); break; }
            case OpCode.OP_GT: { var b = PopInt(); var a = PopInt(); Push(a > b  ? 1 : 0); break; }
            case OpCode.OP_LE: { var b = PopInt(); var a = PopInt(); Push(a <= b ? 1 : 0); break; }
            case OpCode.OP_GE: { var b = PopInt(); var a = PopInt(); Push(a >= b ? 1 : 0); break; }
            case OpCode.OP_EQ: { var b = PopInt(); var a = PopInt(); Push(a == b ? 1 : 0); break; }
            case OpCode.OP_NE: { var b = PopInt(); var a = PopInt(); Push(a != b ? 1 : 0); break; }

            // Переходы
            case OpCode.OP_JF:
            {
                int addr  = PopInt();  // метка
                int cond  = PopInt();  // условие
                if (cond == 0) return addr;
                break;
            }
            case OpCode.OP_J:
            {
                int addr = PopInt();
                return addr;
            }

            // Присваивание
            case OpCode.OP_ASSIGN:
            {
                int rval = PopInt();
                var target = _stack.Pop();
                StoreValue(target, rval);
                break;
            }

            // Индексирование массива
            case OpCode.OP_I:
            {
                int idx     = PopInt();
                var arrItem = _stack.Pop();
                int varIdx  = ResolveVarIdx(arrItem);
                _stack.Push(StackItem.ArrayElemReference(varIdx, idx));
                break;
            }

            // Объявление массива: varRef, size → создать массив
            case OpCode.OP_ARRAY:
            {
                int size   = PopInt();
                var vRef   = _stack.Pop();
                int varIdx = ResolveVarIdx(vRef);
                _arrays[varIdx]  = new int[size];
                _isArray[varIdx] = true;
                _varInited[varIdx] = true;
                break;
            }

            // Ввод
            case OpCode.OP_R:
            {
                var target = _stack.Pop();
                string? line = Console.ReadLine();
                if (!int.TryParse(line?.Trim(), out int val))
                    throw new RuntimeException($"Ожидалось целое число, введено: \"{line}\"");
                StoreValue(target, val);
                break;
            }

            // Вывод числа
            case OpCode.OP_W:
            {
                // Может быть число или строка (если write("str") попало сюда)
                var item = _stack.Pop();
                if (item.Kind == StackKind.StrValue)
                    Console.WriteLine(item.StrVal);
                else
                    Console.WriteLine(GetInt(item));
                break;
            }

            // Вывод строки
            case OpCode.OP_WS:
            {
                var item = _stack.Pop();
                if (item.Kind == StackKind.StrValue)
                    Console.WriteLine(item.StrVal);
                else
                    Console.WriteLine(GetInt(item).ToString());
                break;
            }

            default:
                throw new RuntimeException($"Неизвестная операция ОПС: {op}");
        }

        return pc + 1;
    }

    // ─── вспомогательные методы ───────────────────────────────────────────────

    private void Push(int v) => _stack.Push(StackItem.FromInt(v));

    private int PopInt()
    {
        var item = _stack.Pop();
        return GetInt(item);
    }

    private int GetInt(StackItem item)
    {
        return item.Kind switch
        {
            StackKind.IntValue => item.IntVal,
            StackKind.VarRef   => GetVarValue(item.IntVal),
            StackKind.ArrayElemRef => GetArrayElem(item.IntVal, item.ArrIndex),
            _ => throw new RuntimeException($"Ожидалось числовое значение, получено {item}")
        };
    }

    private int GetVarValue(int varIdx)
    {
        if (!_varInited[varIdx])
            throw new RuntimeException($"Переменная '{_varTable[varIdx]}' не инициализирована");
        if (_isArray[varIdx])
            throw new RuntimeException($"'{_varTable[varIdx]}' — массив, ожидалась скалярная переменная");
        return _vars[varIdx];
    }

    private int GetArrayElem(int varIdx, int idx)
    {
        EnsureArray(varIdx);
        var arr = _arrays[varIdx];
        if (idx < 0 || idx >= arr.Length)
            throw new RuntimeException($"Индекс {idx} вне границ массива '{_varTable[varIdx]}' [0..{arr.Length-1}]");
        return arr[idx];
    }

    private void StoreValue(StackItem target, int val)
    {
        switch (target.Kind)
        {
            case StackKind.VarRef:
            {
                int varIdx = target.IntVal;
                if (_isArray[varIdx])
                    throw new RuntimeException($"'{_varTable[varIdx]}' — массив, нельзя присвоить скалярное значение");
                _vars[varIdx]      = val;
                _varInited[varIdx] = true;
                break;
            }
            case StackKind.ArrayElemRef:
            {
                EnsureArray(target.IntVal);
                var arr = _arrays[target.IntVal];
                if (target.ArrIndex < 0 || target.ArrIndex >= arr.Length)
                    throw new RuntimeException($"Индекс {target.ArrIndex} вне границ массива '{_varTable[target.IntVal]}'");
                arr[target.ArrIndex] = val;
                break;
            }
            default:
                throw new RuntimeException($"Некорректная цель присваивания: {target}");
        }
    }

    private int ResolveVarIdx(StackItem item) => item.Kind switch
    {
        StackKind.VarRef   => item.IntVal,
        StackKind.ArrayRef => item.IntVal,
        _ => throw new RuntimeException($"Ожидалась ссылка на переменную/массив, получено {item}")
    };

    private void EnsureArray(int varIdx)
    {
        if (!_isArray[varIdx] || _arrays[varIdx] == null)
            throw new RuntimeException($"Переменная '{_varTable[varIdx]}' не является массивом. Объявите: array {_varTable[varIdx]}[N];");
    }
}
