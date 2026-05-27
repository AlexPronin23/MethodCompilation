# MethodCompilation

# Лексический анализатор

## 1. Список лексем

| № | Лексема | Пример | Описание |
|---|---------|--------|----------|
| 1 | `ID` | `x`, `abc`, `a1` | Имя переменной или массива |
| 2 | `NUMBER` | `42`, `7` | Целое число |
| 3 | `STRING` | `"hello"` | Строковый литерал |
| 4 | `PLUS` | `+` | Сложение |
| 5 | `MINUS` | `-` | Вычитание |
| 6 | `MUL` | `*` | Умножение |
| 7 | `DIV` | `/` | Деление |
| 8 | `ASSIGN` | `:=` | Присваивание |
| 9 | `SEMICOLON` | `;` | Конец оператора |
| 10 | `LPAREN` | `(` | Левая круглая скобка |
| 11 | `RPAREN` | `)` | Правая круглая скобка |
| 12 | `LBRACE` | `{` | Начало блока |
| 13 | `RBRACE` | `}` | Конец блока |
| 14 | `LBRACKET` | `[` | Начало индекса массива |
| 15 | `RBRACKET` | `]` | Конец индекса массива |
| 16 | `LT` | `<` | Меньше |
| 17 | `GT` | `>` | Больше |
| 18 | `LE` | `<=` | Меньше или равно |
| 19 | `GE` | `>=` | Больше или равно |
| 20 | `EQ` | `=` | Равно |
| 21 | `NE` | `<>` | Не равно |
| 22 | `IF` | `if` | Ключевое слово if |
| 23 | `ELSE` | `else` | Ключевое слово else |
| 24 | `WHILE` | `while` | Ключевое слово while |
| 25 | `READ` | `read` | Оператор ввода |
| 26 | `WRITE` | `write` | Оператор вывода |
| 27 | `ARRAY` | `array` | Объявление массива |
| 28 | `EOF` | — | Конец файла/программы |

---

## 1.1 Таблица переходов конечного автомата

### Классы символов

| Обозначение | Что входит |
|-------------|-----------|
| `<б>` | Любая буква a–z, A–Z |
| `<ц>` | Любая цифра 0–9 |
| `<пр>` | Пробел, табуляция, `\n` |
| `<any>` | Любой символ кроме `"` |
| `⊥` | Конец файла (EOF) |
| остальные | Символы `+ - * / = : < > ( ) { } [ ] ; "` — каждый сам по себе |

> Точка `.` не является допустимым символом — числа только целые.

### Состояния автомата

| Состояние | Описание |
|-----------|----------|
| `S` | Старт — ждём начало новой лексемы |
| `I` | Читаем идентификатор (буквы/цифры) |
| `N` | Читаем целое число |
| `P` | Прочитали `:`, ждём `=` → это `:=` |
| `E` | Прочитали `<`, ждём `=` или `>` |
| `H` | Прочитали `>`, ждём `=` |
| `Q` | Читаем строку внутри кавычек |
| `Z` | Финал — лексема готова, символ съеден |
| `Z*` | Финал — лексема готова, текущий символ вернуть назад |
| `ERR` | Ошибка — недопустимый символ |

### Таблица переходов

| Состояние | `<б>` | `<ц>` | `<пр>` | `+` | `-` | `*` | `/` | `=` | `:` | `<` | `>` | `(` | `)` | `{` | `}` | `[` | `]` | `;` | `"` | `<any>` | `⊥` |
|-----------|-------|-------|--------|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|---------|-----|
| `S` | I | N | S | Z | Z | Z | Z | Z | P | E | H | Z | Z | Z | Z | Z | Z | Z | Q | ERR | Z |
| `I` | I | I | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `N` | Z* | N | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `P` | ERR | ERR | ERR | ERR | ERR | ERR | ERR | Z | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR |
| `E` | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z | Z* | Z* | Z | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `H` | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `Q` | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Z | Q | ERR |

> `Z` — лексема готова, текущий символ принадлежит ей (съеден).
> `Z*` — лексема готова, текущий символ вернуть во входной поток.
> Состояние `Q` — внутри строки, накапливаем всё до закрывающей `"`.

### Какие лексемы возвращает каждое финальное состояние

