using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualBank.ProductAdvisor.DTO
{
    public class QuestionDto
    {
        public string Category { get; set; }

        public IList<AnswerDto> PossibleAnswers { get; set; }
    }
}