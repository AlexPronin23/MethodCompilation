# MethodCompilation

# Лексический анализатор

## 1. Список лексем

| № | Лексема | Пример | Описание |
|---|---------|--------|----------|
| 1 | `ID` | `x`, `abc`, `a1` | Имя переменной или массива |
| 2 | `NUMBER` | `42`, `7` | Целое число |
| 3 | `STRING` | `"hello"` | Строковый литерал |
| 4 | `QUOTE` | `"` | Кавычка — ограничитель строки |
| 5 | `PLUS` | `+` | Сложение |
| 6 | `MINUS` | `-` | Вычитание |
| 7 | `MUL` | `*` | Умножение |
| 8 | `DIV` | `/` | Деление |
| 9 | `ASSIGN` | `:=` | Присваивание |
| 10 | `SEMICOLON` | `;` | Конец оператора |
| 11 | `LPAREN` | `(` | Левая круглая скобка |
| 12 | `RPAREN` | `)` | Правая круглая скобка |
| 13 | `LBRACE` | `{` | Начало блока |
| 14 | `RBRACE` | `}` | Конец блока |
| 15 | `LBRACKET` | `[` | Начало индекса массива |
| 16 | `RBRACKET` | `]` | Конец индекса массива |
| 17 | `LT` | `<` | Меньше |
| 18 | `GT` | `>` | Больше |
| 19 | `LE` | `<=` | Меньше или равно |
| 20 | `GE` | `>=` | Больше или равно |
| 21 | `EQ` | `=` | Равно |
| 22 | `NE` | `<>` | Не равно |
| 23 | `IF` | `if` | Ключевое слово if |
| 24 | `ELSE` | `else` | Ключевое слово else |
| 25 | `WHILE` | `while` | Ключевое слово while |
| 26 | `READ` | `read` | Оператор ввода |
| 27 | `WRITE` | `write` | Оператор вывода |
| 28 | `ARRAY` | `array` | Объявление массива |
| 29 | `EOF` | — | Конец файла/программы |

> `QUOTE` как отдельная лексема не возвращается — она служит сигналом для автомата
> перейти в состояние `Q` (чтение строки). Результатом является лексема `STRING`
> которая содержит всё что было между кавычками.

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
> Символ `"` (QUOTE) не накапливается в буфер — он только переключает состояние автомата в `Q`.

### Состояния автомата

| Состояние | Описание |
|-----------|----------|
| `S` | Старт — ждём начало новой лексемы |
| `I` | Читаем идентификатор (буквы/цифры) |
| `N` | Читаем целое число |
| `P` | Прочитали `:`, ждём `=` → это `:=` |
| `E` | Прочитали `<`, ждём `=` или `>` |
| `H` | Прочитали `>`, ждём `=` |
| `Q` | Прочитали `QUOTE` — читаем строку внутри кавычек |
| `Z` | Финал — лексема готова, символ съеден |
| `Z*` | Финал — лексема готова, текущий символ вернуть назад |
| `ERR` | Ошибка — недопустимый символ |

### Таблица переходов

| Состояние | `<б>` | `<ц>` | `<пр>` | `+` | `-` | `*` | `/` | `=` | `:` | `<` | `>` | `(` | `)` | `{` | `}` | `[` | `]` | `;` | `"` (QUOTE) | `<any>` | `⊥` |
|-----------|-------|-------|--------|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-------------|---------|-----|
| `S` | I | N | S | Z | Z | Z | Z | Z | P | E | H | Z | Z | Z | Z | Z | Z | Z | Q | ERR | Z |
| `I` | I | I | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `N` | Z* | N | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `P` | ERR | ERR | ERR | ERR | ERR | ERR | ERR | Z | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR | ERR |
| `E` | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z | Z* | Z* | Z | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `H` | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* | Z* |
| `Q` | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Q | Z | Q | ERR |

