# MethodCompilation


# Лексический анализатор

## 1. Список лексем 

| № | Лексема | Пример | Описание |
|---|---------|--------|----------|
| 1 | `ID` | `x`, `abc`, `a1` | Имя переменной или массива |
| 2 | `NUMBER` | `42`, `3.14` | Целое или вещественное число |
| 3 | `PLUS` | `+` | Сложение |
| 4 | `MINUS` | `-` | Вычитание |
| 5 | `MUL` | `*` | Умножение |
| 6 | `DIV` | `/` | Деление |
| 7 | `ASSIGN` | `:=` | Присваивание |
| 8 | `SEMICOLON` | `;` | Конец оператора |
| 9 | `LPAREN` | `(` | Левая круглая скобка |
| 10 | `RPAREN` | `)` | Правая круглая скобка |
| 11 | `LBRACE` | `{` | Начало блока |
| 12 | `RBRACE` | `}` | Конец блока |
| 13 | `LBRACKET` | `[` | Начало индекса массива |
| 14 | `RBRACKET` | `]` | Конец индекса массива |
| 15 | `LT` | `<` | Меньше |
| 16 | `GT` | `>` | Больше |
| 17 | `LE` | `<=` | Меньше или равно |
| 18 | `GE` | `>=` | Больше или равно |
| 19 | `EQ` | `=` | Равно |
| 20 | `NE` | `<>` | Не равно |
| 21 | `IF` | `if` | Ключевое слово if |
| 22 | `ELSE` | `else` | Ключевое слово else |
| 23 | `WHILE` | `while` | Ключевое слово while |
| 24 | `READ` | `read` | Оператор ввода |
| 25 | `WRITE` | `write` | Оператор вывода |
| 26 | `EOF` | — | Конец файла/программы |


## 1.1 Таблица переходов конечного автомата

### Классы символов

| Обозначение | Что входит |
|-------------|-----------|
| `<б>` | Любая буква a–z, A–Z |
| `<ц>` | Любая цифра 0–9 |
| `<.>` | Точка `.` |
| `<пр>` | Пробел, табуляция, `\n` |
| `⊥` | Конец файла (EOF) |
| остальные | Символы `+ - * / = ! < > ( ) { } [ ] ;` — каждый сам по себе |

### Состояния автомата

| Состояние | Описание |
|-----------|----------|
| `S` | Старт — ждём начало новой лексемы |
| `I` | Читаем идентификатор (буквы/цифры) |
| `N` | Читаем целое число |
| `F` | Читаем дробную часть числа |
| `P` | Прочитали `:`, ждём `=` → это `:=` |
| `E` | Прочитали `<`, ждём `=` или `>` |
| `H` | Прочитали `>`, ждём `=` |
| `Z` | Финал — лексема готова, символ съеден |
| `Z*` | Финал — лексема готова, текущий символ вернуть назад |
| `ERR` | Ошибка — недопустимый символ |

### Таблица переходов

| Состояние | `<б>` | `<ц>` | `<.>` | `<пр>` | `+` | `-` | `*` | `/` | `=` | `:` | `<` | `>` | `(` | `)` | `{` | `}` | `[` | `]` | `;` | `⊥` | other |
|-----------|-------|-------|-------|--------|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-------|
| `S` | I | N | ERR | S | Z | Z | Z | Z | Z | P | E | H | Z | Z | Z | Z | Z | Z | Z | Z | ERR |
| `I` | I | I | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `N` | Z* | N | F | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `F` | Z* | F | ERR | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `P` | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | Z | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR |
| `E` | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z | Z* | Z* | Z | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `H` | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |

> `Z` — лексема готова, текущий символ принадлежит ей (съеден).  
> `Z*` — лексема готова, текущий символ вернуть во входной поток.


### Семантические программы

Таблица переходов отвечает на вопрос **«куда идти?»**  
Семантическая программа отвечает на вопрос **«что делать при этом переходе?»**

### Список семантических программ

| № | Название | Что делает |
|---|----------|------------|
| 0 | **Нет действия** | Ничего не делать, просто перейти в новое состояние |
| 1 | **Накопить символ** | Добавить текущий символ в буфер |
| 2 | **Накопить и вернуть** | Добавить символ в буфер и сразу вернуть лексему |
| 3 | **Вернуть без накопления** | Вернуть лексему из буфера, текущий символ не трогать — он уже следующей лексемы |
| 4 | **ID или ключевое слово?** | Проверить буфер по таблице ключевых слов: если `if` → IF, если `while` → WHILE, иначе → ID |
| 5 | **Зафиксировать NUMBER** | Буфер содержит число — вернуть лексему NUMBER |
| 6 | **Ошибка** | Недопустимый символ — выдать сообщение с номером строки и позиции |
| 7 | **Пропустить пробел** | Символ — пробел/таб/`\n`, буфер не трогать, если `\n` — увеличить счётчик строк |

