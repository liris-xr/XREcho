using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static List<Dictionary<string, object>> ReadCSV(string file, string filterColumn = null, string filterValue = null)
    {
        var list = new List<Dictionary<string, object>>();
        StreamReader reader = new StreamReader(file);

        string data = reader.ReadToEnd();
        var lines = Regex.Split(data, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Length; i++)
        {
            bool addEntry = true;

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                if (filterColumn != null && header[j].Equals(filterColumn) && filterValue != null && !value.Equals(filterValue))
                {
                    addEntry = false;
                    break;
                }

                if (value.Equals(""))
                    continue;

                object finalvalue = value;
                float f;
                if (float.TryParse(value, out f))
                    finalvalue = f;

                entry[header[j]] = finalvalue;

            }

            if (addEntry)
                list.Add(entry);
        }

        reader.Close();

        return list;
    }

}
