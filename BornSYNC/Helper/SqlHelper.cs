using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BornSYNC.Helper
{
    public class SqlHelper
    {

        public static DataTable TabloGetir(string comText, string ConStr)
        {
            DataTable dt = new DataTable();
            try
            {
                var conn = new SqlConnection(ConStr);
                SqlCommand cmnd = new SqlCommand();
                cmnd.CommandTimeout = 0;
                cmnd.Connection = conn;
                cmnd.CommandText = comText;
                conn.Open();

                var adp = new SqlDataAdapter(cmnd);
                dt = new DataTable();
                adp.Fill(dt);

                conn.Close();
            }
            catch (Exception ex) { }
            return dt;
        }


        public static bool VerileriKaydet(string ComText, string ConStr)
        {
            try
            {
                var conn = new SqlConnection(ConStr);
                conn.Open();
                var cmd = new SqlCommand(ComText, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception) { }
            return false;
        }



        public static void VerifyDir(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
            catch { }
        }

        public static void Logger(string Mesaj)
        {
            string path = "C:/Adimyazilim/Logs/";
            VerifyDir(path);
            string fileName = "BURN_SYNC_" + DateTime.Now.Ticks + "_Logs.txt";
            try
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(path + fileName, true);
                file.WriteLine(DateTime.Now.ToString() + ": " + Mesaj);
                file.Close();
            }
            catch (Exception) { }
        }

        public bool LogEkle(string ConStr, string CLCARDREF, string CARI_NO, string CARI_UNVANI, string ACIKLAMA, string BANKA, string ORDER_ID, string CC_NO, string CC_OWNER, string AMOUNT, string INSTALLMENT, string MD_STATUS, string MD_MESSAGE, string KUL_ID, string P_DATE)
        {
            try
            {
                SqlConnection conn = new SqlConnection(ConStr);
                conn.Open();
                string query = "INSERT INTO EY_LOG ([LOGICALREF],[CARI_NO],[CARI_UNVANI],[ACIKLAMA],[BANKA],[ORDER_ID],[CC_NO],[CC_OWNER],[AMOUNT],[INSTALLMENT],[MD_STATUS],[MD_MESSAGE],[KUL_ID],[P_DATE]) " +
                    " VALUES (" + CLCARDREF + ",'" + CARI_NO + "','" + CARI_UNVANI + "','" + ACIKLAMA + "','" + BANKA + "','" + ORDER_ID + "','" + CC_NO + "','" + CC_OWNER + "','" + AMOUNT.ToString().Replace(",", ".") + "','" + INSTALLMENT + "','" + MD_STATUS + "','" + MD_MESSAGE + "','" + KUL_ID + "',CONVERT(DATETIME,'" + P_DATE + "',104))";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                return true;
            }
            catch (Exception) { }
            return false;
        }

    }
}
