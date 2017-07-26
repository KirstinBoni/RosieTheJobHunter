using System;
using System.Collections.Generic;

namespace RosieTheJobHunter.Model
{
    public class Response
    {
        public string id { get; set; }
        public IList<string> keyPhrases { get; set; }
    }
}