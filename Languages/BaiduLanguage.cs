using System.Collections.Generic;

namespace CopyPlusPlus.Languages
{
    internal class BaiduLanguage
    {
        public static Dictionary<string, string> GetLanguage = new Dictionary<string, string>()
        {
            {"检测语言", "auto"},
            {"中文", "zh"},
            {"英语", "en"},
            {"日语", "jp"},
            {"韩语", "kor"},
            {"法语", "fra"},
            {"德语", "de"},
            {"俄语", "ru"},
            {"繁体中文", "cht"},
            {"文言文", "wyw"}
        };
    }
}