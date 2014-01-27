using System;
using System.IO;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;
using DirectoryDepartments.edu.middlebury.devweb;
using System.Xml;

namespace DirectoryDepartments
{
    class Program
    {
        static void Main(string[] args)
        {
            WebDirectory directory = new WebDirectory();
            directory.Timeout = System.Threading.Timeout.Infinite;

            XmlNode departments = directory.getDepartments("");

            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlNode xmlRoot = doc.ImportNode(departments, true);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);
            doc.AppendChild(xmlRoot);
            doc.Save("C:\\Uploads\\DirectoryDepartments.xml");
        }
    }
}
