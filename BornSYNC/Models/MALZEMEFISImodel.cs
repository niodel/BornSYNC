using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BornSYNC.Models
{
    public class MALZEMEFISImodel
    {
        public DateTime DATE_ { get; set; }
        public string TIME { get; set; }
        public string DOC_NUMBER { get; set; }//BORN URETIMNO
        public string OZELKOD { get; set; }
        public string YETKIKODU { get; set; }
        public string PROCREF { get; set; }//URETIMNO
        public string BRANCH { get; set; }//sube
        public string WAREHOUSE { get; set; }//depo

        public List<MALZEMEFISI_SATIRRmodel> ALTKALEMLER { get; set; }

    }
    public class MALZEMEFISI_SATIRRmodel
    {
        public string ITEM_CODE { get; set; }
        public string UNIT_CODE { get; set; }
        public string QUANTITY { get; set; }
        public string PROCREF { get; set; }//URETIMNO
        public string SPECODE { get; set; }//PARTINO
        public string SPECODE2 { get; set; }//IDREF
        public string EXP { get; set; }//IDREF


    }
}