> `Z` — лексема готова, текущий символ принадлежит ей (съеден).
> `Z*` — лексема готова, текущий символ вернуть во входной поток.
> Состояние `Q` — активируется лексемой `QUOTE`, накапливаем всё до закрывающей `QUOTE`.

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
| `Q` | `"` (QUOTE) | STRING (содержимое без кавычек) |
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
| `S` | `"` (QUOTE) | `Q` | 0 | Начало строки — QUOTE не накапливаем, переходим в Q |
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
| `Q` | `"` (QUOTE) | `Z` | 2 | Закрывающая QUOTE — вернуть STRING |
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
multiplier → QUOTE STRING QUOTE
multiplier → target
multiplier → LPAREN formula RPAREN
multiplier → MINUS multiplier
```

> `QUOTE STRING QUOTE` — строка всегда обёрнута в кавычки
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
multiplier    → QUOTE STRING QUOTE
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
multiplier    → target | NUMBER | QUOTE STRING QUOTE | LPAREN formula RPAREN | MINUS multiplier
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
cond → QUOTE STRING QUOTE rel_op formula
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
formula → QUOTE STRING QUOTE addend' formula'
formula → LPAREN formula RPAREN addend' formula'
formula → MINUS multiplier addend' formula'

formula_id_tail → LBRACKET formula RBRACKET addend' formula'
formula_id_tail → addend' formula'

formula' → PLUS ID formula_id_tail
formula' → PLUS NUMBER addend' formula'
formula' → PLUS QUOTE STRING QUOTE addend' formula'
formula' → PLUS LPAREN formula RPAREN addend' formula'
formula' → PLUS MINUS multiplier addend' formula'
formula' → MINUS ID formula_id_tail
formula' → MINUS NUMBER addend' formula'
formula' → MINUS QUOTE STRING QUOTE addend' formula'
formula' → MINUS LPAREN formula RPAREN addend' formula'
formula' → MINUS MINUS multiplier addend' formula'
formula' → ε

addend' → MUL ID addend_id_tail
addend' → MUL NUMBER addend'
addend' → MUL QUOTE STRING QUOTE addend'
addend' → MUL LPAREN formula RPAREN addend'
addend' → MUL MINUS multiplier addend'
addend' → DIV ID addend_id_tail
addend' → DIV NUMBER addend'
addend' → DIV QUOTE STRING QUOTE addend'
addend' → DIV LPAREN formula RPAREN addend'
addend' → DIV MINUS multiplier addend'
addend' → ε

addend_id_tail → LBRACKET formula RBRACKET addend'
addend_id_tail → addend'

multiplier → ID
multiplier → ID LBRACKET formula RBRACKET
multiplier → NUMBER
multiplier → QUOTE STRING QUOTE
multiplier → LPAREN formula RPAREN
multiplier → MINUS multiplier
```

> Нетерминалы `formula_id_tail` и `addend_id_tail` введены для факторизации
> правил начинающихся с `ID` — после него возможны либо `[` (массив) либо
> продолжение. Это обязательное условие LL(1)-анализатора.

---

## 5. Семантические действия для генерации ОПС

Генерация ОПС выполняется одновременно с работой LL(1)-анализатора.
Для каждого правила грамматики задана последовательность семантических действий —
по одному на каждый символ правой части. Действия хранятся во втором магазине
и выполняются синхронно с основным магазином анализатора.

### Обозначения семантических действий

| Обозначение | Смысл |
|-------------|-------|
| `—` | Нет действия — символ обрабатывается без генерации ОПС |
| `{a}` | Записать в ОПС ссылку на переменную (TYPE_VAR) |
| `{k}` | Записать в ОПС ссылку на целочисленную константу (TYPE_CONST) |
| `{ks}` | Записать в ОПС ссылку на строковую константу (TYPE_STR_CONST) |
| `{:=}` | Записать в ОПС операцию присваивания |
| `{i}` | Записать в ОПС операцию индексирования массива |
| `{+}` `{-}` `{*}` `{/}` | Записать в ОПС арифметическую операцию |
| `{-'}` | Записать в ОПС операцию унарного минуса |
| `{<}` `{>}` `{<=}` `{>=}` `{=}` `{<>}` | Записать в ОПС операцию сравнения |
| `{r}` | Записать в ОПС операцию ввода |
| `{w}` | Записать в ОПС операцию вывода числа |
| `{ws}` | Записать в ОПС операцию вывода строки |
| `{П1}` `{П2}` `{П3}` `{П4}` `{П5}` | Вызов соответствующей семантической программы |

---

### Описание семантических программ

**Программа П1** — вызывается после условия `if`:
1. В магазин меток записывается текущее значение счётчика `k`.
2. В ОПС записывается пустое место под будущую метку перехода при false.
3. В ОПС записывается операция `jf`.

