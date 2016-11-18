using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Straetusshockomatconverter
{
    public partial class Form1 : Form
    {
        private ParseFile _parser;
        private List<string> _originUrl;
        private List<string> _targetUrl;
        private FolderBrowserDialog _folderBrowserDialog;
        private OpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;
        private Dictionary<string, string> _configDictionary;

        public Form1()
        {
            InitializeComponent();
            InitializeParsers();
        }

        private void InitializeParsers()
        {
            _configDictionary = new Dictionary<string, string>();
            _parser = new ParseFile();
            _originUrl = new List<string>();
            _targetUrl = new List<string>();
            _folderBrowserDialog = new FolderBrowserDialog();
            _openFileDialog = new OpenFileDialog { Filter = @"CSV files (*.csv)|*.csv"};
            _saveFileDialog = new SaveFileDialog { Filter = @"CSV files (*.csv)|*.csv"};
        }

        private void Select_Click(object sender, EventArgs e)
        {
            selectSingle();
        }

        private void selectSingle()
        {
            if (_configDictionary == null || _configDictionary.Count == 0)
            {
                MessageBox.Show(@"Please set a config first!");
                return;
            }

            _originUrl = new List<string>();
            _targetUrl = new List<string>();

            if (_openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _originUrl.Add(_openFileDialog.FileName);
            }
            else
            {
                return;
            }

            if (_saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _targetUrl.Add(_saveFileDialog.FileName);
            }
            else
            {
                return;
            }

            _parser.SingleParse(_originUrl, _targetUrl, _configDictionary);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            selectBatch();
        }

        private void selectBatch()
        {
            if (_configDictionary == null || _configDictionary.Count == 0)
            {
                MessageBox.Show(@"Please set a config first!");
                return;
            }
            _originUrl = new List<string>();
            _targetUrl = new List<string>();
            string[] rawFiles;
            string saveDir;

            if (_folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                rawFiles = Directory.GetFiles(_folderBrowserDialog.SelectedPath);
            }
            else
            {
                return;
            }
            if (_folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                saveDir = _folderBrowserDialog.SelectedPath;
            }
            else
            {
                return;
            }

            foreach (var rawUrl in rawFiles)
            {
                var extension = Path.GetExtension(rawUrl);
                if (extension == null || !extension.Equals(".csv")) continue;

                _originUrl.Add(rawUrl);

                var path = saveDir + @"\" + Path.GetFileName(rawUrl);
                _targetUrl.Add(path);
            }

            _parser.SingleParse(_originUrl, _targetUrl, _configDictionary);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_openFileDialog.ShowDialog() != DialogResult.OK) return;

            var _reader = new StreamReader(_openFileDialog.FileName);
            using (_reader = new StreamReader(_openFileDialog.FileName, Encoding.GetEncoding("windows-1255")))
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    var split = line.Split(',');

                    _configDictionary.Add(split[0], split[1]);
                }
            }


            MessageBox.Show(@"Config is now set to:" + _openFileDialog.SafeFileName);
        }
    }   
}
