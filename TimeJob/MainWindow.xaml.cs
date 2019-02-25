using System;
using System.Data;
using System.Linq;
using System.Windows;
using Microsoft.VisualBasic.FileIO;

namespace TimeJob
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }


    public DataTable Data;
    private void LoadCSVOnDataGridView(string fileName)
    {
      try
      {
        ReadCSV csv = new ReadCSV(fileName);

        try
        {
          Data = csv.readCSV;
        }
        catch (Exception ex)
        {
          throw new Exception(ex.Message);
        }
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
      var configWindow = new TimerConfig();
      configWindow.InitializeComponent();
      configWindow.Show();
    }

    private void MenuItem_Click_1(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }

  public class ReadCSV
  {
    public DataTable readCSV;

    public ReadCSV(string fileName, bool firstRowContainsFieldNames = true)
    {
      readCSV = GenerateDataTable(fileName, firstRowContainsFieldNames);
    }

    private static DataTable GenerateDataTable(string fileName, bool firstRowContainsFieldNames = true)
    {
      DataTable result = new DataTable();

      if (fileName == "")
      {
        return result;
      }

      string delimiters = ",";
      string extension = System.IO.Path.GetExtension(fileName);

      if (extension.ToLower() == "txt")
        delimiters = "\t";
      else if (extension.ToLower() == "csv")
        delimiters = ",";

      using (TextFieldParser tfp = new TextFieldParser(fileName))
      {
        tfp.SetDelimiters(delimiters);

        // Get The Column Names
        if (!tfp.EndOfData)
        {
          string[] fields = tfp.ReadFields();

          for (int i = 0; i < fields.Count(); i++)
          {
            if (firstRowContainsFieldNames)
              result.Columns.Add(fields[i]);
            else
              result.Columns.Add("Col" + i);
          }

          // If first line is data then add it
          if (!firstRowContainsFieldNames)
            result.Rows.Add(fields);
        }

        // Get Remaining Rows from the CSV
        while (!tfp.EndOfData)
          result.Rows.Add(tfp.ReadFields());
      }

      return result;
    }
  }
}
