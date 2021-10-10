using System.Collections.Generic;

namespace TheSmartest.Server
{
    public class Question
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public IEnumerable<Answer> Answers { get; set; }

        public int CorrectAnswerId { get; set; }
    }
}