| Из состояния | Условие | Лексема |
|-------------|---------|---------|
| `S` | `=` | EQ |
| `S` | `+ - * / ; ( ) { } [ ]` | соответствующая односимвольная лексема |
| `S` | `⊥` | EOF |
| `I` | небуква/нецифра | ID или ключевое слово (проверка по таблице) |
| `N` | нецифровой символ | NUMBER |
| `P` | `=` | ASSIGN (`:=`) |
| `P` | всё остальное | ERR — `:` без `=` недопустимо |
| `E` | `=` | LE (`<=`) |
| `E` | `>` | NE (`<>`) |
| `E` | всё остальное | LT (`<`) |
| `H` | `=` | GE (`>=`) |
| `H` | всё остальное | GT (`>`) |
| `Q` | `"` | STRING |
| `Q` | `⊥` | ERR — строка не закрыта |

---

## 1.2 Семантические программы

Таблица переходов отвечает на вопрос **«куда идти?»**
Семантическая программа отвечает на вопрос **«что делать при этом переходе?»**

### Список семантических программ

| № | Название | Что делает |
|---|----------|------------|
| 0 | **Нет действия** | Ничего не делать, просто перейти в новое состояние |
| 1 | **Накопить символ** | Добавить текущий символ в буфер |
| 2 | **Накопить и вернуть** | Добавить символ в буфер и сразу вернуть лексему |
| 3 | **Вернуть без накопления** | Вернуть лексему из буфера, текущий символ не трогать — он уже следующей лексемы |
| 4 | **ID или ключевое слово?** | Проверить буфер: `if`→IF, `while`→WHILE, `else`→ELSE, `read`→READ, `write`→WRITE, `array`→ARRAY, иначе→ID |
| 5 | **Зафиксировать NUMBER** | Буфер содержит целое число — вернуть лексему NUMBER |
| 6 | **Ошибка** | Недопустимый символ — выдать сообщение с номером строки и позиции |
| 7 | **Пропустить пробел** | Символ — пробел/таб/`\n`, буфер не трогать, если `\n` — увеличить счётчик строк |

### Привязка семантических программ к переходам

| Из | Символ | В | Сем. программа | Смысл |
|----|--------|---|----------------|-------|
| `S` | буква | `I` | 1 | Начало идентификатора — накопить |
| `S` | цифра | `N` | 1 | Начало числа — накопить |
| `S` | пробел | `S` | 7 | Пропустить пробел |
| `S` | `+ - * / ; ( ) { } [ ]` | `Z` | 2 | Односимвольная лексема — накопить и вернуть |
| `S` | `=` | `Z` | 2 | EQ — накопить и вернуть |
| `S` | `:` | `P` | 1 | Может быть `:=` — накопить |
| `S` | `<` | `E` | 1 | Может быть `<`, `<=`, `<>` — накопить |
| `S` | `>` | `H` | 1 | Может быть `>` или `>=` — накопить |
| `S` | `"` | `Q` | 0 | Начало строки — кавычку не накапливаем |
| `I` | буква/цифра | `I` | 1 | Идентификатор продолжается — накопить |
| `I` | всё остальное | `Z*` | 3 + 4 | Конец ID — символ назад, проверить ключевые слова |
| `N` | цифра | `N` | 1 | Число продолжается — накопить |
| `N` | всё остальное | `Z*` | 3 + 5 | Конец числа — символ назад, вернуть NUMBER |
| `P` | `=` | `Z` | 2 | Это `:=` — накопить и вернуть ASSIGN |
| `P` | всё остальное | `ERR` | 6 | `:` без `=` — ошибка |
| `E` | `=` | `Z` | 2 | Это `<=` — накопить и вернуть LE |
| `E` | `>` | `Z` | 2 | Это `<>` — накопить и вернуть NE |
| `E` | всё остальное | `Z*` | 3 | Это `<` — символ назад, вернуть LT |
| `H` | `=` | `Z` | 2 | Это `>=` — накопить и вернуть GE |
| `H` | всё остальное | `Z*` | 3 | Это `>` — символ назад, вернуть GT |
| `Q` | любой кроме `"` | `Q` | 1 | Внутри строки — накопить |
| `Q` | `"` | `Z` | 2 | Конец строки — вернуть STRING |
| `Q` | `⊥` | `ERR` | 6 | Строка не закрыта — ошибка |

