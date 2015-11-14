using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RxApp
{
    public partial class Form1 : Form
    {
        private HelionBaseReader dataReader;
        private DataTable dataTable;

        private IObservable<IList<DataRow>> bufferedBooks;
        private IDisposable observer;

        bool isAfterInit = false;

        public Form1()
        {
            InitializeComponent();
            InitDataTable();

            dataReader = new HelionBaseReader(Resources.HelionBase, dataTable);

            comboBox1.DataSource = dataReader.ReadTopics();
            dataGridView1.DataSource = dataTable;

            CreateObservable(250);

            isAfterInit = true;
        }

        public void InitDataTable()
        {
            if (dataTable != null) dataTable.Dispose();
            dataTable = new DataTable();
            dataTable.Columns.Add(Resources.ColumnNameNumber, typeof(int));
            dataTable.Columns.Add(Resources.ColumnNameTitle, typeof(String));
            dataTable.Columns.Add(Resources.ColumnNameAuthor, typeof(String));
            dataTable.Columns.Add(Resources.ColumnNameTopic, typeof(String));
            dataTable.Columns.Add(Resources.ColumnNameCover, typeof(String));
            dataTable.Columns.Add(Resources.ColumnNamePrice, typeof(decimal));
        }

        private void BookDataRows(IEnumerable<DataRow> dataRows)
        {
            if (dataGridView1.IsHandleCreated)
            {
                foreach (var dataRow in dataRows)
                {
                    ((DataTable)dataGridView1.DataSource).Rows.Add(dataRow);
                }
                dataGridView1.FirstDisplayedScrollingRowIndex = ((DataTable)dataGridView1.DataSource).Rows.Count;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isAfterInit)
            {
                String selectedTopic = comboBox1.SelectedItem.ToString();
                dataReader.topic = selectedTopic == Resources.EmptyTopic ? null : selectedTopic;
                SubscribeObserver();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            dataReader.titleLike = textBox1.Text;
            SubscribeObserver();
        }

        private void SubscribeObserver()
        {
            dataTable.Clear();
            if (observer != null)
            {
                observer.Dispose();
            }

            observer = bufferedBooks.Subscribe(BookDataRows);
        }

        ~Form1()
        {
            if (dataTable != null) dataTable.Dispose();
            if (observer != null) observer.Dispose();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            CreateObservable(Decimal.ToInt32(numericUpDown1.Value));
        }

        private void CreateObservable(int bufferedResults)
        {
            bufferedBooks = Observable.Interval(TimeSpan.FromMilliseconds(100), Scheduler.Default).
                Zip(dataReader.ReadBooks().ToObservable().Buffer(bufferedResults), (a, b) => b).ObserveOn(SynchronizationContext.Current);
        }
    }
}
