namespace QuizApp.Core
{
    public class Profile
    {
        public string Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }

    public class User
    {
        public string Username { get; }
        public string Email { get; }
        public Profile UserProfile { get; set; } = new Profile();

        protected User(string username, string email)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Ім'я не може бути порожнім.", nameof(username));
            }
            
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Пошта не може бути порожньою.", nameof(email));
            }

            Username = username;
            Email = email;
        }

        // Переопределение использует геттеры, чтобы анализатор не считал их "мертвым кодом"
        public override string ToString()
        {
            return $"{Username} ({Email})"; 
        }
    }

    public class Admin : User
    {
        public Admin(string username, string email) : base(username, email) 
        { 
        }

        public static void ManageUsers()
        {
            throw new NotImplementedException("Функціонал у розробці.");
        }
    }

    public class Fact
    {
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        
        private readonly List<Quiz> _relatedQuizzes = new List<Quiz>();
        
        public IReadOnlyList<Quiz> RelatedQuizzes
        {
            get { return _relatedQuizzes.AsReadOnly(); }
        }

        public void AddRelatedQuiz(Quiz quiz)
        {
            if (quiz == null)
            {
                throw new ArgumentNullException(nameof(quiz));
            }
            
            _relatedQuizzes.Add(quiz);
        }
        
        public override string ToString()
        {
            return $"{Content} - {Source}";
        }
    }

    public class Answer
    {
        public string Text { get; }
        public bool IsCorrect { get; }

        public Answer(string text, bool isCorrect)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Текст не може бути порожнім.", nameof(text));
            }
            
            Text = text;
            IsCorrect = isCorrect;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class Question
    {
        private const int MaxAnswersCount = 5;
        
        public string Text { get; }
        public int Points { get; }
        
        private readonly List<Answer> _answers = new List<Answer>();
        
        public IReadOnlyList<Answer> Answers
        {
            get { return _answers.AsReadOnly(); }
        }

        public Question(string text, int points)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Текст не може бути порожнім.", nameof(text));
            }
            
            if (points <= 0 || points > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(points));
            }
            
            Text = text;
            Points = points;
        }

        public void AddAnswer(string text, bool isCorrect)
        {
            if (_answers.Count >= MaxAnswersCount)
            {
                throw new InvalidOperationException("Ліміт відповідей.");
            }
            
            if (_answers.Any(ans => ans.Text.Trim().Equals(text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Відповідь вже існує.");
            }

            _answers.Add(new Answer(text, isCorrect));
        }

        public int Evaluate(List<string> selectedTexts)
        {
            if (selectedTexts == null || selectedTexts.Count == 0)
            {
                throw new ArgumentException("Немає відповідей.");
            }

            var validTexts = _answers.Select(a => a.Text).ToHashSet();
            var correctTexts = _answers.Where(a => a.IsCorrect).Select(a => a.Text).ToHashSet();

            if (correctTexts.Count == 0)
            {
                throw new InvalidOperationException("Немає правильних відповідей.");
            }
            
            if (selectedTexts.Any(st => !validTexts.Contains(st)))
            {
                throw new ArgumentException("Недопустимий варіант.");
            }

            if (selectedTexts.ToHashSet().SetEquals(correctTexts))
            {
                return Points;
            }
            
            return 0;
        }
    }

    public class Quiz
    {
        public string Title { get; }
        public DateTime CreatedAt { get; }
        
        private readonly List<Question> _questions = new List<Question>();
        
        public IReadOnlyList<Question> Questions
        {
            get { return _questions.AsReadOnly(); }
        }

        public Quiz(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Помилка", nameof(title));
            }
            
            Title = title;
            CreatedAt = DateTime.Now;
        }

        public void AddQuestion(Question question)
        {
            if (question == null)
            {
                throw new ArgumentNullException(nameof(question));
            }
            
            _questions.Add(question);
        }

        public double CalculateSuccessRate(int userPoints)
        {
            if (userPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(userPoints));
            }

            int totalPossible = _questions.Sum(q => q.Points);
            
            if (totalPossible == 0)
            {
                throw new DivideByZeroException("Тест не містить балів.");
            }
            
            if (userPoints > totalPossible)
            {
                throw new ArgumentOutOfRangeException(nameof(userPoints));
            }

            return Math.Round(((double)userPoints / totalPossible) * 100, 2);
        }
        
        public override string ToString()
        {
            return $"{Title} ({CreatedAt})";
        }
    }
}