### Привязка семантических программ к переходам

| Из | Символ | В | Сем. программа | Смысл |
|----|--------|---|----------------|-------|
| `S` | буква | `I` | 1 | Начало идентификатора — накопить |
| `S` | цифра | `N` | 1 | Начало числа — накопить |
| `S` | пробел | `S` | 7 | Пропустить пробел |
| `S` | `+ - * / ( ) { } [ ] ;` | `Z` | 2 | Односимвольная лексема — накопить и сразу вернуть |
| `S` | `=` | `A` | 1 | Может быть `=` или `==` — накопить, разберёмся дальше |
| `S` | `!` | `C` | 1 | Начало `!=` — накопить |
| `S` | `<` | `E` | 1 | Может быть `<` или `<=` — накопить |
| `S` | `>` | `H` | 1 | Может быть `>` или `>=` — накопить |
| `I` | буква/цифра | `I` | 1 | Идентификатор продолжается — накопить |
| `I` | всё остальное | `Z*` | 3 + 4 | Конец ID — вернуть символ назад, проверить ключевые слова |
| `N` | цифра | `N` | 1 | Число продолжается — накопить |
| `N` | `.` | `F` | 1 | Началась дробная часть — накопить |
| `N` | всё остальное | `Z*` | 3 + 5 | Конец числа — вернуть символ назад, вернуть NUMBER |
| `F` | цифра | `F` | 1 | Дробная часть продолжается — накопить |
| `F` | всё остальное | `Z*` | 3 + 5 | Конец числа — вернуть символ назад, вернуть NUMBER |
| `A` | `=` | `Z` | 2 | Это `==` — накопить и вернуть EQ |
| `A` | всё остальное | `Z*` | 3 | Это просто `=` — вернуть символ назад, вернуть ASSIGN |
| `C` | `=` | `Z` | 2 | Это `!=` — накопить и вернуть NE |
| `C` | всё остальное | `ERR` | 6 | `!` без `=` — ошибка |
| `E` | `=` | `Z` | 2 | Это `<=` — накопить и вернуть LE |
| `E` | всё остальное | `Z*` | 3 | Это просто `<` — вернуть символ назад, вернуть LT |
| `H` | `=` | `Z` | 2 | Это `>=` — накопить и вернуть GE |
| `H` | всё остальное | `Z*` | 3 | Это просто `>` — вернуть символ назад, вернуть GT |



## 2. КС-грамматика языка

КС (контекстно-свободная) грамматика описывает **как лексемы должны быть расставлены**,
чтобы программа была правильной. Это правила структуры языка.


### Программа

```
program → statement_list
```

### Список операторов

```
statement_list → statement statement_list
statement_list → ε
```

### Оператор

```
statement → assignment
statement → if_statement
statement → while_statement
statement → read_statement
statement → write_statement
statement → block
```

### Присваивание

```
assignment → variable ASSIGN expression SEMICOLON
```

### Переменная

```
variable → ID
variable → ID LBRACKET expression RBRACKET
```

> Второе правило — это обращение к элементу массива: `arr[i]`

### Составной оператор (блок)

```
block → LBRACE statement_list RBRACE
```

### Условный оператор

```
if_statement → IF LPAREN condition RPAREN block else_part

else_part → ELSE block
else_part → ε
```

> `else` необязателен — поэтому `else_part` может быть пустым (ε)

### Оператор цикла

```
while_statement → WHILE LPAREN condition RPAREN block
```

### Операторы ввода и вывода

```
read_statement  → READ LPAREN variable RPAREN SEMICOLON
write_statement → WRITE LPAREN expression RPAREN SEMICOLON
```

### Условие

```
condition → expression rel_op expression
```

### Операции сравнения

```
rel_op → LT
rel_op → GT
rel_op → LE
rel_op → GE
rel_op → EQ
rel_op → NE
```

### Выражение (низший приоритет: + -)

```
expression → expression PLUS term
expression → expression MINUS term
expression → term
```

### Терм (высший приоритет: * /)

```
term → term MUL factor
term → term DIV factor
term → factor
```

### Фактор — атомарная единица выражения

```
factor → NUMBER
factor → variable
factor → LPAREN expression RPAREN
factor → MINUS factor
```

##  Устранение левой рекурсии

Левая рекурсия — это когда нетерминал стоит в начале своей же правой части:


### Формула устранения

```
A → A α         A → β A'
A → β     →     A' → α A'
                A' → ε
```

Всё что было «хвостом» после рекурсии (α) уходит в новый нетерминал A',
который повторяет себя или заканчивается пустым (ε).

---

### expression

Было:
```
expression → expression PLUS term
expression → expression MINUS term
expression → term
```

Стало:
```
expression  → term expression'
expression' → PLUS term expression'
expression' → MINUS term expression'
expression' → ε
```

### term

Было:
```
term → term MUL factor
term → term DIV factor
term → factor
```

