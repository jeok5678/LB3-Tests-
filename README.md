# РЕФАКТОРИНГ КОДУ ТА ПРОВЕДЕННЯ CODE REVIEW ЗА ІНДУСТРІАЛЬНИМИ СТАНДАРТАМИ

## 1. Мета роботи

Набуття практичних навичок із проведення Code Review за індустріальними стандартами з використанням GitHub Pull Requests. Оволодіння методами ідентифікації та класифікації типових проблем якості коду (code smells). Отримання досвіду застосування рефакторинг-операцій із верифікацією збереження поведінки через регресійне тестування.

## 2. Результати Code Review одногрупника

### Мертвий метод

а) Dead Code

б)
https://github.com/denisdilog/lab2OPI.Konyshev/blob/ab26a53006d5c9cb55d361e3e464cec652f5cdce/LB3/quiz_engine.py#L8

https://github.com/denisdilog/lab2OPI.Konyshev/blob/ab26a53006d5c9cb55d361e3e464cec652f5cdce/LB3/quiz_engine.py#L40

в)
Закоментований код засмічує проєкт. Старий код варто прибрати, якщо є необхідність в його збереженні, то його можна буде переглянути в історії змін GitHub

### Використання магічних чисел

а) Magic Numbers

б) 
https://github.com/denisdilog/lab2OPI.Konyshev/blob/ab26a53006d5c9cb55d361e3e464cec652f5cdce/LB3/quiz_engine.py#L38

https://github.com/denisdilog/lab2OPI.Konyshev/blob/ab26a53006d5c9cb55d361e3e464cec652f5cdce/LB3/quiz_engine.py#L43

в)
Треба винести значення в іменовані константи, наприклад POINTS_PER_ANSWER = 10 та SPEED_BONUS = 5

### Зайва логіка та дублювання перевірок

а) Redundant Conditions

б)
https://github.com/denisdilog/lab2OPI.Konyshev/blob/ab26a53006d5c9cb55d361e3e464cec652f5cdce/LB3/quiz_engine.py#L56

в) 
Ця перевірка абсолютно зайва. Попередня перевірка if sc >= 100 вже відсікло всі значення більше ніж 100, а наступна конструкція elif sc >= 50 гарантує що sc буде меньше 100

### Неефективний обхід колекції 

а) 
https://github.com/denisdilog/lab2OPI.Konyshev/blob/ab26a53006d5c9cb55d361e3e464cec652f5cdce/LB3/quiz_engine.py#L36



б) Code Smell / Bad Practice

в)
Використовувати прямий ідіоматичний обхід елементів колекції або генераторний вираз.

### Імпорт модулів які не використовуються

а)
Unused Import

б)
https://github.com/denisdilog/lab2OPI.Konyshev/blob/ab26a53006d5c9cb55d361e3e464cec652f5cdce/LB3/quiz_engine.py#L1

в)
Модулі os та math імпортуються, але не використовуються в коді.


## 3. Посилання на PR з коментарями Code Review (URL)

### Issue
https://github.com/denisdilog/lab2OPI.Konyshev/issues/1

### Pull Request
https://github.com/denisdilog/lab2OPI.Konyshev/pull/2

## 4. Результати рефакторингу власного коду

1. Усунення "Магічних чисел"
БУЛО:

C#
if (Answers.Count >= 5)
    throw new InvalidOperationException("Неможливо додати більше 5 варіантів відповідей.");
СТАЛО:

C#
private const int MaxAnswersCount = 5;
// ...
if (Answers.Count >= MaxAnswersCount)
    throw new InvalidOperationException($"Неможливо додати більше {MaxAnswersCount} варіантів відповідей.");
ЧОМУ: Винесення захардкодженого числа 5 у пойменовану константу MaxAnswersCount робить код самодокументованим. Якщо у майбутньому бізнес-логіка зміниться (наприклад, дозволять 6 відповідей), значення потрібно буде змінити лише в одному місці.

2. Виправлення "Порушення інкапсуляції"
БУЛО:

C#
public List<Quiz> RelatedQuizzes { get; set; } = new List<Quiz>();
СТАЛО:

C#
public List<Quiz> RelatedQuizzes { get; private set; } = new List<Quiz>();

public void AddRelatedQuiz(Quiz quiz)
{
    if (quiz == null) 
        throw new ArgumentNullException(nameof(quiz), "Тест не може бути null.");
    RelatedQuizzes.Add(quiz);
}
ЧОМУ: Публічний сеттер дозволяв будь-якому зовнішньому коду повністю замінити колекцію або присвоїти їй null (fact.RelatedQuizzes = null;), що порушує цілісність об'єкта. Закриття сеттера (private set) та додавання методу AddRelatedQuiz гарантує, що модифікація списку відбувається контрольовано і з валідацією.

3. Видалення "Мертвого коду" (Порожньої заглушки)
БУЛО:

C#
public class Admin : User
{
    public void ManageUsers() { /* Логіка управління */ }
}
СТАЛО:

C#
public class Admin : User
{
    public Admin(string username, string email) : base(username, email) { }

    public void ManageUsers() 
    {
        throw new NotImplementedException("Функціонал управління користувачами ще не реалізовано.");
    }
}
ЧОМУ: Порожній метод без реалізації створює ілюзію того, що дія виконується успішно. Викидання винятку NotImplementedException явно сигналізує розробникам та системі, що цей функціонал перебуває в розробці та ще не готовий до використання.

4. Додавання "Недостатньої валідації" для сутності User
БУЛО:

C#
public class User
{
    public string Username { get; set; }
    public string Email { get; set; }
    public Profile UserProfile { get; set; }
}
СТАЛО:

C#
public class User
{
    public string Username { get; set; }
    public string Email { get; set; }
    public Profile UserProfile { get; set; }

    public User(string username, string email)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Ім'я користувача не може бути порожнім.", nameof(username));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Електронна пошта не може бути порожньою.", nameof(email));

        Username = username;
        Email = email;
    }
}
ЧОМУ: Без конструктора можна було створити "анонімного" об'єкта користувача з null або порожніми рядками замість імені та пошти. Додавання конструктора з валідацією через string.IsNullOrWhiteSpace захищає бізнес-модель від переходу в некоректний (невалідний) стан у момент створення. Також відповідно оновлено похідний клас Admin, щоб він коректно викликав базовий конструктор.

## Результати тесту
<img width="752" height="245" alt="зображення" src="https://github.com/user-attachments/assets/36073d78-ece8-4fa3-a093-5cb98c638bb5" />


## 6. Результати код ревью

### До:
<img width="1111" height="674" alt="зображення" src="https://github.com/user-attachments/assets/963fdc07-940c-46d0-8513-6c40a7953ef9" />

### Після: 
<img width="724" height="696" alt="зображення" src="https://github.com/user-attachments/assets/844c2c23-79c5-41b2-a1e5-1f62932b6571" />

### Висновки

У ході виконання роботи було успішно здобуто практичні навички проведення Code Review за індустріальними стандартами з використанням GitHub Pull Requests. Аналіз чужого та власного коду дозволив на практиці ідентифікувати й усунути типові проблеми (code smells), такі як "магічні числа", мертвий код, дублювання логіки та порушення інкапсуляції. Завдяки проведеному рефакторингу вдалося значно покращити архітектуру, надійність та читабельність коду. Всі внесені зміни були успішно верифіковані тестами, що підтвердило збереження початкової поведінки програми.
