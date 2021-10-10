using System.Collections.Generic;

namespace TheSmartest.Server
{
    public interface IQuestionsSource
    {
        IEnumerable<Question> GetQuestions();
    }
}