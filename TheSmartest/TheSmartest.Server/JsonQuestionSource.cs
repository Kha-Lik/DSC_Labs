using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TheSmartest.Server;

public class JsonQuestionSource : IQuestionsSource
{
    private readonly string _pathToFile;

    public JsonQuestionSource(string pathToFile)
    {
        _pathToFile = pathToFile;
    }

    public IEnumerable<Question> GetQuestions()
    {
        var json = File.ReadAllText(_pathToFile);
        return JsonConvert.DeserializeObject<IEnumerable<Question>>(json);
    }
}