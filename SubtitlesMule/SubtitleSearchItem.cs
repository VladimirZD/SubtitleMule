using System;
using CookComputing.XmlRpc;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitlesMule
{
    public class SubtitleSearchItem
    {
        //struct description http://trac.opensubtitles.org/projects/opensubtitles/wiki/XMLRPC#SearchSubtitles
        // public string MatchedBy { get; set; }
        //public String MovieNameEng { get; set; }
        //public String MovieHash { get; set; }
        //public String LanguageName { get; set; }
        public int IDSubtitleFile { get; set; }
        //public String SubDownloadLink { get; set; }
        public String SubFormat { get; set; }
        //public int SubBad { get; set; }
        public decimal SubRating { get; set; }
        public SubtitleSearchItem()
        {

        }

        public SubtitleSearchItem(XmlRpcStruct item)
        {
            var reflection = this.GetType();

            foreach (var key in item.Keys)
            {
                var propInfo = reflection.GetProperty(key.ToString(), BindingFlags.Instance | BindingFlags.Public);
                if (propInfo != null)
                {
                    propInfo.SetValue(this, Convert.ChangeType(item[key], propInfo.PropertyType, CultureInfo.CurrentUICulture));
                }

            }
        }
    }
}
