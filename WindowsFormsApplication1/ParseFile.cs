using System;
using System.Collections.Generic;
using System.Globalization;
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

        private DateTime _date;

        private bool _filterByDateTime;

        private Dictionary<string, string> _conversionTable;

        private Dictionary<int, int> SerialNumCount;

        public void SingleParse(List<string> originUrl, List<string> targetUrl, Dictionary<string, string> configDictionary, string dateText, bool dateFilterCheckbox)
        {
            if (!DateTimeParse(dateText, dateFilterCheckbox))
            {
                MessageBox.Show(@"The date you have entered is an invalid date.");
                return;
            }

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

                var duplicateSerialNumberDictionary = SerialNumCount.Where(x => x.Value > 1).Select(x => x.Key).ToList();

                if (duplicateSerialNumberDictionary.Count > 0)
                {
                    var badKeys = string.Join(", ", duplicateSerialNumberDictionary);
                    var message = $"There are duplicate serial numbers in the file: {badKeys}";
                    MessageBox.Show(message);
                    return;
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

        private bool DateTimeParse(string dateText, bool dateFilterCheckbox)
        {
            bool success;

            if (!dateFilterCheckbox)
            {
                _date = DateTime.MinValue;
                success = true;
            }
            else
            {
                try
                {
                    _date = DateTime.ParseExact(dateText, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    success = true;
                }
                catch (Exception)
                {
                    success = false;
                }
            }

            return success;
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
            SerialNumCount = new Dictionary<int, int>();
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                var parts = new List<string>();
                for (int i = 0; i < 20; i++)
                {
                    parts.Add(LineSplitter(line).ElementAt(i));
                }

                if (parts[15] != null && parts[15] != string.Empty)
                {
                    int serialNum;
                    var isNum = int.TryParse(parts[15], out serialNum);
                    if (isNum)
                    {
                        if (!SerialNumCount.ContainsKey(serialNum))
                        {
                            SerialNumCount.Add(serialNum, 1);
                        }
                        else
                        {
                            SerialNumCount[serialNum]++;
                        }
                    }
                }
                DateTime currentLinesDate;
                var successfullParse = DateTime.TryParse(parts[19], out currentLinesDate);
                if (successfullParse && currentLinesDate < _date) continue;

                var unconvertedValue = parts[14].Trim();
                if (_conversionTable.ContainsKey(unconvertedValue))
                {
                    parts[14] = _conversionTable[unconvertedValue];
                }

                newFileData.Add(string.Join(",", parts));
            }
        }

        IEnumerable<string> LineSplitter(string line)
        {
            var fieldStart = 0;
            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == ',')
                {
                    yield return line.Substring(fieldStart, i - fieldStart);
                    fieldStart = i + 1;
                }
                if (i == line.Length - 1)
                {
                    yield return line.Substring(fieldStart, i - fieldStart + 1);
                }
                if (line[i] != '"') continue;

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
