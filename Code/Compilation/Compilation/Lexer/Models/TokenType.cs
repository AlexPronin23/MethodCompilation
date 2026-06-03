namespace Compilation.Interpreter.Lexer.Models;

public enum TokenType
{
    // Literals
    ID,
    NUMBER,
    STRING,

    // Operators
    PLUS,
    MINUS,
    MUL,
    DIV,
    ASSIGN,   // :=
    EQ,       // =
    LT,       // <
    GT,       // >
    LE,       // <=
    GE,       // >=
    NE,       // <>

    // Delimiters
    SEMICOLON,
    LPAREN,
    RPAREN,
    LBRACE,
    RBRACE,
    LBRACKET,
    RBRACKET,

    // Keywords
    IF,
    ELSE,
    WHILE,
    READ,
    WRITE,
    ARRAY,

    // Special
    EOF
}