> `3 + 4` — вернуть символ назад и проверить таблицу ключевых слов
> `3 + 5` — вернуть символ назад и вернуть NUMBER

---

## 2. КС-грамматика языка

КС-грамматика описывает **как лексемы должны быть расставлены**
чтобы программа была правильной.

Два вида символов:
- **Терминалы** — лексемы: `IF`, `ID`, `NUMBER`, `ASSIGN` и т.д.
- **Нетерминалы** — абстрактные понятия: `formula`, `operator`, `block_body`
- **ε** — пустая строка

### Программа

```
program → operator_list
```

### Список операторов

```
operator_list → operator operator_list
operator_list → ε
```

### Оператор

```
operator → array_decl
operator → assign_op
operator → if_op
operator → while_op
operator → input_op
operator → output_op
operator → block_body
```

### Объявление массива

```
array_decl → ARRAY ID LBRACKET NUMBER RBRACKET SEMICOLON
```

### Присваивание

```
assign_op → target ASSIGN formula SEMICOLON
```

### Цель присваивания

```
target → ID
target → ID LBRACKET formula RBRACKET
```

> Второе правило — обращение к элементу массива: `a[i]`

### Составной оператор (блок)

```
block_body → LBRACE operator_list RBRACE
```

### Условный оператор

```
if_op   → IF LPAREN cond RPAREN block_body else_op
else_op → ELSE block_body
else_op → ε
```

> `else` необязателен — поэтому `else_op` может быть пустым (ε)

### Оператор цикла

```
while_op → WHILE LPAREN cond RPAREN block_body
```

### Операторы ввода и вывода

```
input_op  → READ LPAREN target RPAREN SEMICOLON
output_op → WRITE LPAREN formula RPAREN SEMICOLON
```

### Условие

```
cond → formula rel_op formula
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

### Формула — низший приоритет (+ -)

```
formula → formula PLUS  addend
formula → formula MINUS addend
formula → addend
```

### Слагаемое — высший приоритет (* /)

```
addend → addend MUL multiplier
addend → addend DIV multiplier
addend → multiplier
```

### Множитель — атомарная единица

```
multiplier → NUMBER
multiplier → STRING
multiplier → target
multiplier → LPAREN formula RPAREN
multiplier → MINUS multiplier
```

> `STRING` — строку можно использовать в выражениях и выводе
> `NUMBER` — только целое число

### Почему три уровня

```
formula      →  сложение и вычитание  (низший приоритет)
   │
 addend      →  умножение и деление   (высший приоритет)
   │
multiplier   →  число, строка, переменная, скобки  (атом)
```

Чем глубже уровень — тем раньше вычисляется.

---

## 3. Устранение левой рекурсии

Левая рекурсия — нетерминал стоит в начале своей же правой части:

```
formula → formula PLUS addend   ← бесконечный цикл для нисходящего анализатора
```

### Формула устранения

```
A → A α         A → β A'
A → β     →     A' → α A'
                A' → ε
```

### formula

Было:
```
formula → formula PLUS addend
formula → formula MINUS addend
formula → addend
```

Стало:
```
formula  → addend formula'
formula' → PLUS addend formula'
formula' → MINUS addend formula'
formula' → ε
```

### addend

Было:
```
addend → addend MUL multiplier
addend → addend DIV multiplier
addend → multiplier
```

Стало:
```
addend  → multiplier addend'
addend' → MUL multiplier addend'
addend' → DIV multiplier addend'
addend' → ε
```

### operator_list

```
operator_list → operator operator_list
operator_list → ε
```

> Рекурсия **правая** — `operator_list` стоит в конце. Не трогаем.

### Полная грамматика после устранения левой рекурсии

```
program       → operator_list

operator_list → operator operator_list
operator_list → ε

operator      → array_decl
operator      → assign_op
operator      → if_op
operator      → while_op
operator      → input_op
operator      → output_op
operator      → block_body

array_decl    → ARRAY ID LBRACKET NUMBER RBRACKET SEMICOLON

assign_op     → target ASSIGN formula SEMICOLON

target        → ID
target        → ID LBRACKET formula RBRACKET

block_body    → LBRACE operator_list RBRACE

if_op         → IF LPAREN cond RPAREN block_body else_op
else_op       → ELSE block_body
else_op       → ε

while_op      → WHILE LPAREN cond RPAREN block_body

