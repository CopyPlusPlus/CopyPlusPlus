using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyPlusPlus
{
    class Language
    {
        public static Dictionary<string, string> GetLanguage = new Dictionary<string, string>()
        {
            {"检测语言", "auto"},
            {"中文", "zh"},
            {"英语", "en"},
            {"粤语", "yue"},
            {"文言文", "wyw"},
            {"日语", "jp"},
            {"韩语", "kor"},
            {"法语", "fra"},
            {"德语", "de"},
            {"俄语", "ru"},
            {"繁体中文", "cht"}
        };
    }
}
