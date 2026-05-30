// Code Review by Denis
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizApp.Core
{
    // --- Базові класи з UML-діаграми ---
    public class Profile
    {
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public Profile UserProfile { get; set; } // Композиція (1 до 1)
    }

    public class Admin : User
    {
        public void ManageUsers() { /* Логіка управління */ }
    }

    public class Fact
    {
        public string Content { get; set; }
        public string Source { get; set; }
        public List<Quiz> RelatedQuizzes { get; set; } = new List<Quiz>(); // Агрегація
    }

    // --- Основні класи з нетривіальною логікою ---
    public class Answer
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }

        public Answer(string text, bool isCorrect)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Текст відповіді не може бути порожнім.", nameof(text));
                
            Text = text;
            IsCorrect = isCorrect;
        }
    }

    public class Question
    {
        public string Text { get; set; }
        public int Points { get; set; }
        public List<Answer> Answers { get; private set; } // Композиція (1 до багатьох)

        public Question(string text, int points)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Текст питання не може бути порожнім.", nameof(text));
            if (points <= 0 || points > 100)
                throw new ArgumentOutOfRangeException(nameof(points), "Кількість балів має бути від 1 до 100.");

            Text = text;
            Points = points;
            Answers = new List<Answer>();
        }

        // Нетривіальний метод №1: Додавання відповіді з валідацією
        public void AddAnswer(string text, bool isCorrect)
        {
            // BVA: перевірка граничної кількості відповідей
            if (Answers.Count >= 5)
                throw new InvalidOperationException("Неможливо додати більше 5 варіантів відповідей.");

            // EP: перевірка на унікальність (ігноруючи регістр)
            foreach (var ans in Answers)
            {
                if (ans.Text.Trim().Equals(text.Trim(), StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Така відповідь вже існує у цьому питанні.");
            }

            Answers.Add(new Answer(text, isCorrect));
        }

        // Нетривіальний метод №2: Оцінювання відповідей користувача
        public int Evaluate(List<string> selectedTexts)
        {
            if (selectedTexts == null || !selectedTexts.Any())
                throw new ArgumentException("Не надано жодної відповіді для перевірки.");

            var validTexts = Answers.Select(a => a.Text).ToHashSet();
            var correctTexts = Answers.Where(a => a.IsCorrect).Select(a => a.Text).ToHashSet();

            if (!correctTexts.Any())
                throw new InvalidOperationException("У питанні не налаштовано жодної правильної відповіді.");

            // Перевірка допустимості вводу
            foreach (var st in selectedTexts)
            {
                if (!validTexts.Contains(st))
                    throw new ArgumentException($"Недопустимий варіант відповіді: {st}");
            }

            // Логіка нарахування балів (повний збіг з правильними відповідями)
            var selectedSet = selectedTexts.ToHashSet();
            if (selectedSet.SetEquals(correctTexts))
            {
                return Points;
            }
            return 0;
        }
    }

    public class Quiz
    {
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Question> Questions { get; private set; } // Композиція (1 до багатьох)

        public Quiz(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Назва тесту не може бути порожньою.", nameof(title));
                
            Title = title;
            CreatedAt = DateTime.Now;
            Questions = new List<Question>();
        }

        public void AddQuestion(Question question)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            Questions.Add(question);
        }

        // Нетривіальний метод №3: Підрахунок загального відсотка успішності
        public double CalculateSuccessRate(int userPoints)
        {
            if (userPoints < 0)
                throw new ArgumentOutOfRangeException(nameof(userPoints), "Кількість набраних балів не може бути від'ємною.");

            int totalPossible = Questions.Sum(q => q.Points);

            if (totalPossible == 0)
                throw new DivideByZeroException("Неможливо вирахувати успішність: тест не містить балів.");

            if (userPoints > totalPossible)
                throw new ArgumentOutOfRangeException(nameof(userPoints), "Набрані бали перевищують максимально можливі для цього тесту.");

            return Math.Round(((double)userPoints / totalPossible) * 100, 2);
        }
    }
}