input_op      → READ LPAREN target RPAREN SEMICOLON
output_op     → WRITE LPAREN formula RPAREN SEMICOLON

cond          → formula rel_op formula

rel_op        → LT | GT | LE | GE | EQ | NE

formula       → addend formula'
formula'      → PLUS addend formula'
formula'      → MINUS addend formula'
formula'      → ε

addend        → multiplier addend'
addend'       → MUL multiplier addend'
addend'       → DIV multiplier addend'
addend'       → ε

multiplier    → NUMBER
multiplier    → STRING
multiplier    → target
multiplier    → LPAREN formula RPAREN
multiplier    → MINUS multiplier
```

---

## 4. ННФГ (Нестрогая нормальная форма Грейбах)

Каждое правило начинается с терминала или равно ε.

Для преобразования: в правиле вида `A → B α` где B — нетерминал,
заменяем B на все его правые части. Повторяем пока все правила
не будут соответствовать ННФГ.

В нашей грамматике нетерминалы в начале встречаются в:

```
operator      → assign_op | if_op | while_op | ...
assign_op     → target ASSIGN formula SEMICOLON
target        → ID | ID LBRACKET formula RBRACKET
cond          → formula rel_op formula
formula       → addend formula'
addend        → multiplier addend'
multiplier    → target | NUMBER | STRING | LPAREN formula RPAREN | MINUS multiplier
```

### Итоговая грамматика в ННФГ

```
program → operator_list

operator_list → operator operator_list
operator_list → ε

operator → ARRAY ID LBRACKET NUMBER RBRACKET SEMICOLON
operator → ID operator_tail
operator → IF LPAREN cond RPAREN block_body else_op
operator → WHILE LPAREN cond RPAREN block_body
operator → READ LPAREN ID read_tail RPAREN SEMICOLON
operator → WRITE LPAREN formula RPAREN SEMICOLON
operator → LBRACE operator_list RBRACE

operator_tail → ASSIGN formula SEMICOLON
operator_tail → LBRACKET formula RBRACKET ASSIGN formula SEMICOLON

block_body → LBRACE operator_list RBRACE

else_op → ELSE LBRACE operator_list RBRACE
else_op → ε

read_tail → LBRACKET formula RBRACKET
read_tail → ε

cond → ID cond_id_tail
cond → NUMBER rel_op formula
cond → STRING rel_op formula
cond → LPAREN formula RPAREN rel_op formula
cond → MINUS multiplier addend' rel_op formula

cond_id_tail → LBRACKET formula RBRACKET rel_op formula
cond_id_tail → rel_op formula

rel_op → LT
rel_op → GT
rel_op → LE
rel_op → GE
rel_op → EQ
rel_op → NE

formula → ID formula_id_tail
formula → NUMBER addend' formula'
formula → STRING addend' formula'
formula → LPAREN formula RPAREN addend' formula'
formula → MINUS multiplier addend' formula'

formula_id_tail → LBRACKET formula RBRACKET addend' formula'
formula_id_tail → addend' formula'

formula' → PLUS ID formula_id_tail
formula' → PLUS NUMBER addend' formula'
formula' → PLUS STRING addend' formula'
formula' → PLUS LPAREN formula RPAREN addend' formula'
formula' → PLUS MINUS multiplier addend' formula'
formula' → MINUS ID formula_id_tail
formula' → MINUS NUMBER addend' formula'
formula' → MINUS STRING addend' formula'
formula' → MINUS LPAREN formula RPAREN addend' formula'
formula' → MINUS MINUS multiplier addend' formula'
formula' → ε

addend' → MUL ID addend_id_tail
addend' → MUL NUMBER addend'
addend' → MUL STRING addend'
addend' → MUL LPAREN formula RPAREN addend'
addend' → MUL MINUS multiplier addend'
addend' → DIV ID addend_id_tail
addend' → DIV NUMBER addend'
addend' → DIV STRING addend'
addend' → DIV LPAREN formula RPAREN addend'
addend' → DIV MINUS multiplier addend'
addend' → ε

addend_id_tail → LBRACKET formula RBRACKET addend'
addend_id_tail → addend'

multiplier → ID
multiplier → ID LBRACKET formula RBRACKET
multiplier → NUMBER
multiplier → STRING
multiplier → LPAREN formula RPAREN
multiplier → MINUS multiplier
```


