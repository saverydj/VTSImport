using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using STARS.Applications.VETS.Interfaces.Entities;

namespace STARS.Applications.VETS.Plugins.VTS.Interface
{
    public partial class SelectPrecondition : Form
    {
        public TestResult SelectedTestRecord { get; private set; }
        private List<Precondition> _precondition = new List<Precondition>();
        private string _sortOrder = String.Empty;
        private List<TestResult> _testResults;
        private string _vin;
        private bool _isShowAll = false;

        public SelectPrecondition(List<TestResult> testResults, string vin)
        {
            _testResults = testResults;
            _vin = vin;
            InitializeComponent();
            label2.Text = String.Format("VIN = {0}", _vin);
            PopulatePreconditionList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selectedTestRecordName = dataGridView1.CurrentRow.Cells[0].Value.ToString();
            SelectedTestRecord = _testResults.Where(x => x.Name == selectedTestRecordName).FirstOrDefault();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _isShowAll = !_isShowAll;
            PopulatePreconditionList();
            button3.Text = (_isShowAll ? "Matching VIN" : "All Test Results");
        }

        private void PopulatePreconditionList()
        {
            string thisVIN;
            _precondition.Clear();
            foreach (TestResult testResult in _testResults)
            {
                thisVIN = testResult.GetProperty<string>("VIN", null);
                if (_isShowAll || (thisVIN != null && thisVIN == _vin))
                {
                    _precondition.Add(new Precondition(
                                      testResult.Name,
                                      testResult.GetProperty<string>("TestIDNumber", ""),
                                      testResult.GetProperty<string>("TestTypeCode", ""),
                                      testResult.GetProperty<string>("VIN", "")
                                      ));
                }
            }
            PopulateDataGrid();
            SetColumnSizes();
        }

        private void PopulateDataGrid()
        {
            dataGridView1.DataSource = new BindingSource(new BindingList<Precondition>(_precondition), null);
        }

        private void SetColumnSizes()
        {
            string columnName;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                columnName = column.Name;
                if (columnName == "TestResultsName") column.Width = 250;
                if (columnName == "TestIDNumber") column.Width = 100;
                if (columnName == "TestTypeCode") column.Width = 100;
                if (columnName == "VIN") column.Width = 175;
            }
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (_sortOrder != columnName)
            {
                _precondition = _precondition.OrderBy(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                _sortOrder = columnName;
            }
            else
            {
                _precondition = _precondition.OrderByDescending(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                _sortOrder = String.Empty;
            }
            PopulateDataGrid();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

    }
}
