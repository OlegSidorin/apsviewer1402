using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apsviewer1402.Library
{
    public class SignedUrl
    {
        public string uploadKey { get; set; }
        public DateTime uploadExpiration { get; set; }
        public DateTime urlExpiration { get; set; }
        public string[] urls { get; set; }
    }

}
