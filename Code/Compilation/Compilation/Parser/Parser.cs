using Compilation.Interpreter.Lexer.Models;

namespace Compilation.Interpreter.Parser;

/// <summary>
/// Синтаксический анализатор — LL(1) магазинный автомат + табличный генератор ОПС.
/// 
/// </summary>
public class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;

    // Таблицы компилятора
    public List<OpsElement> Ops { get; } = new();
    public List<string>     VarTable   { get; } = new();   // имена переменных
    public List<int>        ConstTable { get; } = new();   // числовые константы
    public List<string>     StrTable   { get; } = new();   // строковые константы

    // Магазин меток для П1-П5
    private readonly Stack<int> _labelStack = new();

    // ─── helpers ──────────────────────────────────────────────────────────────

    private Token Cur => _tokens[_pos];

    private Token Consume(TokenType expected)
    {
        if (Cur.Type != expected)
            throw new ParserException(Cur, expected.ToString());
        return _tokens[_pos++];
    }

    private Token ConsumeAny()
    {
        return _tokens[_pos++];
    }

    private bool Check(TokenType t) => Cur.Type == t;

    // ─── таблицы ──────────────────────────────────────────────────────────────

    private int GetOrAddVar(string name)
    {
        int idx = VarTable.IndexOf(name);
        if (idx < 0) { idx = VarTable.Count; VarTable.Add(name); }
        return idx;
    }

    private int GetOrAddConst(int value)
    {
        int idx = ConstTable.IndexOf(value);
        if (idx < 0) { idx = ConstTable.Count; ConstTable.Add(value); }
        return idx;
    }

    private int GetOrAddStr(string value)
    {
        int idx = StrTable.IndexOf(value);
        if (idx < 0) { idx = StrTable.Count; StrTable.Add(value); }
        return idx;
    }

    // ─── генерация ОПС ────────────────────────────────────────────────────────

    private void Emit(OpsType t, int v) => Ops.Add(new OpsElement(t, v));
    private void EmitOp(OpCode op)       => Emit(OpsType.TYPE_OP, (int)op);
    private void EmitVar(string name)    => Emit(OpsType.TYPE_VAR, GetOrAddVar(name));
    private void EmitConst(int val)      => Emit(OpsType.TYPE_CONST, GetOrAddConst(val));
    private void EmitStr(string val)     => Emit(OpsType.TYPE_STR_CONST, GetOrAddStr(val));

    /// Зарезервировать место под метку, вернуть индекс этого места
    private int EmitPlaceholder()
    {
        int idx = Ops.Count;
        Ops.Add(new OpsElement(OpsType.TYPE_LABEL, -1)); // -1 = незаполнено
        return idx;
    }

    private void FillPlaceholder(int placeholderIdx, int address)
    {
        Ops[placeholderIdx].Value = address;
    }

    // ─── Семантические программы ──────────────────────────────────────────────

    // П1: после условия if/while — записать jf с пустой меткой, сохранить адрес
    private void П1()
    {
        int ph = EmitPlaceholder();   // место под метку
        EmitOp(OpCode.OP_JF);
        _labelStack.Push(ph);         // сохраняем адрес placeholder-а
    }

    // П2: начало else — заполнить метку П1, записать j с пустой меткой
    private void П2()
    {
        int ph1 = _labelStack.Pop();
        // jf должен прыгнуть сюда + 2 (перелететь через j+метку)
        FillPlaceholder(ph1, Ops.Count + 2);
        int ph2 = EmitPlaceholder();
        EmitOp(OpCode.OP_J);
        _labelStack.Push(ph2);
    }

    // П3: конец if / if-else — заполнить последнюю метку
    private void П3()
    {
        int ph = _labelStack.Pop();
        FillPlaceholder(ph, Ops.Count);
    }

    // П4: перед условием while — запомнить адрес начала
    private void П4()
    {
        _labelStack.Push(Ops.Count);  // адрес начала условия
    }

    // П5: после тела while — заполнить метку П1, записать j назад
    private void П5()
    {
        int ph1 = _labelStack.Pop();             // placeholder от П1
        int loopStart = _labelStack.Pop();       // адрес начала цикла от П4
        FillPlaceholder(ph1, Ops.Count + 2);     // jf → сюда (после j)
        Emit(OpsType.TYPE_LABEL, loopStart);
        EmitOp(OpCode.OP_J);
    }



    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        _pos = 0;
    }

    public void Parse()
    {
        ParseOperatorList();
        Consume(TokenType.EOF);
    }

    // ─── грамматические правила ───────────────────────────────────────────────

    // operator_list → operator operator_list | ε
    private void ParseOperatorList()
    {
        while (IsStartOfOperator())
            ParseOperator();
    }

    private bool IsStartOfOperator()
    {
        return Cur.Type is TokenType.ARRAY or TokenType.ID or TokenType.IF
                       or TokenType.WHILE or TokenType.READ or TokenType.WRITE
                       or TokenType.LBRACE;
    }

    // operator → ...
    private void ParseOperator()
    {
        switch (Cur.Type)
        {
            case TokenType.ARRAY:
                ParseArrayDecl();
                break;
            case TokenType.ID:
                ParseAssign();
                break;
            case TokenType.IF:
                ParseIf();
                break;
            case TokenType.WHILE:
                ParseWhile();
                break;
            case TokenType.READ:
                ParseRead();
                break;
            case TokenType.WRITE:
                ParseWrite();
                break;
            case TokenType.LBRACE:
                ParseBlock();
                break;
            default:
                throw new ParserException(Cur, "оператор", true);
        }
    }

    // array_decl → ARRAY ID LBRACKET NUMBER RBRACKET SEMICOLON
    private void ParseArrayDecl()
    {
        Consume(TokenType.ARRAY);
        var idTok = Consume(TokenType.ID);
        Consume(TokenType.LBRACKET);
        var numTok = Consume(TokenType.NUMBER);
        Consume(TokenType.RBRACKET);
        Consume(TokenType.SEMICOLON);

        // Семантика: зарегистрировать имя массива, создать его
        int varIdx = GetOrAddVar(idTok.Value);
        int size   = int.Parse(numTok.Value);
        int cIdx   = GetOrAddConst(size);

        Emit(OpsType.TYPE_VAR, varIdx);
        Emit(OpsType.TYPE_CONST, cIdx);
        EmitOp(OpCode.OP_ARRAY);
    }

    // assign_op → ID operator_tail
    private void ParseAssign()
    {
        var idTok = Consume(TokenType.ID);
        EmitVar(idTok.Value);   // {a}
        ParseOperatorTail();
    }

    // operator_tail → ASSIGN formula SEMICOLON
    //               | LBRACKET formula RBRACKET ASSIGN formula SEMICOLON
    private void ParseOperatorTail()
    {
        if (Check(TokenType.ASSIGN))
        {
            Consume(TokenType.ASSIGN);
            ParseFormula();
            Consume(TokenType.SEMICOLON);
            EmitOp(OpCode.OP_ASSIGN);  // {:=}
        }
        else if (Check(TokenType.LBRACKET))
        {
            Consume(TokenType.LBRACKET);
            ParseFormula();
            Consume(TokenType.RBRACKET);
            EmitOp(OpCode.OP_I);       // {i}
            Consume(TokenType.ASSIGN);
            ParseFormula();
            Consume(TokenType.SEMICOLON);
            EmitOp(OpCode.OP_ASSIGN);  // {:=}
        }
        else
        {
            throw new ParserException(Cur, ":= или [", true);
        }
    }

    // block_body → LBRACE operator_list RBRACE
    private void ParseBlock()
    {
        Consume(TokenType.LBRACE);
        ParseOperatorList();
        Consume(TokenType.RBRACE);
    }

    // if_op → IF LPAREN cond RPAREN block_body else_op
    private void ParseIf()
    {
        Consume(TokenType.IF);
        Consume(TokenType.LPAREN);
        ParseCond();
        Consume(TokenType.RPAREN);
        П1();                // после условия
        ParseBlock();
        ParseElse();
        П3();                // конец if
    }

    // else_op → ELSE block_body | ε
    private void ParseElse()
    {
        if (Check(TokenType.ELSE))
        {
            П2();            // начало else
            Consume(TokenType.ELSE);
            ParseBlock();
        }
    }

    // while_op → WHILE LPAREN cond RPAREN block_body
    private void ParseWhile()
    {
        Consume(TokenType.WHILE);
        П4();                // запомнить начало
        Consume(TokenType.LPAREN);
        ParseCond();
        Consume(TokenType.RPAREN);
        П1();                // после условия
        ParseBlock();
        П5();                // после тела
    }

    // input_op → READ LPAREN target RPAREN SEMICOLON
    private void ParseRead()
    {
        Consume(TokenType.READ);
        Consume(TokenType.LPAREN);
        var idTok = Consume(TokenType.ID);
        EmitVar(idTok.Value);   // {a}
        if (Check(TokenType.LBRACKET))
        {
            Consume(TokenType.LBRACKET);
            ParseFormula();
            Consume(TokenType.RBRACKET);
            EmitOp(OpCode.OP_I);  // {i}
        }
        Consume(TokenType.RPAREN);
        Consume(TokenType.SEMICOLON);
        EmitOp(OpCode.OP_R);   // {r}
    }

    // output_op → WRITE LPAREN formula RPAREN SEMICOLON
    //           | WRITE LPAREN STRING RPAREN SEMICOLON
    private void ParseWrite()
    {
        Consume(TokenType.WRITE);
        Consume(TokenType.LPAREN);

        if (Check(TokenType.STRING))
        {
            var sTok = Consume(TokenType.STRING);
            EmitStr(sTok.Value);   // {ks}
            EmitOp(OpCode.OP_WS); // {ws}
        }
        else
        {
            ParseFormula();
            EmitOp(OpCode.OP_W);  // {w}
        }

        Consume(TokenType.RPAREN);
        Consume(TokenType.SEMICOLON);
    }

    // ─── Условие ──────────────────────────────────────────────────────────────

    // cond → formula rel_op formula
    private void ParseCond()
    {
        ParseFormula();
        var op = ParseRelOp();
        ParseFormula();
        EmitOp(op);
    }

    private OpCode ParseRelOp()
    {
        var tok = ConsumeAny();
        return tok.Type switch
        {
            TokenType.LT => OpCode.OP_LT,
            TokenType.GT => OpCode.OP_GT,
            TokenType.LE => OpCode.OP_LE,
            TokenType.GE => OpCode.OP_GE,
            TokenType.EQ => OpCode.OP_EQ,
            TokenType.NE => OpCode.OP_NE,
            _            => throw new ParserException(tok, "оператор сравнения", true)
        };
    }

    // ─── Формула ──────────────────────────────────────────────────────────────

    // formula → addend formula'
    private void ParseFormula()
    {
        ParseAddend();
        ParseFormulaTail();
    }

    // formula' → PLUS addend formula' | MINUS addend formula' | ε
    private void ParseFormulaTail()
    {
        if (Check(TokenType.PLUS))
        {
            Consume(TokenType.PLUS);
            ParseAddend();
            EmitOp(OpCode.OP_ADD);   // {+}
            ParseFormulaTail();
        }
        else if (Check(TokenType.MINUS))
        {
            // Нужно отличить бинарный минус от унарного.
            // Здесь это бинарный (мы уже внутри formula')
            Consume(TokenType.MINUS);
            ParseAddend();
            EmitOp(OpCode.OP_SUB);   // {-}
            ParseFormulaTail();
        }
    }

    // addend → multiplier addend'
    private void ParseAddend()
    {
        ParseMultiplier();
        ParseAddendTail();
    }

    // addend' → MUL multiplier addend' | DIV multiplier addend' | ε
    private void ParseAddendTail()
    {
        if (Check(TokenType.MUL))
        {
            Consume(TokenType.MUL);
            ParseMultiplier();
            EmitOp(OpCode.OP_MUL);   // {*}
            ParseAddendTail();
        }
        else if (Check(TokenType.DIV))
        {
            Consume(TokenType.DIV);
            ParseMultiplier();
            EmitOp(OpCode.OP_DIV);   // {/}
            ParseAddendTail();
        }
    }

    // multiplier → NUMBER | STRING | ID | ID[formula] | (formula) | -multiplier
    private void ParseMultiplier()
    {
        if (Check(TokenType.NUMBER))
        {
            var tok = Consume(TokenType.NUMBER);
            EmitConst(int.Parse(tok.Value));  // {k}
        }
        else if (Check(TokenType.STRING))
        {
            var tok = Consume(TokenType.STRING);
            EmitStr(tok.Value);               // {ks}
        }
        else if (Check(TokenType.ID))
        {
            var tok = Consume(TokenType.ID);
            EmitVar(tok.Value);               // {a}
            if (Check(TokenType.LBRACKET))
            {
                Consume(TokenType.LBRACKET);
                ParseFormula();
                Consume(TokenType.RBRACKET);
                EmitOp(OpCode.OP_I);          // {i}
            }
        }
        else if (Check(TokenType.LPAREN))
        {
            Consume(TokenType.LPAREN);
            ParseFormula();
            Consume(TokenType.RPAREN);
        }
        else if (Check(TokenType.MINUS))
        {
            Consume(TokenType.MINUS);
            ParseMultiplier();
            EmitOp(OpCode.OP_NEG);            // {-'}
        }
        else
        {
            throw new ParserException(Cur, "число, строка, переменная или выражение в скобках", true);
        }
    }
}
