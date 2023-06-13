using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity
{
    public class DBConnection
    {
        public string DBConnectionString { get { return ConfigurationManager.ConnectionStrings["DBConnectionString"].ToString(); } }
        public string SQLCommandTimeOut { get { return ConfigurationManager.AppSettings["SQLCommandTimeOut"].ToString(); } }
    }
}
