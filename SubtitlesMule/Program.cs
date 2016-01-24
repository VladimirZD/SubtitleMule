using System;
using NLog;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

//System.IO.Compression.FileSystem


namespace SubtitlesMule
{

    class Program
    {

        private static Logger _logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            _logger.Info("Starting...");
            var arguments = new CommandLineArguments();
            if (CommandLine.Parser.Default.ParseArguments(args, arguments) && (!string.IsNullOrEmpty(arguments.File) || (!string.IsNullOrEmpty(arguments.Directory))))
            {
                _logger.Trace(string.Format("File={0}", arguments.File));
                _logger.Trace(string.Format("Directory={0}", arguments.Directory));
                _logger.Trace(string.Format("Language={0}", arguments.Language));
                _logger.Trace(string.Format("RegisterFileTypes={0}", arguments.RegisterFileTypes));
                var langCode = arguments.Language;
                var client = LogIn(langCode);
                if (!string.IsNullOrEmpty(arguments.Directory))
                {
                    FindSubtitlesForFolder(client,arguments.Directory, langCode);
                }
                else
                {
                    FindSubtitlesForFile(client,arguments.File, langCode);
                }
                client.LogOut();
            }
            else
            {
                Console.WriteLine(arguments.GetUsage());
            }

            Console.WriteLine("Press any key to continue..");
            Console.ReadKey();
        }

        public static void FindSubtitlesForFolder(OpenSubtitlesClient client,string folder, string langCode)
        {
            var fileTypes = Properties.Settings.Default.FileTypesToScan.Split(';');
            var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Where(inS => fileTypes.Contains(System.IO.Path.GetExtension(inS),StringComparer.OrdinalIgnoreCase));
            foreach (var file in files)
            {
                FindSubtitlesForFile(client,file, langCode);
            }
        }

        private static OpenSubtitlesClient LogIn(string langCode)
        {
            var userName = Properties.Settings.Default.OpenSubtitlesUserName;
            var password = Properties.Settings.Default.OpenSubtitlesPassword;
            var passwordHash = "";
            using (MD5 md5Hash = MD5.Create())
            {
                passwordHash = GetMd5Hash(md5Hash, password);
            }
            var userAgent = Properties.Settings.Default.OpenSubtitlesUserAgent;
            var client = new OpenSubtitlesClient(Properties.Settings.Default.OpenSubtitleApiURL);
            _logger.Trace("Trying to login... ");
            var loginOK = client.Login(userName, passwordHash, langCode, userAgent);
            if (!loginOK)
            {
                _logger.Error("Login failed!");
            }
            return client;
        }
        
        public static void FindSubtitlesForFile(OpenSubtitlesClient client, string fileName, string langCode)
        {
            var subTitle = client.GetSubtitleByHash(fileName, langCode);
            if (subTitle.SubtitleStream != null)
            {
                SaveSubtitles(subTitle, fileName);
                _logger.Info("Subtitle for movie {0} found", fileName);
            }
            else
            {
                _logger.Info("Subtitle for movie {0} NOT FOUND", fileName);
            }

        }

        private static void SaveSubtitles(OpenSubtitlesClient.Subtitle subTitle, string movieFileName)
        {
            FileInfo fi = new FileInfo(movieFileName);
            var subTitlePath = fi.FullName.Replace(fi.Extension, "." + subTitle.SubFormat);
            SaveFileStream(subTitlePath, subTitle.SubtitleStream);
        }

        private static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        /*private static void FindSubtitlesForFile(string fileName, string langCode)
        {
            FileInfo fi = new FileInfo(fileName);
            string subtitleFile = fi.FullName.Replace(fi.Extension, ".srt");
            if (!File.Exists(subtitleFile))
            {
                using (WebClient client = new WebClient())
                {
                    var query = string.Format(Properties.Settings.Default.SearchURL, fi.Name.Replace(fi.Extension, ""), langCode);
                    string html = client.DownloadString(query);
                    var htmlDoc = new HtmlDocument()
                    {
                        OptionFixNestedTags = true,
                        OptionAutoCloseOnEnd = true
                    };

                    htmlDoc.LoadHtml(html);
                    HtmlNode img = htmlDoc.DocumentNode.SelectNodes(DOWNLOAD_LINK_TOKEN).First();
                    string downloadPageUrl = img.ParentNode.Attributes["href"].Value;
                    if (downloadPageUrl.Contains("http://www.subtitles4free.net/"))
                    {
                        string downloadPageHtml = client.DownloadString(downloadPageUrl);
                        string fileDownloadUrl = GetDownloadUrl(downloadPageHtml);
                        Stream responseStream = client.OpenRead(fileDownloadUrl);
                        SaveSubtitles(responseStream, fi.DirectoryName);
                        //http://www.getsubtitle.com/webService/download_subtitle.php?cod_bsplayer=34233428&post_date=17-12-2014
                        //http://www.getsubtitle.com/webService/download_subtitle.php?cod_bsplayer=37535025&post_date=10-05-2015

                    }
                    else
                    {
                        _logger.Error("Unsupported subtitle provider, aborting");
                    }
                }
            }
            else
            {
                _logger.Info(string.Format("File {0} exists!Skiping file", subtitleFile));
            }
        }
        */
        //private static void SaveSubtitles(Stream fileStream, string saveFolder)
        //{
        //    Stream unzippedEntryStream;
        //    ZipArchive archive = new ZipArchive(fileStream);
        //    foreach (ZipArchiveEntry entry in archive.Entries)
        //    {
        //        unzippedEntryStream = entry.Open();
        //        SaveFileStream(Path.Combine(saveFolder, entry.FullName), unzippedEntryStream);
        //    }

        //}
        private static void SaveFileStream(String path, Stream stream)
        {
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
                fileStream.Dispose();
            }
        }
        //private static string GetDownloadUrl(string downloadPageHtml)  
        //{
        //    var htmlDoc = new HtmlDocument()
        //    {
        //        OptionFixNestedTags = true,
        //        OptionAutoCloseOnEnd = true
        //    };
        //    htmlDoc.LoadHtml(downloadPageHtml);
        //    HtmlNode title = htmlDoc.DocumentNode.SelectNodes("//title").First();
        //    var titleID = title.InnerText.Split('|').Last().Trim();
        //    var searchToken = string.Format("cod_bsplayer={0}&post_date=", titleID);
        //    var startIndex = downloadPageHtml.IndexOf(searchToken);
        //    var fileDownloadUrlSuffix = downloadPageHtml.Substring(startIndex, 10 + searchToken.Length);
        //    var fileDownloadUrl = string.Format(Properties.Settings.Default.FileDownloadURL, fileDownloadUrlSuffix);
        //    return fileDownloadUrl;
        //}
    }
}

///TODO Implement http://trac.opensubtitles.org/projects/opensubtitles/wiki/DevReadFirst#Implementingopensubtitles.orgsupport