**Программа П2** — вызывается в начале `else`:
1. Заполняется пустое место от П1 значением `k + 2`.
2. В магазин меток записывается текущее `k`.
3. В ОПС записывается пустое место под метку безусловного перехода.
4. В ОПС записывается операция `j`.

**Программа П3** — вызывается в конце `if` или `if-else`:
1. Заполняется пустое место от П2 текущим значением `k`.

**Программа П4** — вызывается перед условием `while`:
1. В магазин меток записывается текущее `k` — адрес начала цикла.

**Программа П5** — вызывается после тела `while`:
1. Заполняется пустое место от П1 значением `k + 2`.
2. В ОПС записывается адрес начала цикла из магазина меток.
3. В ОПС записывается операция `j`.

---

### Таблица семантических действий

#### Присваивание

| Нетерминал | Правая часть | Сем. действия |
|------------|-------------|--------------|
| `operator` | `ID  operator_tail` | `{a}  —` |
| `operator_tail` | `ASSIGN  formula  SEMICOLON` | `—  —  {:=}` |
| `operator_tail` | `LBRACKET  formula  RBRACKET  ASSIGN  formula  SEMICOLON` | `—  —  {i}  —  —  {:=}` |

#### Объявление массива

| Нетерминал | Правая часть | Сем. действия |
|------------|-------------|--------------|
| `operator` | `ARRAY  ID  LBRACKET  NUMBER  RBRACKET  SEMICOLON` | `—  —  —  —  —  —` |

#### Выражения

| Нетерминал | Правая часть | Сем. действия |
|------------|-------------|--------------|
| `formula` | `ID  formula_id_tail` | `{a}  —` |
| `formula` | `NUMBER  addend'  formula'` | `{k}  —  —` |
| `formula` | `QUOTE  STRING  QUOTE  addend'  formula'` | `—  {ks}  —  —  —` |
| `formula` | `LPAREN  formula  RPAREN  addend'  formula'` | `—  —  —  —  —` |
| `formula` | `MINUS  multiplier  addend'  formula'` | `—  —  {-'}  —` |
| `formula_id_tail` | `LBRACKET  formula  RBRACKET  addend'  formula'` | `—  —  {i}  —  —` |
| `formula_id_tail` | `addend'  formula'` | `—  —` |
| `formula'` | `PLUS  addend  formula'` | `—  —  {+}` |
| `formula'` | `MINUS  addend  formula'` | `—  —  {-}` |
| `formula'` | `ε` | — |
| `addend` | `multiplier  addend'` | `—  —` |
| `addend'` | `MUL  multiplier  addend'` | `—  —  {*}` |
| `addend'` | `DIV  multiplier  addend'` | `—  —  {/}` |
| `addend'` | `ε` | — |
| `multiplier` | `ID` | `{a}` |
| `multiplier` | `ID  LBRACKET  formula  RBRACKET` | `{a}  —  —  {i}` |
| `multiplier` | `NUMBER` | `{k}` |
| `multiplier` | `QUOTE  STRING  QUOTE` | `—  {ks}  —` |
| `multiplier` | `LPAREN  formula  RPAREN` | `—  —  —` |
| `multiplier` | `MINUS  multiplier` | `—  {-'}` |

#### Условие

| Нетерминал | Правая часть | Сем. действия |
|------------|-------------|--------------|
| `cond` | `ID  cond_id_tail` | `{a}  —` |
| `cond` | `NUMBER  rel_op  formula` | `{k}  —  —` |
| `cond` | `QUOTE  STRING  QUOTE  rel_op  formula` | `—  {ks}  —  —  —` |
| `cond` | `LPAREN  formula  RPAREN  rel_op  formula` | `—  —  —  —  —` |
| `cond_id_tail` | `LBRACKET  formula  RBRACKET  rel_op  formula` | `—  —  {i}  —  —` |
| `cond_id_tail` | `rel_op  formula` | `—  —` |
| `rel_op` | `LT` | `{<}` |
| `rel_op` | `GT` | `{>}` |
| `rel_op` | `LE` | `{<=}` |
| `rel_op` | `GE` | `{>=}` |
| `rel_op` | `EQ` | `{=}` |
| `rel_op` | `NE` | `{<>}` |

#### Условный оператор if

| Нетерминал | Правая часть | Сем. действия |
|------------|-------------|--------------|
| `if_op` | `IF  LPAREN  cond  RPAREN  block_body  else_op` | `—  —  —  {П1}  —  —  {П3}` |
| `else_op` | `ELSE  block_body` | `{П2}  —` |
| `else_op` | `ε` | — |

