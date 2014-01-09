using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;

namespace OpenPseudonymiser
{
    

    public partial class DataPreview : Window
    {

        public DataTable prevData;

        string _inputFile;

        public DataPreview(string fileName)
        {
            _inputFile = fileName;

            prevData = new DataTable();
            
            

            
            


            FileStream fileStream = new FileStream(_inputFile, FileMode.Open, FileAccess.Read);
            
            using (StreamReader streamReader = new StreamReader(fileStream))
            {
                string firstLine = streamReader.ReadLine();
                string[] cols = firstLine.Split(',');
                foreach (string colname in cols)
                {
                    prevData.Columns.Add(new DataColumn(colname, typeof(string)));                    
                }

                string secondLine = streamReader.ReadLine();
                string[] datacols = secondLine.Split(',');
                
                var row = prevData.NewRow();            
                row = prevData.NewRow();
                int index = 0;
                foreach (string colname in cols)
                {
                    row[colname] = datacols[index];
                    index ++;
                }
                prevData.Rows.Add(row);

            }



            this.DataContext = prevData;

            InitializeComponent();

        

        }



    }
}
