using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnywayAnyday.ReactiveWebServer.Contract
{
    public interface IHttpListenerConfigurator
    {
        void AddPrefix(string pref);
    }
}
