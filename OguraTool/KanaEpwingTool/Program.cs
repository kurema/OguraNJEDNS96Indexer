using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NMeCab;

namespace KanaEpwingTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("<html><head><title>小倉 - 自然科学系和英大辞典</title></head><body>");

            var nmparam = new NMeCab.MeCabParam();
            System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            nmparam.DicDir = "dic/ipadic";
            var meCabTagger = NMeCab.MeCabTagger.Create(nmparam);

            var genreList = new Dictionary<string,int>();

            while (true)
            {

                var title = Console.In.ReadLine();
                if (title == null) break;
                var content = Console.In.ReadLine();
                if (Console.In.ReadLine() != "") { throw new Exception(); }

                content=content.TrimStart();

                var titleBase = System.Text.RegularExpressions.Regex.Replace(title, @"\(.+?\)", "");
                titleBase = System.Text.RegularExpressions.Regex.Replace(title, @"\[.+?\]", "");
                var titleList = titleBase.Split(',');

                var genreMc=System.Text.RegularExpressions.Regex.Matches(title, @"\[.+?\]");
                var genres = new List<String>();
                foreach(System.Text.RegularExpressions.Match match in genreMc)
                {
                    genres.Add(match.Value.TrimStart('[').TrimEnd(']'));
                }

                var contentBase = System.Text.RegularExpressions.Regex.Replace(content, @"\[.+?\]", "");
                var contentList = contentBase.Split(';');
                for(int i = 0; i < contentList.Count(); i++)
                {
                    contentList[i]=contentList[i].TrimStart();
                    contentList[i] = System.Text.RegularExpressions.Regex.Replace(contentList[i], @"^an? ", "");
                    contentList[i] = System.Text.RegularExpressions.Regex.Replace(contentList[i], @"^the ", "");
                }

                Console.WriteLine("<dl><dt>" + title + "</dt>");
                foreach(var text in titleList)
                {
                    Console.WriteLine("<key type=\"表記\">" + text + "</key>");

                    try
                    {
                        var node = meCabTagger.ParseToNode(text).Next;
                        string furigana = "";
                        while (node != null && node.Stat!=MeCabNodeStat.Eos)
                        {
                            furigana += node.Feature.Split(',')[7];
                            node = node.Next;
                        }
                        Console.WriteLine("<key type=\"かな\">" + furigana + "</key>");
                    }
                    catch { }
                }
                foreach (var text in genres)
                {
                    Console.WriteLine("<key type=\"複合\" name=\"ジャンル\">" + text + "</key>");
                    if (genreList.Keys.Contains(text)) { genreList[text]++; }else
                    {
                        genreList.Add(text, 1);
                    }
                }
                foreach (var text in contentList)
                {
                    var ttext = text.TrimStart();
                    ttext = ttext.TrimEnd();
                    Console.WriteLine("<key type=\"表記\">" + ttext + "</key>");
                    var words = ttext.Split(' ', '[', ']', '(', ')');
                    foreach(var word in words)
                    {
                        var w = word.TrimStart().TrimEnd();
                        Console.WriteLine("<key type=\"クロス\">" + word + "</key>");
                    }
                }
                Console.WriteLine("<dd>" + content + "</dd></dl>");
            }
            Console.WriteLine("</body></html>");

            if (args.Count() > 0)
            {
                using (var sw = new System.IO.StreamWriter(args[0],false,System.Text.Encoding.GetEncoding("shift_jis")))
                {
                    sw.Write("<?xml version=\"1.0\" encoding=\"Shift_JIS\"?><complex><group name=\"ジャンル検索\"><category name=\"ジャンル\">");
                    foreach (var kvp in genreList)
                    {
                        if (kvp.Value > 2)
                        {
                            sw.WriteLine("<subcategory name=\""+kvp.Key+"\">");
                        }
                    }
                    sw.WriteLine("</category><keyword name=\"キーワード1\" /><keyword name=\"キーワード2\" /></group></complex>");
                }
            }
        }
    }
}
