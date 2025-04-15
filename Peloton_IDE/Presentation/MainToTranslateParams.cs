using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peloton_IDE.Presentation
{
    public class MainToTranslateParams
    {
        public MainToTranslateParams() 
        {
            LanguageID = 0;
        }
        public CustomRichEditBox? CustomREB { get; set; }
        public long LanguageID { get; set; }
    }
}
