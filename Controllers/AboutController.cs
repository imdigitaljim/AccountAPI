using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AttributeRouting.Web.Mvc;

namespace AccountManager.Controllers
{
    public class AboutController : ApiController
    {
        public class About
        {
          public string author
          {
            get { return "James Bach"; }
          }

          public string source
          {
            get { return "../source/code.zip"; }
          }
        }

        [GET("About")]
        public object GetInfo()
        {
            return new About();
        }

    }
}
