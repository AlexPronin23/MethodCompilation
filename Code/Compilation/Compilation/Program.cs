using Compilation.Interpreter.Lexer;
using Compilation.Interpreter.Lexer.Models;
using Compilation.Interpreter.Parser;
using Compilation.Interpreter.Interpreter;


// Вспомогательная функция: транслировать и выполнить программу

void Run(string source)
{
    // 1. Лексический анализ
    var lexer = new Lexer(source);
    var tokens = new List<Token>();
    Token token;
    do
    {
        token = lexer.NextToken();
        tokens.Add(token);
    }
    while (token.Type != TokenType.EOF);

    // 2. Синтаксический анализ + генерация ОПС
    var parser = new Parser(tokens);
    parser.Parse();

    // 3. Интерпретация ОПС
    var interpreter = new Interpreter(
        parser.Ops,
        parser.VarTable,
        parser.ConstTable,
        parser.StrTable);

    interpreter.Run();

    // 4. Вывод ОПС после выполнения программы
    Console.WriteLine();
    Console.WriteLine("ОПС:");
    Console.WriteLine(new string('-', 50));

    for (int i = 0; i < parser.Ops.Count; i++)
    {
        Console.WriteLine($"{i,3}: {parser.Ops[i]}");
    }

    Console.WriteLine(new string('-', 50));
}

// ─────────────────────────────────────────────────────────────────────────────
// Меню выбора режима
// ─────────────────────────────────────────────────────────────────────────────
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding  = System.Text.Encoding.UTF8;

while (true)
{
    Console.WriteLine();
    Console.WriteLine("╔══════════════════════════════════════════╗");
    Console.WriteLine("║    Транслятор-интерпретатор (ReserveLang) ║");
    Console.WriteLine("╠══════════════════════════════════════════╣");
    Console.WriteLine("║  1 — Запустить все тесты                 ║");
    Console.WriteLine("║  2 — Интерактивный режим (свой код)      ║");
    Console.WriteLine("║  0 — Выход                               ║");
    Console.WriteLine("╚══════════════════════════════════════════╝");
    Console.Write("Выбор: ");
    string? choice = Console.ReadLine()?.Trim();

    if (choice == "0") break;

    if (choice == "1")
    {
        RunAllTests();
    }
    else if (choice == "2")
    {
        RunInteractive();
    }
    else
    {
        Console.WriteLine("Неверный выбор.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ТЕСТЫ
// ─────────────────────────────────────────────────────────────────────────────
void RunAllTests()
{
    RunTest("Тест 1: Простые вычисления (присваивание + арифметика)", """
        x := 10;
        y := 3;
        z := x + y * 2;
        write(z);
        z := x - y;
        write(z);
        z := x * y;
        write(z);
        z := x / y;
        write(z);
        z := -x + 1;
        write(z);
        """);

    RunTest("Тест 2: Условный оператор if-else (знак числа)", """
        write("Введите число:");
        read(x);
        if (x > 0) {
            write("Верно");
        } else {
           write("Не верно");
        }
        """);

    RunTest("Тест 3: Цикл while — сумма от 1 до n", """
        write("Введите n:");
        read(n);
        sum := 0;
        i := 1;
        while (i <= n) {
            sum := sum + i;
            i := i + 1;
        }
        write(sum);
        """);

    RunTest("Тест 4: Массив — ввод и вывод", """
        array M[5];
        i := 0;
        while (i < 5) {
            write("Введите элемент:");
            read(M[i]);
            i := i + 1;
        }
        i := 0;
        while (i < 5) {
            write(M[i]);
            i := i + 1;
        }
        """);

    RunTest("Тест 5: Пузырьковая сортировка", """
        array M[5];
        i := 0;
        while (i < 5) {
            write("Введите элемент:");
            read(M[i]);
            i := i + 1;
        }
        i := 0;
        while (i < 5) {
            j := 0;
            while (j < 4) {
                if (M[j] > M[j + 1]) {
                    tmp := M[j];
                    M[j] := M[j + 1];
                    M[j + 1] := tmp;
                }
                j := j + 1;
            }
            i := i + 1;
        }
        k := 0;
        while (k < 5) {
            write(M[k]);
            k := k + 1;
        }
        """);

    RunTest("Тест 6: Лексическая ошибка (символ @)", """
        x := 10;
        y := x @ 5;
        """);

    RunTest("Тест 7: Синтаксическая ошибка (нет ; после присваивания)", """
        x := 10
        y := x + 5;
        """);

    RunTest("Тест 8: Синтаксическая ошибка (незакрытая скобка)", """
        x := 10;
        y := (x + 5;
        """);
}

void RunTest(string name, string source)
{
    Console.WriteLine();
    Console.WriteLine($"=== {name} ===");
    try
    {
        Run(source);
        Console.WriteLine("--- OK ---");
    }
    catch (Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Message);
        Console.ResetColor();
    }
}


// ИНТЕРАКТИВНЫЙ РЕЖИМ

void RunInteractive()
{
    Console.WriteLine();
    Console.WriteLine("Интерактивный режим.");
    Console.WriteLine("Введите программу (синтаксис: := для присваивания, array X[N]; для массива).");
    Console.WriteLine("Пустая строка продолжает ввод. Введите END для выполнения, CLEAR для сброса, BACK для выхода.");
    Console.WriteLine();

    var lines = new System.Text.StringBuilder();

    while (true)
    {
        Console.Write(lines.Length == 0 ? ">>> " : "... ");
        string? line = Console.ReadLine();
        if (line == null) break;

        if (line.Trim().Equals("BACK", StringComparison.OrdinalIgnoreCase))
            break;

        if (line.Trim().Equals("CLEAR", StringComparison.OrdinalIgnoreCase))
        {
            lines.Clear();
            Console.WriteLine("[Буфер очищен]");
            continue;
        }

        if (line.Trim().Equals("END", StringComparison.OrdinalIgnoreCase))
        {
            string source = lines.ToString();
            if (source.Trim().Length == 0)
            {
                Console.WriteLine("[Пустая программа]");
                lines.Clear();
                continue;
            }

            Console.WriteLine();
            Console.WriteLine("--- Выполнение ---");
            try
            {
                Run(source);
                Console.WriteLine("--- Выполнено успешно ---");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }

            lines.Clear();
            Console.WriteLine();
            continue;
        }

        lines.AppendLine(line);
    }
}
