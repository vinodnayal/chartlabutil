using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Collections.Generic;
using System.Web.Security;
using System.Web.Configuration;

using System.Text;
using System.IO;
using System.Collections;

/// <summary>
/// Summary description for CSVExporter
/// </summary>
public class CSVExporter
{
    public static void WriteToCSV(List<ArrayList> listData, string path)
    {
        
        StreamWriter writer = new StreamWriter(path);
        WriteToStream(writer, listData, true,true);
    }
    public static void WriteToStream(TextWriter stream, List<ArrayList> listData, bool header, bool quoteall)

        {
       
            //if (header)
            //{
            //    for (int i = 0; i < table.Columns.Count; i++)
            //    {
            //        WriteItem(stream, table.Columns[i].Caption, quoteall);
            //        if (i < table.Columns.Count - 1)
            //            stream.Write(',');
            //        else
            //            stream.Write('\n');

            //    }

            //}

            foreach (ArrayList row in listData)
            {
                int i = 0;
                foreach (object data in row)
                {
                   
                    WriteItem(stream, data, quoteall);                   
                    if (i < row.Count - 1)
                        stream.Write(',');
                    else
                        stream.Write('\n');

                    i++;

                }
            }
            stream.Flush();
            stream.Close();

        }
    private static void WriteItem(TextWriter stream, object item, bool quoteall)

        {

            if (item == null)

                return;

            string s = item.ToString();

            if (quoteall || s.IndexOfAny("\",\x0A\x0D".ToCharArray()) > -1)

                stream.Write("\"" + s.Replace("\"", "\"\"") + "\"");

            else

                stream.Write(s);

            stream.Flush();

        }
}


