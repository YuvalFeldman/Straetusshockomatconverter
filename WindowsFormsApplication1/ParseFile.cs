using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace Straetusshockomatconverter
{
    class ParseFile
    {
        private StreamReader _reader;
        private StreamWriter _writer;

        private List<string> newFileData;

        private TextFieldParser _csvParser;


        private Dictionary<string, string> _conversionTable;

        public void SingleParse(List<string> originUrl, List<string> targetUrl, Dictionary<string, string> configDictionary)
        {
            _conversionTable = configDictionary;
            for (var i = 0; i < originUrl.Count; i++)
            {
                if (!InitRead(originUrl[i], targetUrl[i]))
                {
                    MessageBox.Show("Could not complete the request");
                    continue;
                }

                using (_reader = new StreamReader(originUrl[i], Encoding.GetEncoding("windows-1255")))
                {
                    Parse();
                }
                InitWrite(targetUrl[i]);
                FileStream fileStream = new FileStream(targetUrl[i], FileMode.CreateNew);
                using (_writer = new StreamWriter(fileStream, Encoding.GetEncoding("windows-1255")))
                {
                    writeToFile();
                }

                Quit();
            }
            MessageBox.Show("done");
        }

        private void InitWrite(string targetUrl)
        {
            if (targetUrl == null)
            {
                return;
            }
            if (File.Exists(targetUrl))
            {
                File.Delete(targetUrl);
            }
        }

        private bool InitRead(string OrigionalFileUrl, string TargetFileUrl)
        {
            if (OrigionalFileUrl == null || TargetFileUrl == null)
                return false;

            newFileData = new List<string>();
            
            return true;
        }

        private void Parse()
        {
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                var parts = new List<string>();
                for (int i = 0; i < 19; i++)
                {
                    parts.Add(LineSplitter(line).ElementAt(i));
                }

                var temp = parts[14].Trim();
                if (_conversionTable.ContainsKey(temp))
                {
                    parts[14] = _conversionTable[temp];
                }

                newFileData.Add(string.Join(",", parts));
            }
        }

        IEnumerable<string> LineSplitter(string line)
        {
            int fieldStart = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ',')
                {
                    yield return line.Substring(fieldStart, i - fieldStart);
                    fieldStart = i + 1;
                }
                if (line[i] == '"')
                    for (i++; line[i] != '"'; i++) { }
            }
        }

        private void writeToFile()
        {
            foreach (var line in newFileData)
                _writer.WriteLine(line);
        }

        private void Quit()
        {
            bool flag = true;
            while (flag)
            {
                try
                {
                    if (_reader != null)
                    {
                        _reader.Close();
                    }
                    if (_writer != null)
                    {
                        _writer.Close();
                    }

                    flag = false;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

    }
}
