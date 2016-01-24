using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitlesMule
{
    public interface ISubtitlesProxy : IXmlRpcProxy
    {
        [XmlRpcMethod("LogIn")]
        XmlRpcStruct LogIn(string username, string password, string language, string useragent);

        [XmlRpcMethod("LogOut")]
        XmlRpcStruct LogOut(string token);

        [XmlRpcMethod("DownloadSubtitles")]
        XmlRpcStruct DownloadSubtitles(string token, int[] IDSubtitleFile);

        [XmlRpcMethod("SearchSubtitles")]
        XmlRpcStruct SearchSubtitles(string token, params object[] parameters);

        

    }
}
