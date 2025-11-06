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

        public string FIRMA_ADI = ConfigurationManager.AppSettings["FIRMA_ADI"].ToString();
        public string LOGOUSER = ConfigurationManager.AppSettings["LOGOUSER"].ToString();
        public string LOGOPASS = ConfigurationManager.AppSettings["LOGOPASS"].ToString();
        public string FIRMANO = ConfigurationManager.AppSettings["FIRMANO"].ToString();
        public string FIRMADONEM = ConfigurationManager.AppSettings["FIRMADONEM"].ToString();

        public string AMBAR = ConfigurationManager.AppSettings["AMBAR"].ToString();
        public string ISYERI = ConfigurationManager.AppSettings["ISYERI"].ToString();


        public LOGOLOGINModel LoginModel;
        public FrmMain()
        {
            InitializeComponent();
        }
        private void FrmMain_Load(object sender, EventArgs e)
        {
            LoginModel = new LOGOLOGINModel(LOGOUSER, LOGOPASS, FIRMANO);

            if (FIRMA_ADI == "IPEKYEM")
            {
                timer1.Interval = 60000; // 1 dakika
            }
            else if (FIRMA_ADI == "NUTRILINE")
            {
                timer1.Interval = 600000;
            }

            timer1.Start();
        }
        private DateTime lastRunDate = DateTime.MinValue;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (FIRMA_ADI == "IPEKYEM")
            {
                // Saat 9 ve sonrası + bugün henüz çalışmadıysa
                if (DateTime.Now.Hour >= 9 && lastRunDate.Date != DateTime.Now.Date)
                {
                    AdmUretimFisAktar();
                    lastRunDate = DateTime.Now;
                }
                return;
            }

            // Diğer firmalar normal...
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
            timer1.Start();
            Cursor.Current = Cursors.Arrow;
        }



        #region Uretim
        public void AdmUretimFisAktar()
        {
            UretimLogla("", "", 0, "Sync Çalışmaya Başladı", "", "");
            //üRETİM GİRİŞLERİNİ AKTAR
            var aktarilmamisKayitlar = GetUretimListesiFromAnahtar();
            if (aktarilmamisKayitlar == null) return;

            if (aktarilmamisKayitlar.Rows.Count > 0)
            {
                UretimLogla("", "", 0, "Aktarılacak " + aktarilmamisKayitlar.Rows.Count.ToString() + " Adet üretim kaydı bulundu.", "", "");
                foreach (DataRow row in aktarilmamisKayitlar.Rows)
                {
                    string URETIMNO = row["URETIMNO"] + "";
                    string TARIH = row["TARIH"] + "";
                    string STOK_TURU = row["STOK_TURU"] + "";

                    string PARTINO = URETIMNO; // row["PARTINO"] + "";
                    string IDREF = URETIMNO; // row["IDREF"] + "";

                    DateTime dtTarih = DateTime.Parse(TARIH);
                    String newTarih = dtTarih.Year.ToString() + "-" + dtTarih.Month.ToString().PadLeft(2, '0') + "-" + dtTarih.Day.ToString().PadLeft(2, '0');
                    UretimLogla(URETIMNO, TARIH, 0, "Üretim fişi için aktarılacak kayıt aranmaya başladı...", "", STOK_TURU);

                    #region GÜN GÜN TEK BİR ÜRETİM FİŞİ İPEK YEM İÇİN

                    var uretimDatatable = GetUretimSarfKayitlari(URETIMNO, TARIH, STOK_TURU);
                    MESAJmodel result = new MESAJmodel();

                    if (FIRMA_ADI == "IPEK")
                    {
                        if (uretimDatatable.Rows.Count > 0)
                        {
                            UretimLogla(URETIMNO, TARIH, 0, "Fiş için aktarılacak kayıtlar logoya aktarılmaya çalışılıyor...", "", STOK_TURU);
                            result = UretimFisiAktar(uretimDatatable);
                            if (result.HasError)
                                UretimLogla(URETIMNO, newTarih, 0, result.Message, "", "MM");
                            else
                                UretimLogla(URETIMNO, newTarih, 1, result.Message, result.Data + "", "MM");
                        }
                    }
                    #endregion

                    else
                    {
                        result = UretimFisiAktar(row);
                        if (result.HasError)
                            UretimLogla(URETIMNO, newTarih, 0, result.Message, "", "MM");
                        else
                            UretimLogla(URETIMNO, newTarih, 1, result.Message, result.Data + "", "MM");

                    }


                }
            }




            //SARF ÇIKIŞLARINI AKTARRRRR
            var aktarilmamisSarfKayitlar = GetSarfListesiFromAnahtar();
            if (aktarilmamisSarfKayitlar == null)
            {
                return;
            }
            if (aktarilmamisSarfKayitlar.Rows.Count > 0)
            {
                UretimLogla("", "", 0, "Aktarılacak " + aktarilmamisSarfKayitlar.Rows.Count.ToString() + " Adet sarf kaydı bulundu.", "", "");
                foreach (DataRow row in aktarilmamisSarfKayitlar.Rows)
                {
                    //yeniden kontrol et var mi

                    string URETIMNO = row["URETIMNO"] + "";
                    string TARIH = row["TARIH"] + "";
                    string STOK_TURU = row["STOK_TURU"] + "";
                    string SIPARIS_TIPI = row["SIPARIS_TIPI"] + "";

                    DateTime dtTarih = DateTime.Parse(TARIH);
                    String newTarih = dtTarih.Year.ToString() + "-" + dtTarih.Month.ToString().PadLeft(2, '0') + "-" + dtTarih.Day.ToString().PadLeft(2, '0');
                    UretimLogla(URETIMNO, TARIH, 0, "Sarf fişi için aktarılacak kayıt aranmaya başladı...", "", STOK_TURU);

                    var sarfDataTable = GetUretimSarfKayitlari(URETIMNO, TARIH, STOK_TURU);

                    if (sarfDataTable.Rows.Count > 0)
                    {
                        UretimLogla(URETIMNO, TARIH, 0, "Fiş için aktarılacak kayıtlar logoya aktarılmaya çalışılıyor...", "", STOK_TURU);


                        MESAJmodel result = new MESAJmodel();

                        result = SarfFisiAktar(sarfDataTable);


                        if (result.HasError)
                            UretimLogla(URETIMNO, newTarih, 0, result.Message, "", "HM");
                        else
                            UretimLogla(URETIMNO, newTarih, 1, result.Message, result.Data + "", "HM");
                    }


                }
            }
        }

        #region TABLONUN DOLDUĞU SORGULAR
        public DataTable GetUretimListesiFromAnahtar()
        {
            var sql = "";


            if (FIRMA_ADI == "IPEK")
            {
                sql = " SELECT URETIMNO,TARIH,STOK_TURU FROM [ADV_" + FIRMANO + "_BORNURETIM] WHERE STOK_TURU = 'MM' GROUP BY URETIMNO,TARIH,STOK_TURU";
            }
            else
            {
                sql = " SELECT URETIMNO,STOCKREF,TARIH,STOK_KODU,STOK_ADI,STOK_TURU,GRUP_KODU,BIRIM,SUM(URETIM_MIKTAR) URETIM_MIKTAR,SUM(SARF_MIKTAR) SARF_MIKTAR " +
                          " FROM[ADV_" + FIRMANO + "_BORNURETIM] " +
                          " WHERE STATUS<>1 " +
                          " AND STOK_TURU = 'MM' " +
                          " GROUP BY URETIMNO,STOCKREF,TARIH,STOK_KODU,STOK_ADI,GRUP_KODU,BIRIM,STOK_TURU " +
                          " ORDER BY TARIH, URETIMNO ASC";
            }


            DataTable dt = SqlHelper.TabloGetir(sql, ATLASCON);
            return dt;
        }

        public DataTable GetSarfListesiFromAnahtar()
        {
            var sql = "";

            if (FIRMA_ADI == "NUTRILINE"
                || FIRMA_ADI == "KAYSERIYEM")
            {
                sql = " SELECT URETIMNO,TARIH,SUM(URETIM_MIKTAR) URETIM_MIKTAR,SUM(SARF_MIKTAR) SARF_MIKTAR,STOK_TURU " +
                         " FROM[ADV_" + FIRMANO + "_BORNURETIM] " +
                         " WHERE STATUS<>1 " +
                         " AND STOK_TURU = 'HM' " +
                         " GROUP BY URETIMNO,TARIH,STOK_TURU " +
                         " ORDER BY TARIH, URETIMNO ASC";
            }
            else if (FIRMA_ADI == "IPEK")
            {
                sql = " SELECT DISTINCT URETIMNO,TARIH,STOK_TURU,SIPARIS_TIPI FROM [ADV_" + FIRMANO + "_BORNURETIM] WHERE STOK_TURU = 'HM' GROUP BY URETIMNO,TARIH,STOK_TURU,SIPARIS_TIPI ORDER BY URETIMNO,TARIH,STOK_TURU";
            }
            DataTable dt = SqlHelper.TabloGetir(sql, ATLASCON);
            return dt;
        }
        #endregion

        public DataTable GetUretimSarfKayitlari(String uretimNo, String tarih, string stokTuru)
        {
            DateTime dtTarih = DateTime.Parse(tarih);
            String newTarih = dtTarih.Year.ToString() + "-" + dtTarih.Month.ToString().PadLeft(2, '0') + "-" + dtTarih.Day.ToString().PadLeft(2, '0');

            var sql = "";

            if (FIRMA_ADI == "NUTRILINE"
                || FIRMA_ADI == "KAYSERIYEM")
            {
                sql = " SELECT URETIMNO,STOCKREF,TARIH,STOK_KODU,STOK_ADI,GRUP_KODU,BIRIM,SUM(URETIM_MIKTAR) URETIM_MIKTAR,SUM(SARF_MIKTAR) SARF_MIKTAR " +
                       " FROM[ADV_" + FIRMANO + "_BORNURETIM] " +
                       " WHERE STATUS<>1 " +
                       " AND STOK_TURU = '" + stokTuru + "' " +
                       " AND URETIMNO='" + uretimNo + "' " +
                       " AND TARIH=  '" + newTarih + "' " +
                       " GROUP BY URETIMNO,STOCKREF,TARIH,STOK_KODU,STOK_ADI,GRUP_KODU,BIRIM,SIPARIS_TIPI " +
                       " ORDER BY TARIH, URETIMNO ASC";
            }
            else if (FIRMA_ADI == "IPEK")
            {
                sql = "SELECT URETIMNO,TARIH,STOK_KODU,BIRIM,URETIM_MIKTAR,SARF_MIKTAR,SIPARIS_TIPI " +
                    " FROM [ADV_" + FIRMANO + "_BORNURETIM]" +
                    " WHERE STOK_TURU = '" + stokTuru + "'" +
                    " AND URETIMNO='" + uretimNo + "' " +
                    " AND TARIH=  '" + newTarih + "' ";
            }
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

            int isyeri = Convert.ToInt32(ISYERI);
            int depo = Convert.ToInt32(AMBAR);

            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();

            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(Mainrow["TARIH"] + "");

            if (FIRMA_ADI != "IPEK")
            {
                model.PROCREF = Mainrow["URETIMNO"] + model.DATE_.Year.ToString().Substring(0, 2) + "" + model.DATE_.Month.ToString().PadLeft(2, '0') + "" + model.DATE_.Day.ToString().PadLeft(2, '0');
            }
            else
            {
                model.PROCREF = Mainrow["URETIMNO"] + "";
            }

            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";
            if (FIRMA_ADI != "IPEK")
            {
                model.DOC_NUMBER = model.PROCREF + ".BRN";
                var grupKodu = Mainrow["GRUP_KODU"] + "";
            }
            else
            {
                model.DOC_NUMBER = model.PROCREF;
            }


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

        public MESAJmodel UretimFisiAktar(DataTable Mainrow)
        {
            var isyeriAdi = "Merkez";

            int isyeri = Convert.ToInt32(ISYERI);
            int depo = Convert.ToInt32(AMBAR);

            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();

            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(Mainrow.Rows[0]["TARIH"] + "");

            if (FIRMA_ADI != "IPEK")
            {
                model.PROCREF = Mainrow.Rows[0]["URETIMNO"] + model.DATE_.Year.ToString().Substring(0, 2) + "" + model.DATE_.Month.ToString().PadLeft(2, '0') + "" + model.DATE_.Day.ToString().PadLeft(2, '0');
            }
            else
            {
                model.PROCREF = Mainrow.Rows[0]["URETIMNO"] + "";
            }

            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";
            if (FIRMA_ADI != "IPEK")
            {
                model.DOC_NUMBER = model.PROCREF + ".BRN";
                var grupKodu = Mainrow.Rows[0]["GRUP_KODU"] + "";
            }
            else
            {
                model.DOC_NUMBER = model.PROCREF;
            }


            model.ALTKALEMLER = new List<MALZEMEFISI_SATIRRmodel>();
            foreach (DataRow item in Mainrow.Rows)
            {
                var subitemmodel = new MALZEMEFISI_SATIRRmodel();
                subitemmodel.ITEM_CODE = item["STOK_KODU"] + "";
                subitemmodel.QUANTITY = item["URETIM_MIKTAR"] + "";
                subitemmodel.UNIT_CODE = item["BIRIM"] + "";
                subitemmodel.SPECODE = item["URETIMNO"] + "";
                subitemmodel.SPECODE2 = model.PROCREF + "";

                model.ALTKALEMLER.Add(subitemmodel);
            }



            mesaj = MalzemeFisiHelper.UretimFisiOlustur(LoginModel, model, LOGOCON);

            return mesaj;
        }

        public MESAJmodel SarfFisiAktar(DataRow Mainrow, DataRow SubRow)
        {
            var isyeriAdi = "Merkez";
            int isyeri = Convert.ToInt32(ISYERI);
            int depo = Convert.ToInt32(AMBAR);

            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();


            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(Mainrow["TARIH"] + "");
            if (FIRMA_ADI != "IPEK")
            {
                model.PROCREF = Mainrow["IDREF"] + "";
                model.DOC_NUMBER = Mainrow["IDREF"] + ".BRN";
                var grupKodu = Mainrow["GRUP_KODU"] + "";
            }
            else
            {
                model.PROCREF = Mainrow["URETIMNO"] + "";
                model.DOC_NUMBER = Mainrow["URETIMNO"] + "";
            }
            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";


            model.ALTKALEMLER = new List<MALZEMEFISI_SATIRRmodel>();


            var subitemmodel = new MALZEMEFISI_SATIRRmodel();
            subitemmodel.ITEM_CODE = SubRow["STOK_KODU"] + "";
            subitemmodel.QUANTITY = SubRow["SARF_MIKTAR"] + "";
            subitemmodel.UNIT_CODE = SubRow["BIRIM"] + "";

            if (FIRMA_ADI != "IPEK")
            {
                subitemmodel.SPECODE = SubRow["BORNREF"] + "";
                subitemmodel.SPECODE2 = SubRow["PARTINO"] + "";
            }
            else
            {
                subitemmodel.SPECODE = SubRow["URETIMNO"] + "";
                subitemmodel.SPECODE2 = SubRow["URETIMNO"] + "";
            }

            model.ALTKALEMLER.Add(subitemmodel);

            mesaj = MalzemeFisiHelper.SarfFisiOlustur(LoginModel, model, LOGOCON);

            return mesaj;
        }

        public MESAJmodel SarfFisiAktar(DataTable dtSarf)
        {
            var isyeriAdi = "Merkez";
            int isyeri = Convert.ToInt32(ISYERI);
            int depo = Convert.ToInt32(AMBAR);


            MESAJmodel mesaj = new MESAJmodel();

            var model = new MALZEMEFISImodel();

            //todo hizli uretim doldur 
            model.DATE_ = DateTime.Parse(dtSarf.Rows[0]["TARIH"] + "");

            if (FIRMA_ADI != "IPEK")
            {
                model.PROCREF = dtSarf.Rows[0]["URETIMNO"] + model.DATE_.Year.ToString().Substring(0, 2) + "" + model.DATE_.Month.ToString().PadLeft(2, '0') + "" + model.DATE_.Day.ToString().PadLeft(2, '0');
                model.DOC_NUMBER = model.PROCREF + ".BRN";
                var grupKodu = dtSarf.Rows[0]["GRUP_KODU"] + "";

            }
            else
            {
                model.PROCREF = dtSarf.Rows[0]["URETIMNO"] + "";
                model.DOC_NUMBER = model.PROCREF;
                model.OZELKOD = dtSarf.Rows[0]["SIPARIS_TIPI"] + "";

            }

            model.WAREHOUSE = depo + "";
            model.BRANCH = isyeri + "";


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
