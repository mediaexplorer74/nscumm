﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScumm.Scumm
{
    public class LanguageModel
    {
        public string LanguageName;
        public LanguageModel(Language language)
        {
            LanguageName = language.FullName;
        }
        public LanguageModel(string language)
        {
            LanguageName = language;
        }
    }
}
