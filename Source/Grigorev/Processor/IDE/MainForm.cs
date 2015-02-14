﻿using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.FSharp.Core;
using System.Windows.Forms;
using Processor;
using Controller;
using TTA;
using System.Reactive;
using System.Runtime.CompilerServices;

namespace IDE
{
	public partial class MainForm : Form
	{
		private IController<int> ctrl = new Controller<int>(Controller<int>.IntPresetArray);

		private IObservable<object> newClick;
		private IObservable<object> openClick;
		private IObservable<object> exitClick;
		private IObservable<object> saveClick;
		private IObservable<object> saveAsClick;
		private IObservable<object> initClick;
		private IObservable<object> clearClick;
		private IObservable<object> clearOnRunClick;
		private IObservable<object> showGridClick;
		private IObservable<object> compileClick;
		private IObservable<object> checkClick;
		private IObservable<object> runClick;
		private IObservable<object> debugClick;
		private IObservable<object> stepClick;
		private IObservable<object> stopDebugClick;
		private IObservable<object> threadsClick;
		private IObservable<object> multiThreadClick;
		private IObservable<object> aboutClick;
		private IObservable<object> formResized;
		private IObservable<object> panel1Resized;
		private IObservable<EventPattern<ControlEventArgs>> controlAdded;
		private IObservable<EventPattern<ControlEventArgs>> controlRemoved;
		private IObservable<EventPattern<DataGridViewCellEventArgs>> rowClicked;
		private IObservable<EventPattern<FormClosingEventArgs>> formClosing; 

		public MainForm()
		{
			InitializeComponent();

			InitEvents();
			SubscribeEvents();

			InitEditor();

			clearOnRunToolStripMenuItem.Checked = ctrl.ClearOnRun;
			updateProcessorState();
			//UpdateGrid();

			errorsDataGridView.Columns.Add("Row", "Row");
			errorsDataGridView.Columns.Add("Col", "Col");
			errorsDataGridView.Columns.Add("Message", "Message");
		}

		private void InitEvents()
		{
			newClick = Observable.FromEventPattern(h => newToolStripMenuItem.Click += h, h => newToolStripMenuItem.Click -= h);
			openClick = Observable.FromEventPattern(h => loadToolStripMenuItem.Click += h, h => loadToolStripMenuItem.Click -= h);
			exitClick = Observable.FromEventPattern(h => exitToolStripMenuItem.Click += h, h => exitToolStripMenuItem.Click -= h);
			saveClick = Observable.FromEventPattern(h => saveToolStripMenuItem.Click += h, h => saveToolStripMenuItem.Click -= h);
			saveAsClick = Observable.FromEventPattern(h => saveAsToolStripMenuItem.Click += h, h => saveAsToolStripMenuItem.Click -= h);
			initClick = Observable.FromEventPattern(h => configureToolStripMenuItem.Click += h, h => configureToolStripMenuItem.Click -= h);
			clearClick = Observable.FromEventPattern(h => clearToolStripMenuItem.Click += h, h => clearToolStripMenuItem.Click -= h);
			clearOnRunClick = Observable.FromEventPattern(h => clearOnRunToolStripMenuItem.Click += h, h => clearOnRunToolStripMenuItem.Click -= h);
			showGridClick = Observable.FromEventPattern(h => showGridToolStripMenuItem.Click += h, h => showGridToolStripMenuItem.Click -= h);
			compileClick = Observable.FromEventPattern(h => compileToolStripMenuItem.Click += h, h => compileToolStripMenuItem.Click -= h);
			checkClick = Observable.FromEventPattern(h => checkToolStripMenuItem.Click += h, h => checkToolStripMenuItem.Click -= h);
			runClick = Observable.FromEventPattern(h => runWoDebugToolStripMenuItem.Click += h, h => runWoDebugToolStripMenuItem.Click -= h);
			debugClick = Observable.FromEventPattern(h => debugToolStripMenuItem1.Click += h, h => debugToolStripMenuItem1.Click -= h);
			stepClick = Observable.FromEventPattern(h => stepToolStripMenuItem.Click += h, h => stepToolStripMenuItem.Click -= h);
			stopDebugClick = Observable.FromEventPattern(h => stopDebuggingToolStripMenuItem.Click += h, h => stopDebuggingToolStripMenuItem.Click -= h);
			threadsClick = Observable.FromEventPattern(h => threadsToolStripMenuItem.Click += h, h => threadsToolStripMenuItem.Click -= h);
			multiThreadClick = Observable.FromEventPattern(h => multiThreadToolStripMenuItem.Click += h, h => multiThreadToolStripMenuItem.Click -= h);
			aboutClick = Observable.FromEventPattern(h => aboutToolStripMenuItem.Click += h, h => aboutToolStripMenuItem.Click -= h);
			formResized = Observable.FromEventPattern(h => this.ResizeEnd += h, h => this.ResizeEnd -= h);
			panel1Resized = Observable.FromEventPattern(h => splitContainer2.Panel1.Resize += h, h => splitContainer2.Panel1.Resize -= h);
			controlAdded = Observable.FromEventPattern<ControlEventHandler, ControlEventArgs>(h => splitContainer2.Panel1.ControlAdded += h, h => splitContainer2.Panel1.ControlAdded -= h).Where(p => p.EventArgs.Control is TextBox);
			controlRemoved = Observable.FromEventPattern<ControlEventHandler, ControlEventArgs>(h => splitContainer2.Panel1.ControlRemoved += h, h => splitContainer2.Panel1.ControlRemoved -= h).Where(p => p.EventArgs.Control is TextBox);
			rowClicked = Observable.FromEventPattern<DataGridViewCellEventHandler, DataGridViewCellEventArgs>(h => errorsDataGridView.CellDoubleClick += h, h => errorsDataGridView.CellDoubleClick -= h);
			formClosing = Observable.FromEventPattern<FormClosingEventHandler, FormClosingEventArgs>(h => FormClosing += h, h => FormClosing -= h);
		}

