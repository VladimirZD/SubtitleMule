using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using CookComputing.XmlRpc;


namespace SubtitlesMule
{
    public class OpenSubtitlesClient
    {
        public struct Subtitle
        {
            public MemoryStream SubtitleStream;
            public string SubFormat;

            public Subtitle(MemoryStream stream, string subFormat)
            {
                this.SubtitleStream =stream;
                this.SubFormat = subFormat;
            }
        }

        private const string RESPONSE_STATUS_FIELD = "status";
        private const string RESPONSE_TOKEN_FIELD = "token";
        private const string RESPONSE_RESULT_OK = "200 OK";
        private const string SEARCH_PARAM_LANGUAGE_ID = "sublanguageid";
        private const string SEARCH_PARAM_MOVIE_HASH = "moviehash";
        private const string SEARCH_PARAM_MOVIE_SIZE = "moviebytesize";
        private const string SEARCH_RESPONSE_DATA_FIELD = "data";
        private const string DOWNLOAD_SUBTITLE_PARA_IDSUBTITLEFILE = "IDSubtitleFile";
        private const string DOWNLOADSUBTITLES_RESPONSE_DATA_FIELD = "data";

        private string AuthToken { get; set; }
        private ISubtitlesProxy RpcProxy;

        public OpenSubtitlesClient(string apiURL)
        {
            RpcProxy = XmlRpcProxyGen.Create<ISubtitlesProxy>();
            RpcProxy.KeepAlive = false;
            RpcProxy.Url = apiURL;
            RpcProxy.EnableCompression = true;
        }

        public bool Login(string username, string password, string language, string useragent)
        {
            XmlRpcStruct ResponseStruct = RpcProxy.LogIn(username, password, language, useragent);
            var responseCode = (string)ResponseStruct[RESPONSE_STATUS_FIELD];

            if (responseCode == RESPONSE_RESULT_OK)
            {
                AuthToken = (string)ResponseStruct[RESPONSE_TOKEN_FIELD];
            }
            return AuthToken != "";
        }

        public bool LogOut()
        {
            XmlRpcStruct ResponseStruct = RpcProxy.LogOut(AuthToken);
            var responseCode = (string)ResponseStruct[RESPONSE_STATUS_FIELD];

            if (responseCode == RESPONSE_RESULT_OK)
            {
                AuthToken = "";
            }
            return AuthToken == "";
        }

        public Subtitle GetSubtitleByHash(string fileName, string languageCode)
        {
            Subtitle retValue = new Subtitle();
            XmlRpcStruct searchParams = new XmlRpcStruct();
            List<SubtitleSearchItem> subtitles = null;
            var fileInfo = new FileInfo(fileName);
            searchParams.Add(SEARCH_PARAM_LANGUAGE_ID, languageCode);
            searchParams.Add(SEARCH_PARAM_MOVIE_HASH, GetMovieHash(fileName));
            searchParams.Add(SEARCH_PARAM_MOVIE_SIZE, fileInfo.Length);

            XmlRpcStruct response = RpcProxy.SearchSubtitles(AuthToken, new object[1] { new XmlRpcStruct[1] { searchParams } });
            if ((string)response[RESPONSE_STATUS_FIELD] == RESPONSE_RESULT_OK)
            {
                subtitles = SearchReponse2List(response);
            }
            //Take one with best rating
            if (subtitles.Count > 0)
            {
                var subtitle = subtitles.OrderByDescending(item => item.SubRating).FirstOrDefault();
                var subtitleStream = DownloadSubtitles(subtitle.IDSubtitleFile);

                retValue = new Subtitle(subtitleStream, subtitle.SubFormat);
            }
            return retValue;
        }

        private List<SubtitleSearchItem> SearchReponse2List(XmlRpcStruct response)
        {
            var result = new List<SubtitleSearchItem>();
            foreach (XmlRpcStruct item in ((object[])response[SEARCH_RESPONSE_DATA_FIELD]))
            {
                result.Add(new SubtitleSearchItem(item));
            }
            return result;
        }

        private MemoryStream DownloadSubtitles (int idsubtitleFile)
        {
            XmlRpcStruct response = RpcProxy.DownloadSubtitles(AuthToken, new int[1] { idsubtitleFile });
            byte[] data = System.Convert.FromBase64String((String)((XmlRpcStruct)((object[])response[DOWNLOADSUBTITLES_RESPONSE_DATA_FIELD])[0])[DOWNLOADSUBTITLES_RESPONSE_DATA_FIELD]);
            
            using (MemoryStream ms = new MemoryStream(data))
            {
                byte[] buffer = new byte[BitConverter.ToInt32(data, 0)];
                ms.Position = 0;
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    gzip.Read(buffer, 0, buffer.Length);
                    var subtitleStream=  new MemoryStream(buffer);
                    return subtitleStream;
                }
            }
        }

        public static string GetMovieHash(string filename)
        {
            //Code from http://trac.opensubtitles.org/projects/opensubtitles/wiki/HashSourceCodes
            byte[] result;
            using (Stream input = File.OpenRead(filename))
            {
                result = ComputeMovieHash(input);
            }
            return ToHexadecimal(result);
        }
        private static byte[] ComputeMovieHash(Stream input)
        {
            long lhash;
            long streamsize;
            streamsize = input.Length;
            lhash = streamsize;

            long i = 0;
            byte[] buffer = new byte[sizeof(long)];
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }

            input.Position = Math.Max(0, streamsize - 65536);
            i = 0;
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }
            input.Close();
            byte[] result = BitConverter.GetBytes(lhash);
            Array.Reverse(result);
            return result;
        }
        private static string ToHexadecimal(byte[] bytes)
        {
            StringBuilder hexBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                hexBuilder.Append(bytes[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }
    }
}


