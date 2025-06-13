using BornSYNC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BornSYNC.Helper
{
    public class HizliUretimHelper
    {
        public static MESAJmodel HizliUretimFisiOlustur(LOGOLOGINModel loginModel, HIZLIURETIMmodel hizliUretimModel, string Con, string FirmaNo)
        {
            MESAJmodel mesaj = new MESAJmodel();

            if (!LogoHelper.BaglantiDurum())
            {
                LogoHelper.BaglantiYap(loginModel);
            }
            try
            {
                if (MamulAltKalemleriniSil(hizliUretimModel.ITEMREF) != true)
                {
                    mesaj.HasError = true;
                    mesaj.ErrorCode = 1;
                    mesaj.Message = "Mamül Alt Kalemleri Silinemedi.";
                    SqlHelper.Logger(mesaj.Message);
                }
                else if (UretimSatirlariniGuncelle(hizliUretimModel.ITEMREF, hizliUretimModel) != true)
                {
                    mesaj.HasError = true;
                    mesaj.ErrorCode = 1;
                    mesaj.Message = "Mamül Alt Kalemleri Eklenemedi.";
                    SqlHelper.Logger(mesaj.Message);
                }
                else
                {



                    var Tmp = LogoCon.Get().UnityApp.NewProductionApplication();
                    UnityObjects.QuickProdSlipRefLists RefList = Tmp.NewQPSlipRefLists();
                    object dval = 0;
                    object tval = 0;
                    LogoCon.Get().UnityApp.PackDate(hizliUretimModel.DATE_.Day, hizliUretimModel.DATE_.Month, hizliUretimModel.DATE_.Year, ref dval);
                    LogoCon.Get().UnityApp.PackTime(2, 2, 2, ref tval);
                    int dvalint = Convert.ToInt32(dval);
                    int tvalint = Convert.ToInt32(tval);

                    //bool RES = Tmp.QuickProdFicheProc(hizliUretimModel.ITEMREF, (double)hizliUretimModel.MIKTAR, RefList, dvalint, tvalint, hizliUretimModel.BOLUM, hizliUretimModel.AMBAR, calcOpt: 0);
                    bool RES = Tmp.QuickProdFicheProc(hizliUretimModel.ITEMREF, (double)hizliUretimModel.MIKTAR, RefList, dvalint, tvalint, 0, hizliUretimModel.AMBAR, calcOpt: 0);

                    if (!RES)
                    {
                        mesaj.HasError = true;
                        mesaj.ErrorCode = 1;
                        if (Tmp.GetLastError().ToString() == "10030")
                        {
                            mesaj.Message = "Stok Eksiye Düşmüş Yada Mamül Altkalemlerde Ambar Bilgisi Yanlış";
                            SqlHelper.Logger(mesaj.Message);
                            //SonucYaz("Stok Eksiye Düşmüş Yada Mamül Altkalemlerde Ambar Bilgisi Yanlış", "2", dtUretim.Rows[0]["LREF"].ToString());
                        }
                        else
                        {
                            mesaj.Message = Tmp.GetLastError() + " - " + Tmp.GetLastErrorString();
                            SqlHelper.Logger(mesaj.Message);
                            //SonucYaz(Tmp.GetLastError() + " - " + Tmp.GetLastErrorString(), "2", dtUretim.Rows[0]["LREF"].ToString());
                        }
                    }
                    else
                    {
                        mesaj.HasError = false;
                        mesaj.ErrorCode = 0;
                        mesaj.Message = "Üretim Başarıyla Yapılmıştır.";


                        if (RefList.QProdSlips.Count > 0) //hızlı üretim fişleri
                        {
                            for (int i = 0; i < RefList.QProdSlips.Count; i++)
                            {
                                // MessageBox.Show("QProdSlips" + RefList.QProdSlips.Item[i].lref);

                                var sql = "UPDATE LG_" + FirmaNo + "_01_QPRODUCT SET SPECODE='" + hizliUretimModel.URETIMNO + "' WHERE LOGICALREF='" + RefList.QProdSlips.Item[i].lref + "'";
                                var result = SqlHelper.VerileriKaydet(sql, Con);
                            }
                        }


                        //if (RefList.QProdSlips.Count > 0) //hızlı üretim fişleri
                        //{
                        //    for (int i = 0; i < RefList.QProdSlips.Count; i++)
                        //    {
                        //        //MessageBox.Show("QProdSlips" + RefList.QProdSlips.Item[i].lref);
                        //    }
                        //}
                        //if (RefList.InputfromProdSlips.Count > 0)// üretimden giriş fişleri
                        //{
                        //    for (int i = 0; i < RefList.InputfromProdSlips.Count - 1; i++)
                        //    {
                        //        // MessageBox.Show("InputfromProdSlips" + RefList.InputfromProdSlips.Item[i].lref);
                        //    }
                        //}
                        //if (RefList.ScarpSlips.Count > 0) //fire fişleri
                        //{
                        //    for (int i = 0; i < RefList.ScarpSlips.Count; i++)
                        //    {
                        //        /// MessageBox.Show("ScarpSlips" + RefList.ScarpSlips.Item[i].lref);
                        //    }
                        //}
                        //if (RefList.UsageSlips.Count > 0)//sarf fişleri
                        //{
                        //    for (int i = 0; i < RefList.UsageSlips.Count; i++)
                        //    {
                        //        // MessageBox.Show("UsageSlips" + RefList.UsageSlips.Item[i].lref);
                        //    }
                        //}
                        //if (RefList.WHTransSlips.Count > 0) // ambar transfer fişleri
                        //{
                        //    for (int i = 0; i < RefList.WHTransSlips.Count; i++)
                        //    {
                        //        /// MessageBox.Show("WHTransSlips" + RefList.WHTransSlips.Item[i].lref);
                        //    }
                        //}
                        //else
                        //{

                        //    mesaj.HasError = true;
                        //    mesaj.ErrorCode = 1;
                        //    mesaj.Message = "Üretim Fiş Şablonunda proplem olabilir...!";
                        //    AdmHelper.Logger(mesaj.Message);
                        //}
                        Tmp = null;
                    }
                }
            }
            catch (Exception ex)
            {
                mesaj.HasError = true;
                mesaj.ErrorCode = 1;
                mesaj.Message = "ÜRETİMDE HATA";
                SqlHelper.Logger(mesaj.Message);

            }
            return mesaj;


        }
        public static bool MamulAltKalemleriniSil(int itemref)
        {
            UnityObjects.Data item = LogoCon.Get().UnityApp.NewDataObject(UnityObjects.DataObjectType.doMaterial);

            if (item.Read(itemref))
            {
                UnityObjects.Lines qprods_lines = item.DataFields.FieldByName("QPRODS").Lines;
                int deger = qprods_lines.Count;
                for (int i = 0; i < deger; i++)
                {
                    qprods_lines.DeleteLine(0);

                }
                if (item.Post())
                {

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool UretimSatirlariniGuncelle(int itemref, HIZLIURETIMmodel model)
        {
            UnityObjects.Data item = LogoCon.Get().UnityApp.NewDataObject(UnityObjects.DataObjectType.doMaterial);

            UnityObjects.Lines qprods_lines2 = item.DataFields.FieldByName("QPRODS").Lines;
            if (item.Read(itemref))
            {
                foreach (var satir in model.ALTKALEMLER)
                {
                    qprods_lines2.AppendLine();
                    var satirAmount = (satir.AMOUNT / model.MIKTAR);
                    qprods_lines2[qprods_lines2.Count - 1].FieldByName("AMNT").Value = satirAmount.ToString().Replace(",", ".");
                    qprods_lines2[qprods_lines2.Count - 1].FieldByName("LOSTFACTOR").Value = (satir.LOSTFACTOR / satir.AMOUNT).ToString().Replace(",", ".");
                    qprods_lines2[qprods_lines2.Count - 1].FieldByName("SOURCEINDEX").Value = satir.SOURCEINDEX.ToString();
                    qprods_lines2[qprods_lines2.Count - 1].FieldByName("DEPARTMENT").Value = satir.DEPARTMENT.ToString();
                    qprods_lines2[qprods_lines2.Count - 1].FieldByName("SCODE").Value = satir.SUBCODE;
                    // qprods_lines2[qprods_lines2.Count - 1].FieldByName("SDEF").Value = "MamUl Deneme";
                    qprods_lines2[qprods_lines2.Count - 1].FieldByName("UEDIT").Value = satir.SUBUNIT;
                }

                //qprods_lines2.AppendLine();
                //qprods_lines2[qprods_lines2.Count - 1].FieldByName("AMNT").Value = 10;
                //qprods_lines2[qprods_lines2.Count - 1].FieldByName("LOSTFACTOR").Value = 2;
                //qprods_lines2[qprods_lines2.Count - 1].FieldByName("SOURCEINDEX").Value = 0;
                //qprods_lines2[qprods_lines2.Count - 1].FieldByName("DEPARTMENT").Value = 0;
                //qprods_lines2[qprods_lines2.Count - 1].FieldByName("SCODE").Value = "XXX_002";
                //qprods_lines2[qprods_lines2.Count - 1].FieldByName("SDEF").Value = "MamUl Deneme";
                //qprods_lines2[qprods_lines2.Count - 1].FieldByName("UEDIT").Value = "ADET";
                try
                {
                    string path = "C:/Adimyazilim/Logs";
                    SqlHelper.VerifyDir(path);
                    item.ExportToXML("", path + "/Hizliuretim_" + DateTime.Now.Ticks + ".xml");
                }
                catch (Exception)
                {
                    SqlHelper.Logger("Xml export error");
                }


                if (item.Post())
                {
                    //SqlHelper.Logger("Post1");
                    return true;
                }
                else
                {
                    //SqlHelper.Logger("Post1");
                    return false;

                }

            }
            else
            {
                //SqlHelper.Logger("Post3");
                return false;
            }
        }

    }
}
