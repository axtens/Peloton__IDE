using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peloton_IDE.Presentation
{
    public class TranslateToMainParams
    {
        public TranslateToMainParams()
        {
            SelectedLangauge = 0;
        }
        public RichEditBox? TranslatedREB { get; set; }
        public int SelectedLangauge { get; set; }
    }
}
