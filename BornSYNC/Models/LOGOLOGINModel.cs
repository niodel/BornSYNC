using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BornSYNC.Models
{
    public class LOGOLOGINModel
    {
        public string LOGOUSER { get; set; }
        public string LOGOPASS { get; set; }
        public string LOGOFIRMA { get; set; }

        public LOGOLOGINModel(string _LOGOUSER, string _LOGOPASS, string _LOGOFIRMA)
        {
            this.LOGOUSER = _LOGOUSER;
            this.LOGOPASS = _LOGOPASS;
            this.LOGOFIRMA = _LOGOFIRMA;
        }

    }
}