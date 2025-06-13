using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BornSYNC.Models
{
    public class MESAJmodel
    {
        public bool HasError{ get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}