#### Цикл while

| Нетерминал | Правая часть | Сем. действия |
|------------|-------------|--------------|
| `while_op` | `WHILE  LPAREN  cond  RPAREN  block_body` | `{П4}  —  —  —  {П1}  —  {П5}` |

#### Ввод и вывод

| Нетерминал | Правая часть | Сем. действия |
|------------|-------------|--------------|
| `input_op` | `READ  LPAREN  ID  RPAREN  SEMICOLON` | `—  —  {a}  {r}  —` |
| `input_op` | `READ  LPAREN  ID  LBRACKET  formula  RBRACKET  RPAREN  SEMICOLON` | `—  —  {a}  —  —  {i}  {r}  —` |
| `output_op` | `WRITE  LPAREN  formula  RPAREN  SEMICOLON` | `—  —  —  {w}  —` |
| `output_op` | `WRITE  LPAREN  QUOTE  STRING  QUOTE  RPAREN  SEMICOLON` | `—  —  —  {ks}  —  {ws}  —` |

#### Составной оператор (блок)

| Нетерминал | Правая часть | Сем. действия |
|------------|-------------|--------------|
| `block_body` | `LBRACE  operator_list  RBRACE` | `—  —  —` |
| `operator_list` | `operator  operator_list` | `—  —` |
| `operator_list` | `ε` | — |

---

# Список операций ОПС

| № | Операция | Обозначение | Арность |
|---|----------|-------------|---------|
| 1 | Сложение | `+` | 2 |
| 2 | Вычитание | `–` | 2 |
| 3 | Умножение | `*` | 2 |
| 4 | Деление | `/` | 2 |
| 5 | Унарный минус | `–'` | 1 |
| 6 | Присваивание | `:=` | 2 |
| 7 | Индексирование массива | `i` | 2 |
| 8 | Меньше | `<` | 2 |
| 9 | Больше | `>` | 2 |
| 10 | Меньше или равно | `<=` | 2 |
| 11 | Больше или равно | `>=` | 2 |
| 12 | Равно | `=` | 2 |
| 13 | Не равно | `<>` | 2 |
| 14 | Условный переход по false | `jf` | 2 |
| 15 | Безусловный переход | `j` | 1 |
| 16 | Ввод | `r` | 1 |
| 17 | Вывод числа | `w` | 1 |
| 18 | Вывод строки | `ws` | 1 |

---

# Формат ОПС

ОПС — линейный массив элементов. Каждый элемент состоит из двух полей: тип и значение.

## Поле `type`

| Значение | Описание |
|----------|----------|
| `TYPE_VAR` | Ссылка на переменную в таблице переменных |
| `TYPE_CONST` | Ссылка на константу в таблице констант |
| `TYPE_STR_CONST` | Ссылка на строку в таблице строковых констант |
| `TYPE_LABEL` | Метка — номер элемента ОПС (адрес перехода) |
| `TYPE_OP` | Операция |

## Поле `value`

| `type` | Содержимое `value` | Пример |
|--------|-------------------|--------|
| `TYPE_VAR` | Индекс в таблице переменных | `2` |
| `TYPE_CONST` | Индекс в таблице числовых констант | `0` |
| `TYPE_STR_CONST` | Индекс в таблице строковых констант | `0` |
| `TYPE_LABEL` | Номер элемента ОПС | `7` |
| `TYPE_OP` | Код операции | `OP_ADD`, `OP_JF`, `OP_I` |

## Виды содержимого в магазине интерпретатора

| Вид | Описание |
|-----|----------|
| Ссылка на переменную | Адрес в таблице переменных |
| Ссылка на константу | Адрес в таблице констант |
| Числовое значение | Результат арифметической операции |
| Ссылка на массив | Адрес паспорта массива |
| Ссылка на элемент массива | Вычисленный адрес `M + k` |

---

## Примеры генерации ОПС

### Пример 1. Присваивание с унарным минусом

```
x := -5 + a;
```

ОПС: `x  5  –'  a  +  :=`

| Индекс | Тип | Значение |
|--------|-----|----------|
| 0 | `TYPE_VAR` | ссылка на `x` |
| 1 | `TYPE_CONST` | ссылка на `5` |
| 2 | `TYPE_OP` | `OP_NEG` |
| 3 | `TYPE_VAR` | ссылка на `a` |
| 4 | `TYPE_OP` | `OP_ADD` |
| 5 | `TYPE_OP` | `OP_ASSIGN` |

