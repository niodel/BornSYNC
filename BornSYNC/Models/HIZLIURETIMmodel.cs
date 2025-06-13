using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BornSYNC.Models
{
    public class HIZLIURETIMmodel
    {
        public DateTime DATE_ { get; set; }
        public string LREF { get; set; }
        public string TYPE { get; set; }
        public string PROJECTREF { get; set; }
        public string URETIMNO { get; set; }
        public string LOTNO { get; set; }
        public int ITEMREF { get; set; }
        public int AMBAR { get; set; }
        public decimal MIKTAR { get; set; }
        public int BOLUM { get; set; }

        public List<HIZLIURETIMALTKALEMLERmmodel> ALTKALEMLER { get; set; }

    }
    public class HIZLIURETIMALTKALEMLERmmodel
    {
        /// <summary>
        /// 1 Birim için Harcanacak Miktar.
        /// </summary>
        public decimal AMOUNT { get; set; }
        /// <summary>
        /// 1 Birim İçin Harcanacak Fire Miktarı
        /// </summary>
        public decimal LOSTFACTOR { get; set; }
        /// <summary>
        /// BOLUM REFERANSI
        /// </summary>
        public int DEPARTMENT { get; set; }
        /// <summary>
        /// AMBAR REFERANSI
        /// </summary>
        public int SOURCEINDEX { get; set; }
        /// <summary>
        /// ITEMREF
        /// </summary>
        public int MAINCREF { get; set; }
        /// <summary>
        /// MAMUL ALTKALEM KODU
        /// </summary>
        public string  SUBCODE { get; set; }
        /// <summary>
        /// MAMUL ALTKALEM BIRIM
        /// </summary>
        public string SUBUNIT { get; set; }


    }
}