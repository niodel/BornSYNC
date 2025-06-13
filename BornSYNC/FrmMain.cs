using BornSYNC.Helper;
using BornSYNC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BornSYNC
{
    public partial class FrmMain : Form
    {

        public string ATLASCON = ConfigurationManager.AppSettings["ATLASCON"].ToString();
        public string LOGOCON = ConfigurationManager.AppSettings["LOGOCON"].ToString();
        public string BORNCON = ConfigurationManager.AppSettings["BORNCON"].ToString();

        public string ATLASDB = ConfigurationManager.AppSettings["ATLASDB"].ToString();
        public string LOGODB = ConfigurationManager.AppSettings["LOGODB"].ToString();
        public string BORNDB = ConfigurationManager.AppSettings["BORNDB"].ToString();

        public string LOGOUSER = ConfigurationManager.AppSettings["LOGOUSER"].ToString();
        public string LOGOPASS = ConfigurationManager.AppSettings["LOGOPASS"].ToString();
        public string FIRMANO = ConfigurationManager.AppSettings["FIRMANO"].ToString();
        public string FIRMADONEM = ConfigurationManager.AppSettings["FIRMADONEM"].ToString();


        public LOGOLOGINModel LoginModel;
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            LoginModel = new LOGOLOGINModel(LOGOUSER, LOGOPASS, FIRMANO);
            //timer1.Start();
            timer1_Tick(null, null);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            timer1.Stop();
            try
            {
                AdmUretimFisAktar();
            }
            catch (Exception ex)
            {
                SqlHelper.Logger(ex.Message);
            }

            try
            {
                //AdmSanalUretimFisAktar();
            }
            catch (Exception ex)
            {
                SqlHelper.Logger(ex.Message);
            }
            timer1.Start();
            Cursor.Current = Cursors.Arrow;
        }


        #region Uretim
        public void AdmUretimFisAktar()
        {
            //üRETİM GİRİŞLERİNİ AKTAR
            var aktarilmamisKayitlar = GetUretimListesiFromAnahtar();
            foreach (DataRow row in aktarilmamisKayitlar.Rows)
            {
                //yeniden kontrol et var mi


                //var malzemetur = row["STOK_TURU"] + "";
                string URETIMNO = row["URETIMNO"] + "";
                string TARIH = row["TARIH"] + "";
                string PARTINO = URETIMNO; // row["PARTINO"] + "";
                string IDREF = URETIMNO; // row["IDREF"] + "";
                DateTime dtTarih = DateTime.Parse(TARIH);
                String newTarih = dtTarih.Year.ToString() + "-" + dtTarih.Month.ToString().PadLeft(2, '0') + "-" + dtTarih.Day.ToString().PadLeft(2, '0');

                MESAJmodel result = new MESAJmodel();
                result = UretimFisiAktar(row);

                if (result.HasError)
                    UretimLogla(URETIMNO, newTarih, 0, result.Message, "", "MM");
                else
                    UretimLogla(URETIMNO, newTarih, 1, result.Message, result.Data + "", "MM");


                /*  eski yöntem
                var rw = GetUretimSatiriFromAnahtar(IDREF).Rows[0];
                if (rw["STATUS"] + "" == "1")
                {
                    continue;
                }


                MESAJmodel result = new MESAJmodel();
                if (malzemetur == "MM")
                {
                    result = UretimFisiAktar(row);
                }
                else if (malzemetur == "HM")
                {
                    result = SarfFisiAktar(row, row);
                }

                if (result.HasError)
                    UretimLogla(URETIMNO, PARTINO, 0, result.Message, "", IDREF);
                else
                    UretimLogla(URETIMNO, PARTINO, 1, result.Message, result.Data + "", IDREF);
                */

            }

            //SARF ÇIKIŞLARINI AKTARRRRR
            var aktarilmamisSarfKayitlar = GetSarfListesiFromAnahtar();
            foreach (DataRow row in aktarilmamisSarfKayitlar.Rows)
            {
                //yeniden kontrol et var mi

                string URETIMNO = row["URETIMNO"] + "";
                string TARIH = row["TARIH"] + "";
                DateTime dtTarih = DateTime.Parse(TARIH);
                String newTarih = dtTarih.Year.ToString() + "-" + dtTarih.Month.ToString().PadLeft(2, '0') + "-" + dtTarih.Day.ToString().PadLeft(2, '0');

                var sarfDataTable = GetUretimSarfKayitlari(URETIMNO, TARIH);

                if(sarfDataTable.Rows.Count > 0)
                {
                    MESAJmodel result = new MESAJmodel();

                    result = SarfFisiAktar(sarfDataTable);


                    if (result.HasError)
                        UretimLogla(URETIMNO, newTarih, 0, result.Message, "", "HM");
                    else
                        UretimLogla(URETIMNO, newTarih, 1, result.Message, result.Data + "", "HM");
                }


                /*  eski yöntem
                var rw = GetUretimSatiriFromAnahtar(IDREF).Rows[0];
                if (rw["STATUS"] + "" == "1")
                {
                    continue;
                }


                MESAJmodel result = new MESAJmodel();
                if (malzemetur == "MM")
                {
                    result = UretimFisiAktar(row);
                }
                else if (malzemetur == "HM")
                {
                    result = SarfFisiAktar(row, row);
                }

                if (result.HasError)
                    UretimLogla(URETIMNO, PARTINO, 0, result.Message, "", IDREF);
                else
                    UretimLogla(URETIMNO, PARTINO, 1, result.Message, result.Data + "", IDREF);
                */

            }
        }

        public DataTable GetUretimListesiFromAnahtar()
        {
            var sql = "SELECT * FROM ADV_" + FIRMANO + "_BORNURETIM WHERE STATUS<>1 ORDER BY TARIH ASC";
            sql = " SELECT URETIMNO,STOCKREF,TARIH,STOK_KODU,STOK_ADI,GRUP_KODU,BIRIM,SUM(URETIM_MIKTAR) URETIM_MIKTAR,SUM(SARF_MIKTAR) SARF_MIKTAR "+
                  " FROM[ADV_" + FIRMANO + "_BORNURETIM] " +
                  " WHERE STATUS<>1 " +
                  " AND STOK_TURU = 'MM' " +
                  " GROUP BY URETIMNO,STOCKREF,TARIH,STOK_KODU,STOK_ADI,GRUP_KODU,BIRIM " +
                  " ORDER BY TARIH, URETIMNO ASC";
            DataTable dt = SqlHelper.TabloGetir(sql, ATLASCON);
            return dt;
        }

        public DataTable GetSarfListesiFromAnahtar()
        {
            var sql = "SELECT * FROM ADV_" + FIRMANO + "_BORNURETIM WHERE STATUS<>1 ORDER BY TARIH ASC";
            sql = " SELECT URETIMNO,TARIH,SUM(URETIM_MIKTAR) URETIM_MIKTAR,SUM(SARF_MIKTAR) SARF_MIKTAR " +
                  " FROM[ADV_" + FIRMANO + "_BORNURETIM] " +
                  " WHERE STATUS<>1 " +
                  " AND STOK_TURU = 'HM' " +
                  " GROUP BY URETIMNO,TARIH " +
                  " ORDER BY TARIH, URETIMNO ASC";
            DataTable dt = SqlHelper.TabloGetir(sql, ATLASCON);
            return dt;
        }

        public DataTable GetUretimSarfKayitlari(String uretimNo,String tarih)
        {
            DateTime dtTarih = DateTime.Parse(tarih);
            String newTarih = dtTarih.Year.ToString() + "-" + dtTarih.Month.ToString().PadLeft(2, '0') + "-" + dtTarih.Day.ToString().PadLeft(2, '0');

            var sql = "SELECT * FROM ADV_" + FIRMANO + "_BORNURETIM WHERE STATUS<>1 ORDER BY TARIH ASC";
            sql = " SELECT URETIMNO,STOCKREF,TARIH,STOK_KODU,STOK_ADI,GRUP_KODU,BIRIM,SUM(URETIM_MIKTAR) URETIM_MIKTAR,SUM(SARF_MIKTAR) SARF_MIKTAR " +
                  " FROM[ADV_" + FIRMANO + "_BORNURETIM] " +
                  " WHERE STATUS<>1 " +
                  " AND STOK_TURU = 'HM' " +
                  " AND URETIMNO='"+uretimNo+"' " +
                  " AND TARIH=  '"+newTarih+"' " +
                  " GROUP BY URETIMNO,STOCKREF,TARIH,STOK_KODU,STOK_ADI,GRUP_KODU,BIRIM " +
                  " ORDER BY TARIH, URETIMNO ASC";
            DataTable dt = SqlHelper.TabloGetir(sql, ATLASCON);
            return dt;
        }

        public DataTable GetUretimSatiriFromAnahtar(string IDREF)
        {
            var sql = "SELECT * FROM ADV_" + FIRMANO + "_BORNURETIM WHERE STATUS<>1 AND IDREF='" + IDREF + "' ORDER BY TARIH ASC";
            DataTable dt = SqlHelper.TabloGetir(sql, ATLASCON);
            return dt;
        }


        public MESAJmodel UretimFisiAktar(DataRow Mainrow)
        {
            var isyeriAdi = "Merkez";
            int isyeri = 0;
            int depo = 0;

            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();
            
            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(Mainrow["TARIH"] + "");
            model.PROCREF = Mainrow["URETIMNO"] + model.DATE_.Year.ToString().Substring(0,2)+""+model.DATE_.Month.ToString().PadLeft(2,'0')+""+ model.DATE_.Day.ToString().PadLeft(2, '0');
            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";
            model.DOC_NUMBER = model.PROCREF + ".BRN";
            var grupKodu = Mainrow["GRUP_KODU"] + "";


            model.ALTKALEMLER = new List<MALZEMEFISI_SATIRRmodel>();


            var subitemmodel = new MALZEMEFISI_SATIRRmodel();
            subitemmodel.ITEM_CODE = Mainrow["STOK_KODU"] + "";
            subitemmodel.QUANTITY = Mainrow["URETIM_MIKTAR"] + "";
            subitemmodel.UNIT_CODE = Mainrow["BIRIM"] + "";
            subitemmodel.SPECODE = Mainrow["URETIMNO"] + "";
            subitemmodel.SPECODE2 = model.PROCREF + "";

            model.ALTKALEMLER.Add(subitemmodel);


            mesaj = MalzemeFisiHelper.UretimFisiOlustur(LoginModel, model, LOGOCON);

            return mesaj;
        }

        public MESAJmodel SarfFisiAktar(DataRow Mainrow, DataRow SubRow)
        {
            var isyeriAdi = "Merkez";
            int isyeri = 0;
            int depo = 0;
            //if (isyeriAdidepo = 1;
            //}

            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();

            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(Mainrow["TARIH"] + "");
            model.PROCREF = Mainrow["IDREF"] + "";
            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";
            model.DOC_NUMBER = Mainrow["IDREF"] + ".BRN";
            var grupKodu = Mainrow["GRUP_KODU"] + "";


            model.ALTKALEMLER = new List<MALZEMEFISI_SATIRRmodel>();


            var subitemmodel = new MALZEMEFISI_SATIRRmodel();
            subitemmodel.ITEM_CODE = SubRow["STOK_KODU"] + "";
            subitemmodel.QUANTITY = SubRow["SARF_MIKTAR"] + "";
            subitemmodel.UNIT_CODE = SubRow["BIRIM"] + "";
            subitemmodel.SPECODE = SubRow["BORNREF"] + "";
            subitemmodel.SPECODE2 = SubRow["PARTINO"] + "";

            model.ALTKALEMLER.Add(subitemmodel);

            mesaj = MalzemeFisiHelper.SarfFisiOlustur(LoginModel, model, LOGOCON);

            return mesaj;
        }

        public MESAJmodel SarfFisiAktar(DataTable dtSarf)
        {
            var isyeriAdi = "Merkez";
            int isyeri = 0;
            int depo = 0;

            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();

            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(dtSarf.Rows[0]["TARIH"] + "");
            model.PROCREF = dtSarf.Rows[0]["URETIMNO"] + model.DATE_.Year.ToString().Substring(0, 2) + "" + model.DATE_.Month.ToString().PadLeft(2, '0') + "" + model.DATE_.Day.ToString().PadLeft(2, '0');
            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";
            model.DOC_NUMBER = model.PROCREF + ".BRN";
            var grupKodu = dtSarf.Rows[0]["GRUP_KODU"] + "";


            model.ALTKALEMLER = new List<MALZEMEFISI_SATIRRmodel>();

            for (int i = 0; i < dtSarf.Rows.Count; i++)
            {
                var subitemmodel = new MALZEMEFISI_SATIRRmodel();
                subitemmodel.ITEM_CODE = dtSarf.Rows[i]["STOK_KODU"] + "";
                subitemmodel.QUANTITY = dtSarf.Rows[i]["SARF_MIKTAR"] + "";
                subitemmodel.UNIT_CODE = dtSarf.Rows[i]["BIRIM"] + "";
                subitemmodel.SPECODE = dtSarf.Rows[i]["URETIMNO"] + "";
                subitemmodel.SPECODE2 = model.PROCREF + "";

                model.ALTKALEMLER.Add(subitemmodel);

            }
            

            mesaj = MalzemeFisiHelper.SarfFisiOlustur(LoginModel, model, LOGOCON);

            return mesaj;
        }
        #endregion

        #region Sanal Uretim

        public void AdmSanalUretimFisAktar()
        {
            var aktarilmamisKayitlar = GetSanalUretimListesiFromAnahtar();
            foreach (DataRow row in aktarilmamisKayitlar.Rows)
            {

                string URETIMNO = row["URETIMNO"] + "";
                string PARTINO = row["PARTINO"] + "";
                string IDREF = row["IDREF"] + "";

                //cuval sarf olustur
                //malzeme sarf olustur
                //malzeme uretim fisi olustur

                var result = SanalSarfCuvalFisiAktar(row);
                result = SanalSarfMalzemeFisiAktar(row);
                result = SanalUretimFisiAktar(row);


                if (result.HasError)
                    UretimLogla(URETIMNO, PARTINO, 0, "", "", IDREF);
                else
                    UretimLogla(URETIMNO, PARTINO, 1, result.Message, result.Data + "", IDREF);

            }
        }

        public DataTable GetSanalUretimListesiFromAnahtar()
        {
            var sql = "SELECT * FROM ADV_" + FIRMANO + "_BORNURETIM_SANAL WHERE STATUS<>1 ORDER BY TARIH ASC";
            DataTable dt = SqlHelper.TabloGetir(sql, ATLASCON);
            return dt;
        }


        public MESAJmodel SanalUretimFisiAktar(DataRow Mainrow)
        {
            var isyeriAdi = "Merkez";
            int isyeri = 0;
            int depo = 0;
            if (isyeriAdi == "Merkez")
            {
                isyeri = 5;
                depo = 10;
            }
            else if (isyeriAdi == "Söğüt")
            {
                isyeri = 1;
                depo = 1;
            }

            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();

            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(Mainrow["TARIH"] + "");
            model.PROCREF = Mainrow["IDREF"] + "";
            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";
            model.DOC_NUMBER = Mainrow["IDREF"] + "SNL";
            var grupKodu = Mainrow["GRUP_KODU"] + "";


            model.ALTKALEMLER = new List<MALZEMEFISI_SATIRRmodel>();


            var subitemmodel = new MALZEMEFISI_SATIRRmodel();
            subitemmodel.ITEM_CODE = Mainrow["PAKET_URUN"] + "";
            subitemmodel.QUANTITY = Mainrow["SARF_MIKTAR"] + "";
            subitemmodel.UNIT_CODE = Mainrow["BIRIM2"] + "";
            subitemmodel.SPECODE = Mainrow["BORNREF"] + "";
            subitemmodel.SPECODE2 = Mainrow["PARTINO"] + "";

            model.ALTKALEMLER.Add(subitemmodel);


            mesaj = MalzemeFisiHelper.UretimFisiOlustur(LoginModel, model, LOGOCON);

            return mesaj;
        }

        public MESAJmodel SanalSarfCuvalFisiAktar(DataRow Mainrow)
        {
            var isyeriAdi = "Merkez";
            int isyeri = 0;
            int depo = 0;
            if (isyeriAdi == "Merkez")
            {
                isyeri = 5;
                depo = 10;
            }
            else if (isyeriAdi == "Söğüt")
            {
                isyeri = 1;
                depo = 1;
            }

            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();

            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(Mainrow["TARIH"] + "");
            model.PROCREF = Mainrow["IDREF"] + "";
            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";
            model.DOC_NUMBER = Mainrow["IDREF"] + "SNL";
            var grupKodu = Mainrow["GRUP_KODU"] + "";


            model.ALTKALEMLER = new List<MALZEMEFISI_SATIRRmodel>();


            var subitemmodel = new MALZEMEFISI_SATIRRmodel();
            subitemmodel.ITEM_CODE = Mainrow["STOK_KODU"] + "";
            subitemmodel.QUANTITY = Mainrow["SARF_MIKTAR"] + "";
            subitemmodel.UNIT_CODE = Mainrow["BIRIM"] + "";
            subitemmodel.SPECODE = Mainrow["BORNREF"] + "";
            subitemmodel.SPECODE2 = Mainrow["PARTINO"] + "";

            model.ALTKALEMLER.Add(subitemmodel);

            mesaj = MalzemeFisiHelper.SarfFisiOlustur(LoginModel, model, LOGOCON);

            return mesaj;
        }


        public MESAJmodel SanalSarfMalzemeFisiAktar(DataRow Mainrow)
        {
            var isyeriAdi = "Merkez";
            int isyeri = 0;
            int depo = 0;
            if (isyeriAdi == "Merkez")
            {
                isyeri = 5;
                depo = 10;
            }
            else if (isyeriAdi == "Söğüt")
            {
                isyeri = 1;
                depo = 1;
            }

            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();

            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(Mainrow["TARIH"] + "");
            model.PROCREF = Mainrow["IDREF"] + "";
            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";
            model.DOC_NUMBER = Mainrow["IDREF"] + "SNL";
            var grupKodu = Mainrow["GRUP_KODU"] + "";


            model.ALTKALEMLER = new List<MALZEMEFISI_SATIRRmodel>();


            var subitemmodel = new MALZEMEFISI_SATIRRmodel();
            subitemmodel.ITEM_CODE = Mainrow["PAKET_URUN"] + "";
            subitemmodel.QUANTITY = Mainrow["URETIM_MIKTAR2"] + "";
            subitemmodel.UNIT_CODE = Mainrow["BIRIM2"] + "";
            subitemmodel.SPECODE = Mainrow["BORNREF"] + "";
            subitemmodel.SPECODE2 = Mainrow["PARTINO"] + "";

            model.ALTKALEMLER.Add(subitemmodel);

            mesaj = MalzemeFisiHelper.SarfFisiOlustur(LoginModel, model, LOGOCON);

            return mesaj;
        }

        #endregion

        #region Hizli Uretim


        //public MESAJmodel HizliUretimLogoyaAktar(DataRow Mainrow, DataTable SubRow)
        //{
        //    var isyeriAdi = "Merkez";
        //    //int isyeri = 0;
        //    int depo = 0;
        //    if (isyeriAdi == "Merkez")
        //    {
        //        //isyeri = 5;
        //        depo = 10;
        //    }
        //    else if (isyeriAdi == "Söğüt")
        //    {
        //        //isyeri = 1;
        //        depo = 1;
        //    }

        //    MESAJmodel mesaj = new MESAJmodel();

        //    var hizliUretimModel = new HIZLIURETIMmodel();

        //    //todo hizli uretim doldur 
        //    hizliUretimModel.DATE_ = DateTime.Parse(GetUretimTarih(Mainrow["URETIMNO"] + "").Rows[0]["TARIH"] + "");
        //    hizliUretimModel.ITEMREF = int.Parse(Mainrow["STOCKREF"] + "");
        //    hizliUretimModel.MIKTAR = decimal.Parse(Mainrow["URETIM_MIKTAR"] + "");
        //    hizliUretimModel.AMBAR = depo;
        //    //hizliUretimModel.BOLUM = isyeri;
        //    hizliUretimModel.URETIMNO = Mainrow["URETIMNO"] + "";
        //    //hizliUretimModel.LREF = "";
        //    //hizliUretimModel.PROJECTREF = "";
        //    //hizliUretimModel.TYPE = "";
        //    //hizliUretimModel.LOTNO = Mainrow["URETIMNO"] + "-" + Mainrow["PARTINO"];
        //    hizliUretimModel.LOTNO = Mainrow["URETIMNO"] + "";
        //    var grupKodu = Mainrow["GRUP_KODU"] + "";


        //    hizliUretimModel.ALTKALEMLER = new List<HIZLIURETIMALTKALEMLERmmodel>();

        //    foreach (DataRow item in SubRow.Rows)
        //    {
        //        var subitemmodel = new HIZLIURETIMALTKALEMLERmmodel();
        //        subitemmodel.SUBCODE = item["STOK_KODU"] + "";
        //        subitemmodel.LOSTFACTOR = 0;
        //        subitemmodel.AMOUNT = decimal.Parse(item["SARF_MIKTAR"] + "");
        //        subitemmodel.SUBUNIT = item["BIRIM"] + "";
        //        subitemmodel.DEPARTMENT = hizliUretimModel.BOLUM;
        //        subitemmodel.SOURCEINDEX = hizliUretimModel.AMBAR;
        //        subitemmodel.MAINCREF = hizliUretimModel.ITEMREF;

        //        hizliUretimModel.ALTKALEMLER.Add(subitemmodel);
        //    }

        //    mesaj = HizliUretimHelper.HizliUretimFisiOlustur(LoginModel, hizliUretimModel, LOGOCON, FIRMANO);

        //    return mesaj;
        //}
        #endregion



        public void UretimLogla(string URETIMNO, string PARTINO, int STATUS, string TRANSFERMESSAGE, string LOGO_PROCREF, string BORN_IDREF)
        {
            try
            {
                var sql = "INSERT INTO ADM_BORNLOGS ( STATUS, BORN_URETIMNO, BORN_PARTINO, CREATEDATE, MESSAGE, LOGO_PROCREF, BORN_IDREF ) VALUES ('" + STATUS + "','" + URETIMNO + "','" + PARTINO + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + TRANSFERMESSAGE + "','" + LOGO_PROCREF + "','" + BORN_IDREF + "')";
                var result = SqlHelper.VerileriKaydet(sql, ATLASCON);
            }
            catch (Exception ex)
            {
                SqlHelper.Logger(ex.Message);
            }

        }

        private void btnManuelSenkronizayon_Click(object sender, EventArgs e)
        {

        }
    }
}
