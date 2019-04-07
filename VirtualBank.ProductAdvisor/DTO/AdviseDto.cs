using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualBank.ProductAdvisor.DTO
{
    public class AdviseDto
    {
        public AnswerDigest Answers { get; set; }

        public BundleDto SelectedBundle { get; set; }
    }

    /// <summary>
    /// User answers in short
    /// </summary>
    public class AnswerDigest
    {
        public int Age { get; set; }
        public int Student { get; set; }
        public int Income { get; set; }
    }
}