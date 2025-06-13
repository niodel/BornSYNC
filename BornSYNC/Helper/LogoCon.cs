using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BornSYNC.Helper
{
    public class LogoCon
    {
        public static LogoCon _instance = null;
        private static readonly object syncLock = new object();
        public UnityObjects.UnityApplication UnityApp = null;

        private static LogoCon Instance
        {
            get
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new LogoCon();
                    }
                    return _instance;
                }
            }
        }
        public LogoCon()
        {
            UnityApp = new UnityObjects.UnityApplication();
        }

        public static LogoCon Get()
        {
            return Instance;
        }


        ///şehir listesini keşte tutmak için
        ///
   

    }
}