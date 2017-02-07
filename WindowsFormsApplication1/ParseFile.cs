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
        private List<string> _newFileData;
        private List<string> _newFileDataDuplicates;
        private List<string> _newFileDept;

        private TextFieldParser _csvParser;

        private DateTime _date;

        private bool _filterByDateTime;

        private Dictionary<string, string> _conversionTable;

        private List<string> _lineDictionary;
        private Dictionary<string, int> _deptDictionary; 

        private int numberOfZeroSerialsDeleted;
        private int numberOfCityNamesUpdated;
        private int numberOfAddressNamesUpdated;
        private int numberOfMikudsUpdated;

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

                SeperateFileDictionaryToLists();

                InitWrite(targetUrl[i]);
                FileStream fileStream = new FileStream(targetUrl[i], FileMode.CreateNew);
                using (_writer = new StreamWriter(fileStream, Encoding.GetEncoding("windows-1255")))
                {
                    WriteToFile(_newFileData, string.Empty);
                }
                //if (_newFileDataDuplicates.Count > 0)
                //{
                //    var duplicatesFileName = targetUrl[i].Replace(".csv", "_Duplicate.csv");
                //    fileStream = new FileStream(duplicatesFileName, FileMode.CreateNew);
                //    using (_writer = new StreamWriter(fileStream, Encoding.GetEncoding("windows-1255")))
                //    {
                //        WriteToFile(_newFileDataDuplicates, _newFileData[0]);
                //    }
                //}
                var minusDept = _deptDictionary.Where(x => x.Value <= 0);
                foreach (var pair in minusDept)
                {
                    _newFileDept.Add(String.Join(",", pair.Key, pair.Value.ToString()));
                }
                if (_newFileDept.Count > 0)
                {
                    var deptFileName = targetUrl[i].Replace(".csv", "_Dept.csv");
                    fileStream = new FileStream(deptFileName, FileMode.CreateNew);
                    using (_writer = new StreamWriter(fileStream, Encoding.GetEncoding("windows-1255")))
                    {
                        var deptFileHeader = "Name, Dept";
                        WriteToFile(_newFileDept, deptFileHeader);
                    }
                }

                Quit();
            }

            //var duplicates = _newFileDataDuplicates.Count > 0 ? "\r\n Duplicates where found, Duplicates file was created." : string.Empty;
            var customersWithDebts = _newFileDept.Count > 0 ? "\r\n People with debt where found, Dept file was created." : string.Empty;
            var cityNamesChanged = numberOfCityNamesUpdated > 0 ?
                string.Format("\r\n Number of city names changed: {0}", numberOfCityNamesUpdated)
                : string.Empty;
            var zipCodesChanged = numberOfMikudsUpdated > 0 ?
                string.Format("\r\n Number of zip codes changed: {0}", numberOfMikudsUpdated)
                : string.Empty;
            var rowsDeleted = numberOfZeroSerialsDeleted > 0 ?
                string.Format("\r\n Number of rows with zero or no serial deleted: {0}", numberOfZeroSerialsDeleted)
                : string.Empty;

            MessageBox.Show(
                string.Format("done {0}{1}{2}{3}", customersWithDebts, cityNamesChanged, zipCodesChanged,
                    rowsDeleted));
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
            if (targetUrl == null) return;
            if (File.Exists(targetUrl)) File.Delete(targetUrl);
            if (File.Exists(targetUrl + "_Duplicates")) File.Delete(targetUrl + "_Duplicates");
        }

        private bool InitRead(string OrigionalFileUrl, string TargetFileUrl)
        {
            if (OrigionalFileUrl == null || TargetFileUrl == null)
                return false;

            _newFileData = new List<string>();
            _newFileDataDuplicates = new List<string>();
            _newFileDept = new List<string>();

            return true;
        }

        private void SeperateFileDictionaryToLists()
        {
            foreach (var line in _lineDictionary)
            {
                _newFileData.Add(line);
            }
            //var newFileParts = _lineDictionary.Where(x => x.Value.Count == 1).Select(x => x.Value).ToList();
            //foreach (var part in newFileParts.SelectMany(newFilePart => newFilePart))
            //{
            //    _newFileData.Add(part);
            //}
            //var newFileDuplicatesParts = _lineDictionary.Where(x => x.Value.Count > 1).Select(x => x.Value).ToList();
            //foreach (var part in newFileDuplicatesParts.SelectMany(newFileDuplicatePart => newFileDuplicatePart))
            //{
            //    _newFileDataDuplicates.Add(part);
            //}
        }

        private void Parse()
        {
            _lineDictionary = new List<string>();
            _deptDictionary = new Dictionary<string, int>();
            numberOfZeroSerialsDeleted = 0;
            numberOfCityNamesUpdated = 0;
            numberOfAddressNamesUpdated = 0;
            numberOfMikudsUpdated = 0;
            var firstLine = true;
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                var parts = new List<string>();
                for (var i = 0; i < 20; i++)
                {
                    parts.Add(LineSplitter(line).ElementAt(i));
                }
                if (firstLine)
                {
                    _lineDictionary.Add(string.Join(",", parts));
                    firstLine = false;
                    continue;
                }

                if (parts[15] == null || parts[15] == string.Empty)
                {
                    numberOfZeroSerialsDeleted++;
                    continue;
                }

                int serialNum;
                var isNum = int.TryParse(parts[15], out serialNum);

                if (isNum && serialNum == 0)
                {
                    numberOfZeroSerialsDeleted++;
                    continue;
                }

                if (parts[16].Contains('-'))
                {
                    parts[16] = parts[16].Replace("-", "");
                    parts[16] = String.Format("-{0}", parts[16]);
                }
                if (parts[19].Contains('-'))
                {
                    parts[19] = parts[19].Replace("-", "");
                    parts[18] = String.Format("-{0}", parts[18]);
                }

                if (parts[6] == null || parts[6] == string.Empty)
                {
                    numberOfAddressNamesUpdated++;
                    parts[6] = "מען";
                }
                int mikud;
                var mikudIsNum = int.TryParse(parts[7], out mikud);

                if (parts[7] == null || parts[7] == string.Empty || !mikudIsNum || parts[7].Length != 5)
                {
                    numberOfMikudsUpdated++;
                    parts[7] = "11111";
                }
                if (parts[8] == null || parts[8] == string.Empty)
                {
                    numberOfCityNamesUpdated++;
                    parts[8] = "מען";
                }
                DateTime currentLinesDate;
                var successfullParse = DateTime.TryParse(parts[19], out currentLinesDate);
                if (successfullParse && currentLinesDate < _date) continue;

                var unconvertedValue = parts[14].Trim();
                if (_conversionTable.ContainsKey(unconvertedValue))
                {
                    parts[14] = _conversionTable[unconvertedValue];
                }

                _lineDictionary.Add(string.Join(",", parts));

                int dept;
                var isInt = int.TryParse(parts[18], out dept);

                if (_deptDictionary.ContainsKey(parts[3]) && isInt)
                {
                    _deptDictionary[parts[3]] += dept;
                }
                else if (isInt)
                {
                    _deptDictionary.Add(parts[3], dept);
                }
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

        private void WriteToFile(IEnumerable<string> data, string extraContentToWriteFirst)
        {
            if (!string.IsNullOrEmpty(extraContentToWriteFirst))
            {
                _writer.WriteLine(extraContentToWriteFirst);
            }
            foreach (var line in data)
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