		private void SubscribeEvents()
		{
			newClick.Subscribe(s => { if (savePrompt()) return; ctrl.New(); InitEditor(); });
			openClick.Subscribe(openHandler);
			exitClick.Subscribe(s => Close());
			saveClick.Subscribe(s => ctrl.Save());
			saveAsClick.Subscribe(saveAsHandler);
			initClick.Subscribe(initProcessorHandler);
			clearClick.Subscribe(s => { ctrl.Clear(); UpdateGrid(); });
			clearOnRunClick.Subscribe(s => { ctrl.ClearOnRun = !ctrl.ClearOnRun; clearOnRunToolStripMenuItem.Checked = ctrl.ClearOnRun; });
			showGridClick.Subscribe();
			compileClick.Subscribe(s => Compile());
			checkClick.Subscribe();
			runClick.Subscribe(s => Run());
			debugClick.Subscribe(s => StartDebug());
			stepClick.Subscribe(s => Step());
			stopDebugClick.Subscribe(e => StopDebug());
			threadsClick.Subscribe(threadsHandler);
			multiThreadClick.Subscribe();
			aboutClick.Subscribe(s => MessageBox.Show((char)169 + " Sergey Grigorev, 2014", "About"));
			formResized.Subscribe(s => InitEditor());
			panel1Resized.Subscribe(s => InitEditor());
			controlAdded.Subscribe(s => SubscribeOnTextBox(s.EventArgs.Control as TextBox));
			controlRemoved.Subscribe(s => UnsubscribeOnTextBox(s.EventArgs.Control as TextBox));
			rowClicked.Subscribe(s => { var r = errorsDataGridView.Rows[s.EventArgs.RowIndex]; var i = (int) r.Cells[0].Value; var j = (int) r.Cells[1].Value; SetCursorTo(i, j); });
			formClosing.Subscribe(s => closingHandler(s.EventArgs));

			ctrl.Alert += AlertHandler;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.B | Keys.Control | Keys.Shift))
			{
				Compile();
				return true;
			}
			else if (keyData == Keys.F5)
			{
				StartDebug();
				return true;
			}
			else if (keyData == (Keys.F5 | Keys.Control))
			{
				Run();
				return true;
			}
			else if (keyData == Keys.F10)
			{
				Step();
				return true;
			}
			else if (keyData == (Keys.Shift | Keys.F5))
			{
				StopDebug();
				return true;
			}
			else if (keyData == (Keys.Shift | Keys.Tab))
			{
				SwithTextBox(false);
				return true;
			}
			else if (keyData == Keys.Tab)
			{
				SwithTextBox(true);
				return true;
			}
			else return base.ProcessCmdKey(ref msg, keyData);
		}

		private void Run()
		{
			ctrl.Run();
			UpdateGrid();
		}

		private void SwithTextBox(bool forward = true)
		{
			var coll = splitContainer2.Panel1.Controls;
			int i;
			for (i = 0; i < coll.Count; i++)
			{
				var c = coll[i] as TextBox;
				if (c == null)
					return;
				if (c.Focused)
					break;
			}
			if (i == coll.Count)
				return;
			var old = coll[i] as TextBox;
			var p = old.SelectionStart;
			for (int j = 0; j < old.Lines.Length; j++)
			{
				p -= old.Lines[j].Length + 2;
				if (p < 0)
				{
					p = j;
					break;
				}
			}
			if (i == 0 && !forward)
				i = coll.Count - 1;
			else if (i == coll.Count - 1 && forward)
				i = 0;
			else i = forward ? i + 1 : i - 1;
			var cc = coll[i] as TextBox;
			if (cc == null)
				return;
			if (p >= cc.Lines.Length)
				p = cc.Lines.Length - 1;
			SetCursorTo(p, i, false);
			cc.Focus();
		}

		private void StartDebug()
		{
			ctrl.StartDebug();
			stepToolStripMenuItem.Enabled = stopDebuggingToolStripMenuItem.Enabled = true;
			debugToolStripMenuItem1.Enabled = false;
			UpdateStatusBar();
			DisableMenu();
			ToggleEditor(false);
		}

		private void DisableMenu()
		{
			fileToolStripMenuItem.Enabled = processorToolStripMenuItem.Enabled = buildToolStripMenuItem.Enabled = settingsToolStripMenuItem.Enabled = runWoDebugToolStripMenuItem.Enabled = false;
		}

		private void EnableMenu()
		{
			fileToolStripMenuItem.Enabled = processorToolStripMenuItem.Enabled = buildToolStripMenuItem.Enabled = settingsToolStripMenuItem.Enabled = runWoDebugToolStripMenuItem.Enabled = true;
		}

		private void Step()
		{
			ctrl.Step();
			UpdateGrid();
			if (!ctrl.InDebug)
				StopDebug();
		}

		private void StopDebug()
		{
			ctrl.StopDebug();
			stepToolStripMenuItem.Enabled = stopDebuggingToolStripMenuItem.Enabled = false;
			debugToolStripMenuItem1.Enabled = true;
			UpdateGrid();
			UpdateStatusBar();
			EnableMenu();
			ToggleEditor(true);
		}

		private void ToggleEditor(bool on)
		{
			foreach (var c in splitContainer2.Panel1.Controls)
			{
				var cc = c as TextBox;
				if (cc != null)
					cc.Enabled = on;
			}
		}

		private void UpdateStatusBar()
		{
			statusStrip1.BackColor = ctrl.InDebug ? Color.DarkOrange : DefaultBackColor;
		}

		private void Compile()
		{
			ctrl.Compile();
			UpdateErrors();
		}

		private void SetCursorTo(int row, int col, bool select = true)
		{
			if (row < 0 && col < 0)
				return;
			var tb = splitContainer2.Panel1.Controls[col < 0 ? 0 : col] as TextBox;
			if (row < 0)
			{
				tb.SelectAll();
				tb.Focus();
				return;
			}
			int sum = 0;
			int rl = tb.Lines[row].Length;
			for (int i = 0; i < row; i++)
				sum += tb.Lines[i].Length + 2;
			if (sum + rl > tb.Text.Length)
				return;
			tb.Select(select ? sum : sum + rl, select ? rl : 0);
			tb.Focus();
		}

		private void UnsubscribeOnTextBox(TextBox tb)
		{
			tb.TextChanged -= UpdateCode;
		}

		private void SubscribeOnTextBox(TextBox tb)
		{
			tb.TextChanged += UpdateCode;
		}

		private void InitEditor()
		{
			Text = string.Format("TTA IDE - {0}", ctrl.ProjectName);
			splitContainer2.Panel1.Controls.Clear();
			var n = ctrl.ThreadNumber;
			var w = splitContainer2.Panel1.Width;
			var h = splitContainer2.Panel1.Height;
			for (int i = 0; i < n; i++)
			{
				splitContainer2.Panel1.Controls.Add(new TextBox() { Multiline = true, Location = new Point(i * w / n, 0), Size = new Size(w / n, h), Text = ctrl.Source[i], Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204))) });
			}
			updateProcessorState();
		}

		private void UpdateCode(object s = null, EventArgs e = null)
		{
			StopDebug();
			var n = ctrl.ThreadNumber;
			var arr = new string[n];
			for (int i = 0; i < n; i++)
			{
				var l = splitContainer2.Panel1.Controls[i] as TextBox;
				arr[i] = l.Text;
			}
			ctrl.Update(arr);
			runWoDebugToolStripMenuItem.Enabled = debugToolStripMenuItem1.Enabled = false;
		}

		private void UpdateGrid()
		{
			var w = ctrl.GridWidth;
			var h = ctrl.GridHeight;
			gridDataGridView.Rows.Clear();
			gridDataGridView.Columns.Clear();
			for (int i = 0; i < w; i++)
				gridDataGridView.Columns.Add(i.ToString(), i.ToString());
			for (int i = 0; i < h; i++)
			{
				var row = new DataGridViewRow();
				row.HeaderCell.Value = i.ToString();
				for (int j = 0; j < w; j++)
					row.Cells.Add(new DataGridViewTextBoxCell());
				gridDataGridView.Rows.Add(row);
			}
			var cells = ctrl.ReadAll();
			foreach (GridCell<int> c in cells)
				gridDataGridView.Rows[c.Row].Cells[c.Col].Value = c.Value;
		}

		private void UpdateErrors()
		{
			var err = ctrl.CompilationErrors;
			errorsDataGridView.Rows.Clear();
			foreach (ErrorListItem e in err)
			{
				var row = new DataGridViewRow();
				row.Cells.Add(new DataGridViewTextBoxCell() { Value = e.Row });
				row.Cells.Add(new DataGridViewTextBoxCell() { Value = e.Col });
				row.Cells.Add(new DataGridViewTextBoxCell() { Value = e.Message });
				errorsDataGridView.Rows.Add(row);
			}
			runWoDebugToolStripMenuItem.Enabled = debugToolStripMenuItem1.Enabled = err.Length == 0;
		}

		private void AlertHandler(object s, AlertEventArgs e)
		{
			MessageBox.Show(e.Message);
		}

		private void openHandler(object s)
		{
			if (savePrompt())
				return;
			OpenFileDialog d = new OpenFileDialog();
			d.Filter = "Project files (*.asmprj)|*.asmprj|All files (*.*)|*.*";
			var dr = d.ShowDialog();
			if (dr == DialogResult.OK)
			{
				ctrl.Open(d.FileName);
				InitEditor();
			}
			runWoDebugToolStripMenuItem.Enabled = debugToolStripMenuItem1.Enabled = false;
		}

		private void saveAsHandler(object s)
		{
			SaveFileDialog d = new SaveFileDialog();
			d.Filter = "Project files (*.asmprj)|*.asmprj|All files (*.*)|*.*";
			var dr = d.ShowDialog();
			if (dr == DialogResult.OK)
			{
				ctrl.Save(d.FileName);
			}
		}

		private void initProcessorHandler(object s)
		{
			InitProcessorForm f = new InitProcessorForm();
			f.InitCode = ctrl.InitCode;
			var d = f.ShowDialog();
			if (d == DialogResult.OK)
			{
				var t = f.InitCode;
				var err = ctrl.Init(t);
				if (err != null)
				{
					compileToolStripMenuItem.Enabled = false;
					MessageBox.Show("Errors occured");
				}
				else
				{
					compileToolStripMenuItem.Enabled = true;
					UpdateCode();
					UpdateGrid();
				}
			}
			updateProcessorState();
		}

		private void updateProcessorState()
		{
			var ps = ctrl.FunctionsCount;
			if (ps == 0)
				processorStateToolStripMenuItem.Text = "Not ready";
			else processorStateToolStripMenuItem.Text = string.Format("Ready - {0}", ps);
		}

		private void threadsHandler(object s)
		{
			var f = new ThreadNumberForm() { Number = ctrl.ThreadNumber };
			var d = f.ShowDialog();
			if (d == DialogResult.OK)
				ctrl.ThreadNumber = f.Number;
			InitEditor();
		}

		private bool savePrompt()
		{
			if (ctrl.IsSaved)
				return false;
			var dr = MessageBox.Show("Save project before exit?", "Alert", MessageBoxButtons.YesNoCancel);
			if (dr == DialogResult.Yes)
			{
				if (ctrl.HasFilename)
					ctrl.Save();
				else saveAsHandler(null);
				return false;
			}
			if (dr == DialogResult.No)
				return false;
			return true;
		}

		private void closingHandler(FormClosingEventArgs e)
		{
			e.Cancel = savePrompt();
		}
	}
}