---

### Пример 2. Присваивание элементу массива

```
array b[5];
b[2] := b[0] + b[1];
```

ОПС: `b  2  i  b  0  i  b  1  i  +  :=`

| Индекс | Тип | Значение |
|--------|-----|----------|
| 0 | `TYPE_VAR` | ссылка на `b` |
| 1 | `TYPE_CONST` | ссылка на `2` |
| 2 | `TYPE_OP` | `OP_I` |
| 3 | `TYPE_VAR` | ссылка на `b` |
| 4 | `TYPE_CONST` | ссылка на `0` |
| 5 | `TYPE_OP` | `OP_I` |
| 6 | `TYPE_VAR` | ссылка на `b` |
| 7 | `TYPE_CONST` | ссылка на `1` |
| 8 | `TYPE_OP` | `OP_I` |
| 9 | `TYPE_OP` | `OP_ADD` |
| 10 | `TYPE_OP` | `OP_ASSIGN` |

---

### Пример 3. Цикл while с массивом

```
while (i < 10) {
    a[i] := i * 2;
    i := i + 1;
}
```

ОПС: `i  10  <  m1  jf  a  i  i  i  2  *  :=  i  i  1  +  :=  m0  j`

| Индекс | Тип | Значение |
|--------|-----|----------|
| 0 | `TYPE_VAR` | `i` (← m0) |
| 1 | `TYPE_CONST` | `10` |
| 2 | `TYPE_OP` | `OP_LT` |
| 3 | `TYPE_LABEL` | `18` (m1) |
| 4 | `TYPE_OP` | `OP_JF` |
| 5 | `TYPE_VAR` | `a` |
| 6 | `TYPE_VAR` | `i` |
| 7 | `TYPE_OP` | `OP_I` |
| 8 | `TYPE_VAR` | `i` |
| 9 | `TYPE_CONST` | `2` |
| 10 | `TYPE_OP` | `OP_MUL` |
| 11 | `TYPE_OP` | `OP_ASSIGN` |
| 12 | `TYPE_VAR` | `i` |
| 13 | `TYPE_VAR` | `i` |
| 14 | `TYPE_CONST` | `1` |
| 15 | `TYPE_OP` | `OP_ADD` |
| 16 | `TYPE_OP` | `OP_ASSIGN` |
| 17 | `TYPE_LABEL` | `0` (m0) |
| 18 | `TYPE_OP` | `OP_J` |

---

### Пример 4. Условный оператор if без else

```
if (x <> 0) {
    write(x);
}
```

ОПС: `x  0  <>  m1  jf  x  w`

| Индекс | Тип | Значение |
|--------|-----|----------|
| 0 | `TYPE_VAR` | `x` |
| 1 | `TYPE_CONST` | `0` |
| 2 | `TYPE_OP` | `OP_NE` |
| 3 | `TYPE_LABEL` | `7` (m1) |
| 4 | `TYPE_OP` | `OP_JF` |
| 5 | `TYPE_VAR` | `x` |
| 6 | `TYPE_OP` | `OP_W` |
| 7 | — | — (← m1) |

---

### Пример 5. Ввод и вывод с условием

```
read(n);
if (n > 0) {
    write("positive");
}
else {
    write("not positive");
}
```

ОПС: `n  r  n  0  >  m1  jf  "positive"  ws  m2  j  "not positive"  ws`

| Индекс | Тип | Значение |
|--------|-----|----------|
| 0 | `TYPE_VAR` | `n` |
| 1 | `TYPE_OP` | `OP_R` |
| 2 | `TYPE_VAR` | `n` |
| 3 | `TYPE_CONST` | `0` |
| 4 | `TYPE_OP` | `OP_GT` |
| 5 | `TYPE_LABEL` | `11` (m1) |
| 6 | `TYPE_OP` | `OP_JF` |
| 7 | `TYPE_STR_CONST` | `"positive"` |
| 8 | `TYPE_OP` | `OP_WS` |
| 9 | `TYPE_LABEL` | `13` (m2) |
| 10 | `TYPE_OP` | `OP_J` |
| 11 | `TYPE_STR_CONST` | `"not positive"` (← m1) |
| 12 | `TYPE_OP` | `OP_WS` |
| 13 | — | — (← m2) |