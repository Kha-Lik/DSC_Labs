namespace TheSmartest.Server
{
    public interface IQuestionService
    {
        int GetQuestionsCount();
        Question GetNextQuestion(int previousQuestionId);
        bool CheckAnswer(int questionId, int answerId);
    }
}