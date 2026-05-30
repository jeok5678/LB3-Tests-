using System;
using System.Collections.Generic;
using Xunit;
using QuizApp.Core; // Твій основний простір імен

namespace QuizApp.Tests
{
    public class QuizModuleTests
    {
        // ==========================================
        // Тести для методу AddAnswer (Клас Question)
        // ==========================================

        [Fact]
        public void AddAnswer_ValidAnswer_AddsToList()
        {
            // Arrange
            var question = new Question("Яка столиця України?", 10);
            
            // Act
            question.AddAnswer("Київ", true);

            // Assert
            // Позитивний тест / EP: успішне додавання валідної відповіді
            Assert.Single(question.Answers);
            Assert.Equal("Київ", question.Answers[0].Text);
        }

        [Fact]
        public void AddAnswer_EmptyString_ThrowsArgumentException()
        {
            // Arrange
            var question = new Question("Питання?", 10);

            // Act & Assert 
            // (У xUnit для винятків Act і Assert об'єднуються)
            // Негативний тест / EP / BVA: Спроба додати порожній рядок
            Assert.Throws<ArgumentException>(() => question.AddAnswer(" ", false));
        }

        [Fact]
        public void AddAnswer_DuplicateAnswerDifferentCase_ThrowsArgumentException()
        {
            // Arrange
            var question = new Question("Питання?", 10);
            question.AddAnswer("Київ", true);

            // Act & Assert
            // Негативний тест / EP: Спроба додати дублікат
            Assert.Throws<ArgumentException>(() => question.AddAnswer("КИЇВ", false));
        }

        [Fact]
        public void AddAnswer_FifthAnswer_AddsSuccessfully()
        {
            // Arrange
            var question = new Question("Питання?", 10);
            question.AddAnswer("В1", false);
            question.AddAnswer("В2", false);
            question.AddAnswer("В3", false);
            question.AddAnswer("В4", false);

            // Act
            question.AddAnswer("В5", true);

            // Assert
            // Позитивний тест / BVA: Додавання 5-ї (граничної) відповіді
            Assert.Equal(5, question.Answers.Count);
        }

        [Fact]
        public void AddAnswer_SixthAnswer_ThrowsInvalidOperationException()
        {
            // Arrange
            var question = new Question("Питання?", 10);
            question.AddAnswer("В1", false);
            question.AddAnswer("В2", false);
            question.AddAnswer("В3", false);
            question.AddAnswer("В4", false);
            question.AddAnswer("В5", true);

            // Act & Assert
            // Негативний тест / BVA: Перевищення ліміту відповідей
            Assert.Throws<InvalidOperationException>(() => question.AddAnswer("В6", false));
        }

        // ==========================================
        // Тести для методу Evaluate (Клас Question)
        // ==========================================

        [Fact]
        public void Evaluate_AllCorrectAnswers_ReturnsFullPoints()
        {
            // Arrange
            var question = new Question("Виберіть кольори світлофора", 10);
            question.AddAnswer("Червоний", true);
            question.AddAnswer("Зелений", true);
            question.AddAnswer("Синій", false);
            var userSelection = new List<string> { "Червоний", "Зелений" };

            // Act
            int result = question.Evaluate(userSelection);

            // Assert
            // Позитивний тест / EP: Всі правильні відповіді
            Assert.Equal(10, result);
        }

        [Fact]
        public void Evaluate_PartiallyCorrect_ReturnsZero()
        {
            // Arrange
            var question = new Question("Кольори світлофора", 10);
            question.AddAnswer("Червоний", true);
            question.AddAnswer("Зелений", true);
            var userSelection = new List<string> { "Червоний" };

            // Act
            int result = question.Evaluate(userSelection);

            // Assert
            // Негативний тест / EP: Частково правильна відповідь (не всі обрані)
            Assert.Equal(0, result);
        }

        [Fact]
        public void Evaluate_EmptySelection_ThrowsArgumentException()
        {
            // Arrange
            var question = new Question("Кольори", 10);
            question.AddAnswer("Червоний", true);

            // Act & Assert
            // Негативний тест / BVA: Передача порожнього масиву відповідей
            Assert.Throws<ArgumentException>(() => question.Evaluate(new List<string>()));
        }

        [Fact]
        public void Evaluate_InvalidOption_ThrowsArgumentException()
        {
            // Arrange
            var question = new Question("Кольори", 10);
            question.AddAnswer("Червоний", true);
            var userSelection = new List<string> { "Фіолетовий" };

            // Act & Assert
            // Негативний тест / EP: Передача неіснуючого варіанту (спроба зламу)
            Assert.Throws<ArgumentException>(() => question.Evaluate(userSelection));
        }

        // ==========================================
        // Тести для CalculateSuccessRate (Клас Quiz)
        // ==========================================

        [Fact]
        public void CalculateSuccessRate_ValidPoints_ReturnsCorrectPercentage()
        {
            // Arrange
            var quiz = new Quiz("Тест з історії");
            quiz.AddQuestion(new Question("П1", 20));
            quiz.AddQuestion(new Question("П2", 30)); // Максимум 50 балів

            // Act
            double result = quiz.CalculateSuccessRate(25);

            // Assert
            // Позитивний тест / EP: Допустима середня кількість балів
            Assert.Equal(50.00, result);
        }

        [Fact]
        public void CalculateSuccessRate_NegativePoints_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var quiz = new Quiz("Тест");
            quiz.AddQuestion(new Question("П1", 10));

            // Act & Assert
            // Негативний тест / BVA: Від'ємна кількість балів
            Assert.Throws<ArgumentOutOfRangeException>(() => quiz.CalculateSuccessRate(-1));
        }

        [Fact]
        public void CalculateSuccessRate_ZeroPoints_ReturnsZero()
        {
            // Arrange
            var quiz = new Quiz("Тест");
            quiz.AddQuestion(new Question("П1", 10));

            // Act
            double result = quiz.CalculateSuccessRate(0);

            // Assert
            // Позитивний тест / BVA: Нульова кількість балів (мінімум)
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void CalculateSuccessRate_MaxPoints_ReturnsHundred()
        {
            // Arrange
            var quiz = new Quiz("Тест");
            quiz.AddQuestion(new Question("П1", 50));

            // Act
            double result = quiz.CalculateSuccessRate(50);

            // Assert
            // Позитивний тест / BVA: Максимально можлива кількість балів
            Assert.Equal(100.0, result);
        }

        [Fact]
        public void CalculateSuccessRate_QuizWithNoPoints_ThrowsDivideByZeroException()
        {
            // Arrange
            var quiz = new Quiz("Порожній тест"); // Немає питань, 0 балів максимум

            // Act & Assert
            // Негативний тест / EP: Розрахунок у тесті без балів
            Assert.Throws<DivideByZeroException>(() => quiz.CalculateSuccessRate(0));
        }
    }
}