Стало:
```
term  → factor term'
term' → MUL factor term'
term' → DIV factor term'
term' → ε
```

### statement_list

```
statement_list → statement statement_list
statement_list → ε
```

> Рекурсия **правая** — `statement_list` стоит в конце. Правая рекурсия
> для магазинного автомата не проблема, не трогаем.

---

### Полная грамматика после устранения левой рекурсии

```
program         → statement_list

statement_list  → statement statement_list
statement_list  → ε

statement       → assignment
statement       → if_statement
statement       → while_statement
statement       → read_statement
statement       → write_statement
statement       → block

assignment      → variable ASSIGN expression SEMICOLON

variable        → ID
variable        → ID LBRACKET expression RBRACKET

block           → LBRACE statement_list RBRACE

if_statement    → IF LPAREN condition RPAREN block else_part
else_part       → ELSE block
else_part       → ε

while_statement → WHILE LPAREN condition RPAREN block

read_statement  → READ LPAREN variable RPAREN SEMICOLON
write_statement → WRITE LPAREN expression RPAREN SEMICOLON

condition       → expression rel_op expression

rel_op          → LT | GT | LE | GE | EQ | NE

expression      → term expression'
expression'     → PLUS term expression'
expression'     → MINUS term expression'
expression'     → ε

term            → factor term'
term'           → MUL factor term'
term'           → DIV factor term'
term'           → ε

factor          → NUMBER
factor          → variable
factor          → LPAREN expression RPAREN
factor          → MINUS factor
```

---

## 3. Нестрогая нормальная форма Грейбах (ННФГ)

Нестрогая нормальная форма Грейбах (ННФГ) — это такой вид записи грамматики, при котором каждое правило обязано начинаться с терминала (лексемы), либо быть пустым (ε).
Чтобы привести грамматику к этому виду, используется следующий приём: если правило имеет вид A → B α, где первый символ B является нетерминалом, то B заменяется на все его правые части поочерёдно. Процесс повторяется до тех пор, пока ни одно правило не будет начинаться с нетерминала.
В нашей грамматике, полученной после устранения левой рекурсии, правила начинающиеся с нетерминала встречаются в следующих случаях:

```
statement  → assignment | if_statement | while_statement | ...
assignment → variable ASSIGN expression SEMICOLON
variable   → ID | ID LBRACKET expression RBRACKET
condition  → expression rel_op expression
expression → term expression'
term       → factor term'
factor     → variable | NUMBER | LPAREN expression RPAREN | MINUS factor

```

### Итоговая грамматика в ННФГ

```
program → statement_list

statement_list → statement statement_list
statement_list → ε

statement → ID statement_tail
statement → IF LPAREN condition RPAREN block else_part
statement → WHILE LPAREN condition RPAREN block
statement → READ LPAREN ID read_tail RPAREN SEMICOLON
statement → WRITE LPAREN expression RPAREN SEMICOLON
statement → LBRACE statement_list RBRACE

statement_tail → ASSIGN expression SEMICOLON
statement_tail → LBRACKET expression RBRACKET ASSIGN expression SEMICOLON

block → LBRACE statement_list RBRACE

else_part → ELSE LBRACE statement_list RBRACE
else_part → ε

read_tail → LBRACKET expression RBRACKET
read_tail → ε

condition → ID condition_tail
condition → NUMBER rel_op expression
condition → LPAREN expression RPAREN rel_op expression
condition → MINUS factor term' rel_op expression

condition_tail → LBRACKET expression RBRACKET rel_op expression
condition_tail → rel_op expression

rel_op → LT
rel_op → GT
rel_op → LE
rel_op → GE
rel_op → EQ
rel_op → NE

expression → ID expression_var_tail
expression → NUMBER term' expression'
expression → LPAREN expression RPAREN term' expression'
expression → MINUS factor term' expression'

expression_var_tail → LBRACKET expression RBRACKET term' expression'
expression_var_tail → term' expression'

expression' → PLUS ID expression_var_tail
expression' → PLUS NUMBER term' expression'
expression' → PLUS LPAREN expression RPAREN term' expression'
expression' → PLUS MINUS factor term' expression'
expression' → MINUS ID expression_var_tail
expression' → MINUS NUMBER term' expression'
expression' → MINUS LPAREN expression RPAREN term' expression'
expression' → MINUS MINUS factor term' expression'
expression' → ε

term' → MUL ID term_var_tail
term' → MUL NUMBER term'
term' → MUL LPAREN expression RPAREN term'
term' → MUL MINUS factor term'
term' → DIV ID term_var_tail
term' → DIV NUMBER term'
term' → DIV LPAREN expression RPAREN term'
term' → DIV MINUS factor term'
term' → ε

term_var_tail → LBRACKET expression RBRACKET term'
term_var_tail → term'

factor → ID
factor → ID LBRACKET expression RBRACKET
factor → NUMBER
factor → LPAREN expression RPAREN
factor → MINUS factor

```