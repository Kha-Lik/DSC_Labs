using System.Collections.Generic;
using System.Linq;

namespace TheSmartest.Server
{
    public class QuestionService : IQuestionService
    {
        private readonly SortedDictionary<int, Question> _questions;

        public QuestionService(IQuestionsSource questionsSource)
        {
            _questions = new SortedDictionary<int, Question>(questionsSource.GetQuestions().ToDictionary(q => q.Id));
        }

        public int GetQuestionsCount() => _questions.Count;

        public Question GetNextQuestion(int previousQuestionId) =>
            _questions.First(q => q.Key > previousQuestionId).Value;

        public bool CheckAnswer(int questionId, int answerId) =>
            _questions[questionId].CorrectAnswerId == answerId;
    }
}