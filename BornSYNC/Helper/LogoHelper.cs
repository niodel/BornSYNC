using BornSYNC.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BornSYNC.Helper
{
   public class LogoHelper
    {
        static string ConLogo;

        public static string BaglantiYap(LOGOLOGINModel loginModel)
        {
            return BaglantiYap(loginModel.LOGOUSER, loginModel.LOGOPASS, int.Parse(loginModel.LOGOFIRMA));
        }

        //UnityObjects.UnityApplication UnityApp;
        public static string BaglantiYap(string LOGOUSER, string LOGOPASS, int LOGOFIRMA)
        {
            try
            {
                if (!BaglantiDurum())
                {
                    var key = "MARE.DLL;L10005;FSLDKFMLSEEEKMFVCMCK";
                    if (!LogoCon.Get().UnityApp.LoginEx(LOGOUSER, LOGOPASS, LOGOFIRMA, key))
                    {
                        ConLogo = "Hata Kodu : " + LogoCon.Get().UnityApp.GetLastError() + "  Açıklama : " + LogoCon.Get().UnityApp.GetLastErrorString();
                    }
                    else
                    {
                        ConLogo = "Bağlantı Kuruldu";
                    }
                    return
                        ConLogo;
                }
                else
                {
                    ConLogo = "Bağlantı Zaten Kurulu";
                }

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            return ConLogo;
        }
        public static bool BaglantiDurum()
        {
            if (LogoCon.Get().UnityApp == null)
            {
                return false;
            }
            return LogoCon.Get().UnityApp.Connected;
        }
    }
}
