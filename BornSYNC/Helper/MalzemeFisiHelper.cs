using BornSYNC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace BornSYNC.Helper
{
    public class MalzemeFisiHelper
    {


       public static FrmMain frm = new FrmMain();
        public static MESAJmodel UretimFisiOlustur(LOGOLOGINModel loginModel, MALZEMEFISImodel model, string Con)
        {
            MESAJmodel mesaj = new MESAJmodel();

            if (!LogoHelper.BaglantiDurum())
            {
                LogoHelper.BaglantiYap(loginModel);
            }

            try
            {
                UnityObjects.Data items = LogoCon.Get().UnityApp.NewDataObject(UnityObjects.DataObjectType.doMaterialSlip);
                items.New();
                items.DataFields.FieldByName("TYPE").Value = 13;
                items.DataFields.FieldByName("NUMBER").Value = model.DOC_NUMBER;
                items.DataFields.FieldByName("DATE").Value = model.DATE_;
                items.DataFields.FieldByName("TIME").Value = LogoTime(2, 2, 2);
                items.DataFields.FieldByName("AUXIL_CODE").Value = model.OZELKOD;
                items.DataFields.FieldByName("AUTH_CODE").Value = model.YETKIKODU;
                items.DataFields.FieldByName("CURRSEL_TOTALS").Value = 1;
                items.DataFields.FieldByName("QPROD_TYPE").Value = 1;
                items.DataFields.FieldByName("DOC_NUMBER").Value = model.DOC_NUMBER;
                items.DataFields.FieldByName("SOURCE_WH").Value = model.WAREHOUSE;
                items.DataFields.FieldByName("SOURCE_DIVISION_NR").Value = model.BRANCH;


                foreach (var satir in model.ALTKALEMLER)
                {
                    UnityObjects.Lines transactions_lines = items.DataFields.FieldByName("TRANSACTIONS").Lines;

                    transactions_lines.AppendLine();
                    transactions_lines[transactions_lines.Count - 1].FieldByName("ITEM_CODE").Value = satir.ITEM_CODE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("AUXIL_CODE").Value = satir.SPECODE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("AUXIL_CODE2").Value = satir.SPECODE2;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("UNIT_CODE").Value = satir.UNIT_CODE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("SOURCEINDEX").Value = model.WAREHOUSE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("LINE_TYPE").Value = 0;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("QUANTITY").Value = satir.QUANTITY.Replace(",", ".");
                    transactions_lines[transactions_lines.Count - 1].FieldByName("EDT_CURR").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("DESCRIPTION").Value = satir.EXP;
                }

                if (items.Post() == true)
                {
                    mesaj.HasError = false;
                    mesaj.ErrorCode = 0;
                    mesaj.Message = "Üretim Fişi Eklenmiştir !";
                    mesaj.Data = items.DataFields.DBFieldByName("LOGICALREF").Value.ToString();

                    if (frm.FIRMA_ADI != "IPEK")
                    {
                        var sql = "UPDATE " + frm.LOGODB + ".dbo.LG_" + loginModel.LOGOFIRMA + "_01_STLINE SET QPRODFCTYP=3,QPRODFCREF='" + model.ALTKALEMLER.First().SPECODE + "', SPECODE='" + model.ALTKALEMLER.First().SPECODE2 + "' WHERE STFICHEREF=" + items.DataFields.DBFieldByName("LOGICALREF").Value.ToString();
                        var result = SqlHelper.VerileriKaydet(sql, Con);

                        var sql2 = "UPDATE " + frm.LOGODB + ".dbo.LG_" + loginModel.LOGOFIRMA + "_01_STFICHE SET QPRODFCTYP=3,QPRODFCREF='" + model.ALTKALEMLER.First().SPECODE + "' WHERE LOGICALREF=" + items.DataFields.DBFieldByName("LOGICALREF").Value.ToString();
                        var result2 = SqlHelper.VerileriKaydet(sql2, Con);
                    }

                }
                else
                {
                    mesaj.HasError = true;
                    mesaj.ErrorCode = items.ErrorCode;

                    if (items.ErrorCode != 0)
                    {
                        mesaj.Message = ("DBError(" + items.ErrorCode.ToString() + ")-" + items.ErrorDesc + items.DBErrorDesc);

                    }
                    else if (items.ValidateErrors.Count > 0)
                    {
                        string result = "XML ErrorList:";
                        for (int i = 0; i < items.ValidateErrors.Count; i++)
                        {
                            result += "(" + items.ValidateErrors[i].ID.ToString() + ") - " + items.ValidateErrors[i].Error;

                        }
                        mesaj.Message = result;
                    }

                }

            }
            catch (Exception ex)
            {
                mesaj.HasError = true;
                mesaj.ErrorCode = 1;
                mesaj.Message = "Üretim Fişi Oluştururken Hata Meydana Geldi." + ex.Message;
                SqlHelper.Logger(mesaj.Message);
            }
            return mesaj;


        }



        public static MESAJmodel SarfFisiOlustur(LOGOLOGINModel loginModel, MALZEMEFISImodel model, string Con)
        {
            MESAJmodel mesaj = new MESAJmodel();

            if (!LogoHelper.BaglantiDurum())
            {
                LogoHelper.BaglantiYap(loginModel);
            }

            try
            {
                UnityObjects.Data items = LogoCon.Get().UnityApp.NewDataObject(UnityObjects.DataObjectType.doMaterialSlip);
                items.New();
                items.DataFields.FieldByName("TYPE").Value = 12;
                items.DataFields.FieldByName("NUMBER").Value = model.DOC_NUMBER;
                items.DataFields.FieldByName("DATE").Value = model.DATE_;
                items.DataFields.FieldByName("TIME").Value = LogoTime(2, 2, 2);
                items.DataFields.FieldByName("AUXIL_CODE").Value = model.OZELKOD;
                items.DataFields.FieldByName("AUTH_CODE").Value = model.YETKIKODU;
                items.DataFields.FieldByName("CURRSEL_TOTALS").Value = 1;
                items.DataFields.FieldByName("QPROD_TYPE").Value = 1;
                items.DataFields.FieldByName("DOC_NUMBER").Value = model.DOC_NUMBER;
                items.DataFields.FieldByName("SOURCE_WH").Value = model.WAREHOUSE;
                items.DataFields.FieldByName("SOURCE_DIVISION_NR").Value = model.BRANCH;


                foreach (var satir in model.ALTKALEMLER)
                {
                    UnityObjects.Lines transactions_lines = items.DataFields.FieldByName("TRANSACTIONS").Lines;

                    transactions_lines.AppendLine();
                    transactions_lines[transactions_lines.Count - 1].FieldByName("ITEM_CODE").Value = satir.ITEM_CODE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("AUXIL_CODE").Value = satir.SPECODE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("AUXIL_CODE2").Value = satir.SPECODE2;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("UNIT_CODE").Value = satir.UNIT_CODE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("SOURCEINDEX").Value = model.WAREHOUSE;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("LINE_TYPE").Value = 0;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("QUANTITY").Value = satir.QUANTITY.Replace(",", ".");
                    transactions_lines[transactions_lines.Count - 1].FieldByName("EDT_CURR").Value = 1;
                    transactions_lines[transactions_lines.Count - 1].FieldByName("DESCRIPTION").Value = satir.EXP;
                }

                if (items.Post() == true)
                {
                    mesaj.HasError = false;
                    mesaj.ErrorCode = 0;
                    mesaj.Message = "Sarf Fişi Eklenmiştir !";
                    mesaj.Data = items.DataFields.DBFieldByName("LOGICALREF").Value.ToString();

                    if (frm.FIRMA_ADI != "IPEK")
                    {
                        var sql = "UPDATE " + frm.LOGODB + ".dbo.LG_" + loginModel.LOGOFIRMA + "_01_STLINE SET QPRODFCTYP=3,QPRODFCREF='" + model.ALTKALEMLER.First().SPECODE + "', SPECODE='" + model.ALTKALEMLER.First().SPECODE2 + "' WHERE STFICHEREF=" + items.DataFields.DBFieldByName("LOGICALREF").Value.ToString();
                        var result = SqlHelper.VerileriKaydet(sql, Con);

                        var sql2 = "UPDATE " + frm.LOGODB + ".dbo.LG_" + loginModel.LOGOFIRMA + "_01_STFICHE SET QPRODFCTYP=3,QPRODFCREF='" + model.ALTKALEMLER.First().SPECODE + "' WHERE LOGICALREF=" + items.DataFields.DBFieldByName("LOGICALREF").Value.ToString();
                        var result2 = SqlHelper.VerileriKaydet(sql2, Con);

                    }
                }
                else
                {
                    mesaj.HasError = true;
                    mesaj.ErrorCode = items.ErrorCode;

                    if (items.ErrorCode != 0)
                    {
                        mesaj.Message = ("DBError(" + items.ErrorCode.ToString() + ")-" + items.ErrorDesc + items.DBErrorDesc);

                    }
                    else if (items.ValidateErrors.Count > 0)
                    {
                        string result = "XML ErrorList:";
                        for (int i = 0; i < items.ValidateErrors.Count; i++)
                        {
                            result += "(" + items.ValidateErrors[i].ID.ToString() + ") - " + items.ValidateErrors[i].Error;

                        }
                        mesaj.Message = result;
                    }

                }

            }
            catch (Exception ex)
            {
                mesaj.HasError = true;
                mesaj.ErrorCode = 1;
                mesaj.Message = "Sarf Fişi Oluştururken Hata Meydana Geldi.";
                SqlHelper.Logger(mesaj.Message);
            }
            return mesaj;


        }


        public static int LogoTime(DateTime dt)
        {
            return (dt.Hour * 16777216) + (dt.Minute * 65536) + (dt.Second * 256);
        }

        public static int LogoTime(int Hour, int Minute, int Second)
        {
            return (Hour * 16777216) + (Minute * 65536) + (Second * 256);
        }
    }
}
