﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IDE
{
    public partial class MainScreen : Form
    {

        public MainScreen()
        {

            InitializeComponent();
            var open = Observable.FromEventPattern(h => openButton.Click += h, h =>  openButton.Click -= h);
            open.ObserveOn(SynchronizationContext.Current).Subscribe(x => OpenFile(openButton));
            var save = Observable.FromEventPattern(h => saveButton.Click += h, h => saveButton.Click -= h);
            save.ObserveOn(SynchronizationContext.Current).Subscribe(x => SaveFile(saveButton));
            var start = Observable.FromEventPattern(h => startButton.Click += h, h => startButton.Click -= h);
            start.ObserveOn(SynchronizationContext.Current).Subscribe(x => Start(startButton));
            var debug = Observable.FromEventPattern(h => debugButton.Click += h, h => debugButton.Click -= h);
            debug.ObserveOn(SynchronizationContext.Current).Subscribe(x => Debug(debugButton));
            var step = Observable.FromEventPattern(h => stepButton.Click += h, h => stepButton.Click -= h);
            step.ObserveOn(SynchronizationContext.Current).Subscribe(x => NextStep(stopButton));
            var stop = Observable.FromEventPattern(h => stopButton.Click += h, h => stopButton.Click -= h);
            stop.ObserveOn(SynchronizationContext.Current).Subscribe(x => Stop(stopButton));
            var formClosing = Observable.FromEventPattern<FormClosingEventHandler, FormClosingEventArgs>(h => FormClosing += h, h => FormClosing -= h);
            formClosing.Subscribe(x => closing(x.EventArgs));
            var renum = Observable.FromEventPattern<DataGridViewRowsAddedEventHandler, DataGridViewRowsAddedEventArgs>(h => data.RowsAdded += h, h => data.RowsAdded -= h);
            renum.ObserveOn(SynchronizationContext.Current).Subscribe(x => NumerateDataGrid(x.EventArgs));
        }
        
        private Compiler.Compiler comp = new Compiler.Compiler();
        public string CreateCode
        {
            get { return richTextBox1.Text; }
        }
        int count = 0;

        private void Debug(object sender)
        {
            data.Rows.Clear();
            comp.Stop();
            try
            {
                comp.Compile(richTextBox1.Text);
                if (createErrorBox().Count == 0)
                {
                    startButton.Visible = false;
                    debugButton.Visible = false;
                    stepButton.Visible = true;
                    stopButton.Visible = true;
                    richTextBox1.Enabled = false;
                    errorsListBox.DataSource = new List<string>();
                }
            }
            catch(Compiler.CompileException e)
            {
                List<String> list = new List<String>();
                list.Add(e.Message);
                errorsListBox.DataSource = list;
            }
        }
        private void NextStep(object sender)
        {
            if (count < richTextBox1.Lines.Length)
            {
                Highlight();
                richTextBox1.Show();
                comp.Step(count);
                count++;
                this.CreateDataGrid(comp, data);
            }
            else
            {
                Stop(sender);
            }
        }
        private void Highlight()
        {
            //Clear previous
            richTextBox1.Select(0, richTextBox1.GetFirstCharIndexFromLine(count));
            richTextBox1.SelectionColor = System.Drawing.Color.Black;
            richTextBox1.SelectionBackColor = System.Drawing.Color.WhiteSmoke;

            //Char position
            int firstCharPosition = richTextBox1.GetFirstCharIndexFromLine(count);
            int ln = richTextBox1.Lines[count].Length;
            
            //Select
            richTextBox1.Select(firstCharPosition, ln);
            richTextBox1.Select();

            //Select Color
            richTextBox1.SelectionColor = System.Drawing.Color.White;
            richTextBox1.SelectionBackColor = System.Drawing.Color.Blue;
   
        }
        private void Stop(object sender)
        {
            richTextBox1.SelectAll();
            richTextBox1.SelectionBackColor = System.Drawing.Color.White;
            richTextBox1.SelectionColor = Color.Black;
            startButton.Visible = true;
            debugButton.Visible = true;
            stepButton.Visible = false;
            stopButton.Visible = false;
            richTextBox1.Enabled = true;
            comp.Stop();
            data.Rows.Clear();
            count = 0;
        }
        private void Start(object sender)
        {
            try
            {
                errorsListBox.DataSource = new List<string>();
                data.Rows.Clear();
                comp.Stop();
                comp.Compile(richTextBox1.Text);
                comp.Run();
                this.CreateDataGrid(comp, data);
            }
            catch (Compiler.ProcessorException e)
            {
                createErrorBox();
            }
            catch (Compiler.CompileException e)
            {
                List<String> list = new List<String>();
                list.Add(e.Message);
                errorsListBox.DataSource = list;
                comp.Stop();
            }
            catch (Compiler.RuntimeException e)
            {
                List<String> list = new List<String>();
                list.Add(e.Message);
                errorsListBox.DataSource = list;
                comp.Stop();
            }
            catch (Exception e)
            {
                createErrorBox();
            }
        }
        public List<string> createErrorBox()
        {
            HashSet<Tuple<String, int, int>> errs = comp.getErrorsList;
            List<string> _items = new List<string>();
            foreach (Tuple<String, int, int> i in errs)
            {
                _items.Add(i.Item1 + " in code line " + (i.Item2 + 1) + " operation " + (i.Item3 + 1));
            }
            data.Rows.Clear();
            this.errorsListBox.DataSource = _items;
            return _items;
        }
        private void CreateDataGrid(Compiler.Compiler compiler, DataGridView data)
        {
            data.RowCount = compiler.CountRows();
            for (int i = 0; i < compiler.CountCols(); i++)
            {
                //data[i]
                Dictionary<int, string> cells = compiler.getStringGrid(i);
                foreach (KeyValuePair<int, string> kvp in cells){
                    data[i, kvp.Key].Value = kvp.Value;
                }
            }
        }
        private void OpenFile(object sender)
        {
            try
            {
                var dr = MessageBox.Show("Save program before open?", "Alert", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Yes)
                {
                    if (SaveFile(saveButton))
                    {
                        open();
                    }
                }
                if (dr == DialogResult.No)
                {
                    open();                   
                }
            }
            catch (Exception) { };
        }
        private void open()
        {
            string filePath = "";
            OpenFileDialog Fd = new OpenFileDialog();
            Fd.Filter = "txt files (*.txt)|*.txt";
            if (Fd.ShowDialog() == DialogResult.OK)
            {
                filePath = Fd.FileName;
                string str = System.IO.File.ReadAllText(@filePath);
                richTextBox1.Text = str;
            }
        }
        private bool SaveFile(object sender)
        {
            string filePath = "";
            string str = richTextBox1.Text;

            SaveFileDialog Sd = new SaveFileDialog();
            Sd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (Sd.ShowDialog() == DialogResult.OK)
            {
                filePath = Sd.FileName;
                System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath);
                sw.Write(richTextBox1.Text);
                sw.Close();
                return true;
            }
            return false;
        }
        private void closing(FormClosingEventArgs e)
        {
            e.Cancel = saveOnClose(); ;
        }
        private bool saveOnClose()
        {
            var dr = MessageBox.Show("Save program before exit?", "Alert", MessageBoxButtons.YesNoCancel);
            if (dr == DialogResult.Yes)
            {
                SaveFile(saveButton);
                return false;
            }
            if (dr == DialogResult.No)
            {
                return false;
            }
            return true;
        }
        private void NumerateDataGrid(DataGridViewRowsAddedEventArgs e)
        {
            for (int i = 0; i < data.Rows.Count; i++)
            {
                data.Rows[i].HeaderCell.Value = String.Format("{0}", data.Rows[i].Index + 1);
            }
            data.AutoResizeRowHeadersWidth(0, DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }
    }
}
