using Compilation.Interpreter.Lexer.Models;

namespace Compilation.Interpreter.Lexer;

/// <summary>
/// Лексический анализатор — конечный автомат.
/// Состояния: S, I, N, P, E, H, Q, Z, Z*, ERR
/// </summary>
public class Lexer
{
    private readonly string _source;
    private int _pos;
    private int _line;
    private int _col;
    private int _startCol;

    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.Ordinal)
    {
        ["if"]    = TokenType.IF,
        ["else"]  = TokenType.ELSE,
        ["while"] = TokenType.WHILE,
        ["read"]  = TokenType.READ,
        ["write"] = TokenType.WRITE,
        ["array"] = TokenType.ARRAY,
    };

    public Lexer(string source)
    {
        _source = source;
        _pos = 0;
        _line = 1;
        _col = 1;
    }

    // ─── helpers ──────────────────────────────────────────────────────────────

    private char Current => _pos < _source.Length ? _source[_pos] : '\0';
    private bool IsEof => _pos >= _source.Length;

    private char Consume()
    {
        char c = _source[_pos++];
        if (c == '\n') { _line++; _col = 1; }
        else           { _col++; }
        return c;
    }

    private void PutBack() { _pos--; if (_col > 1) _col--; }

    private static bool IsLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
    private static bool IsDigit(char c)  => c >= '0' && c <= '9';
    private static bool IsSpace(char c)  => c == ' ' || c == '\t' || c == '\r' || c == '\n';

    private void SkipLineComment()
    {
        while (!IsEof && Current != '\n') Consume();
    }

    // ─── main method ──────────────────────────────────────────────────────────

    /// <summary>Вернуть следующую лексему (конечный автомат).</summary>
    public Token NextToken()
    {
        // Состояние S: пропускаем пробелы и комментарии
        START:
        while (!IsEof && IsSpace(Current)) Consume();
        if (!IsEof && Current == '/' && _pos + 1 < _source.Length && _source[_pos + 1] == '/')
        {
            SkipLineComment();
            goto START;
        }

        if (IsEof)
            return new Token(TokenType.EOF, "", _line, _col);

        _startCol = _col;
        int startLine = _line;
        char ch = Consume();

        // ── Буква → состояние I (идентификатор или ключевое слово) ──
        if (IsLetter(ch))
        {
            var buf = new System.Text.StringBuilder();
            buf.Append(ch);
            // Состояние I: накапливаем буквы и цифры
            while (!IsEof && (IsLetter(Current) || IsDigit(Current)))
                buf.Append(Consume());
            // Z*: символ назад уже за счёт while (не съедали)
            string word = buf.ToString();
            TokenType type = Keywords.TryGetValue(word, out var kw) ? kw : TokenType.ID;
            return new Token(type, word, startLine, _startCol);
        }

        // ── Цифра → состояние N (целое число) ──
        if (IsDigit(ch))
        {
            var buf = new System.Text.StringBuilder();
            buf.Append(ch);
            while (!IsEof && IsDigit(Current))
                buf.Append(Consume());
            // Точка — ошибка (числа только целые)
            if (!IsEof && Current == '.')
                throw new LexerException(_line, _col, Current);
            return new Token(TokenType.NUMBER, buf.ToString(), startLine, _startCol);
        }

        // ── ':' → состояние P, ожидаем '=' ──
        if (ch == ':')
        {
            if (!IsEof && Current == '=')
            {
                Consume();
                return new Token(TokenType.ASSIGN, ":=", startLine, _startCol);
            }
            throw new LexerException(_line, _col, Current == '\0' ? ':' : Current,
                "После ':' ожидается '='");
        }

        // ── '<' → состояние E ──
        if (ch == '<')
        {
            if (!IsEof && Current == '=') { Consume(); return new Token(TokenType.LE, "<=", startLine, _startCol); }
            if (!IsEof && Current == '>') { Consume(); return new Token(TokenType.NE, "<>", startLine, _startCol); }
            return new Token(TokenType.LT, "<", startLine, _startCol);
        }

        // ── '>' → состояние H ──
        if (ch == '>')
        {
            if (!IsEof && Current == '=') { Consume(); return new Token(TokenType.GE, ">=", startLine, _startCol); }
            return new Token(TokenType.GT, ">", startLine, _startCol);
        }

        // ── '"' → состояние Q (строка) ──
        if (ch == '"')
        {
            var buf = new System.Text.StringBuilder();
            while (true)
            {
                if (IsEof)
                    throw new LexerException(_line, _col, '\0', "Незакрытая строка");
                char c = Consume();
                if (c == '"') break;
                buf.Append(c);
            }
            return new Token(TokenType.STRING, buf.ToString(), startLine, _startCol);
        }

        // ── Односимвольные лексемы (состояние Z) ──
        return ch switch
        {
            '+' => new Token(TokenType.PLUS,     "+", startLine, _startCol),
            '-' => new Token(TokenType.MINUS,    "-", startLine, _startCol),
            '*' => new Token(TokenType.MUL,      "*", startLine, _startCol),
            '/' => new Token(TokenType.DIV,      "/", startLine, _startCol),
            '=' => new Token(TokenType.EQ,       "=", startLine, _startCol),
            ';' => new Token(TokenType.SEMICOLON,";", startLine, _startCol),
            '(' => new Token(TokenType.LPAREN,   "(", startLine, _startCol),
            ')' => new Token(TokenType.RPAREN,   ")", startLine, _startCol),
            '{' => new Token(TokenType.LBRACE,   "{", startLine, _startCol),
            '}' => new Token(TokenType.RBRACE,   "}", startLine, _startCol),
            '[' => new Token(TokenType.LBRACKET, "[", startLine, _startCol),
            ']' => new Token(TokenType.RBRACKET, "]", startLine, _startCol),
            // ERR — недопустимый символ
            _   => throw new LexerException(startLine, _startCol, ch)
        };
    }
}
