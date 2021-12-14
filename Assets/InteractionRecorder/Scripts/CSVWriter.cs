using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// The <c>CSVWriter</c> class contains all methods for opening a file, exporting all types of data to a string, put it in CSV format and track the size of the file.
/// </summary>
public class CSVWriter
{
    private int sizeOfFile;
    public int GetSizeOfFile() { return sizeOfFile; }
    private StreamWriter streamWriter;
    
    public CSVWriter(string filepath)
    {
        //Debug.Log("Culture = " + CultureInfo.CurrentCulture.Name + ", writer list separator = " + CultureInfo.CurrentCulture.TextInfo.ListSeparator + ", and decimal separator = " + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        sizeOfFile = 0;
        streamWriter = new StreamWriter(filepath);
    }

    string Export(string s)
    {
        return s;
    }

    string Export(int i)
    {
        return i.ToString();
    }

    string Export(float f)
    {
        return f.ToString();
    }

    string Export(long l)
    {
        return l.ToString();
    }

    string Export(float[] l)
    {
        string exportString = "";

        for (int i = 0; i < l.Length; i++)
        {
            if (i != 0)
                exportString += CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            exportString += Export(l[i]);
        }

        return exportString;
    }

    string Export(Quaternion q)
    {
        return Export(q.eulerAngles);
    }

    string Export(Vector3 v)
    {
        return Export(v.x) + CultureInfo.CurrentCulture.TextInfo.ListSeparator + Export(v.y) + CultureInfo.CurrentCulture.TextInfo.ListSeparator + Export(v.z);
    }

    string Export(ActionType type)
    {
        return Export((int)type);
    }

    string Export(SpecialChars sc)
    {
        switch (sc)
        {
            case SpecialChars.EMPTY_VECTOR3:
                return CultureInfo.CurrentCulture.TextInfo.ListSeparator + CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        }

        return "";
    }

    public void Write(string toWrite, bool addSeparator = false)
    {
        if (addSeparator)
            toWrite += CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        sizeOfFile += System.Text.ASCIIEncoding.ASCII.GetByteCount(toWrite);
        streamWriter.Write(toWrite);
    }

    public void WriteLine(string toWrite)
    {
        toWrite += streamWriter.NewLine;
        sizeOfFile += System.Text.ASCIIEncoding.ASCII.GetByteCount(toWrite);
        streamWriter.Write(toWrite);
    }

    public void Write(dynamic[] args)
    {
        string toWrite = Export(args[0]);

        for (int i = 1; i < args.Length; i++)
            toWrite += CultureInfo.CurrentCulture.TextInfo.ListSeparator + Export(args[i]);

        sizeOfFile += System.Text.ASCIIEncoding.ASCII.GetByteCount(toWrite);
        streamWriter.Write(toWrite);
    }

    public void WriteLine(params dynamic[] args)
    {
        string toWrite = Export(args[0]);

        for (int i = 1; i < args.Length; i++)
            toWrite += CultureInfo.CurrentCulture.TextInfo.ListSeparator + Export(args[i]);

        toWrite += streamWriter.NewLine;
        sizeOfFile += System.Text.ASCIIEncoding.ASCII.GetByteCount(toWrite);
        streamWriter.Write(toWrite);
    }

    public void Close()
    {
        streamWriter.Flush();
        streamWriter.Close();
    }
}
