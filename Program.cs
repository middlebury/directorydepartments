using Microsoft.Security.Application;
using System;
using System.IO;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using DirectoryDepartments.edu.middlebury.devweb;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace DirectoryDepartments
{
    class Program
    {
        static void Main(string[] args)
        {
            Departments fetcher = new Departments();
            XmlDocument departments = fetcher.getDepartments("");
            departments.Save("C:\\Uploads\\DirectoryDepartments.xml");
        }
    }

    class Departments
    {
        public Departments() { }

        public struct DirectoryDepartment
        {
            public string name;
            public string company;

            public DirectoryDepartment(string name, string company)
            {
                this.name = name;
                this.company = company;
            }
        }

        public class DirectoryDepartmentSortComparer : IComparer<DirectoryDepartment>
        {
            public int Compare(DirectoryDepartment x, DirectoryDepartment y)
            {
                return x.name.CompareTo(y.name);
            }
        }

        public XmlDocument getDepartments(string company)
        {
            XmlDocument departments = createXmlDocument("departments");

            string search = String.Empty;

            company = Microsoft.Security.Application.Encoder.LdapFilterEncode(company);

            if (company == "")
            {
                search = "(&(objectClass=user)" + company + ")";
            }
            else
            {
                search = "(&((objectClass=user))(company=*" + company + "*))";
            }

            String[] view = new String[] { "department", "company", "extensionAttribute7" };

            using (DirectorySearcher searcher = new DirectorySearcher(search, view))
            {
                searcher.PageSize = 2000;
                using (SearchResultCollection results = searcher.FindAll())
                {
                    var enumerator = results.GetEnumerator();

                    List<DirectoryDepartment> list = new List<DirectoryDepartment>();

                    while (enumerator.MoveNext())
                    {
                        SearchResult result = (SearchResult)enumerator.Current;

                        ResultPropertyValueCollection resultDepartments = result.Properties["department"];
                        ResultPropertyValueCollection resultCompanies = result.Properties["company"];

                        if (resultCompanies.Count > 0 && resultDepartments.Count > 0 && resultDepartments[0].ToString().Trim() != "")
                        {
                            DirectoryDepartment department = new DirectoryDepartment(resultDepartments[0].ToString(), resultCompanies[0].ToString());

                            if (!list.Contains(department) &&
                                (result.Properties["extensionattribute7"] == null ||
                                 result.Properties["extensionattribute7"].Count == 0 ||
                                 result.Properties["extensionattribute7"][0].ToString().Replace(',', ' ').Replace("\\n", "").Trim() == ""))
                            {
                                list.Add(department);
                            }
                        }
                    }

                    list.Sort(new DirectoryDepartmentSortComparer());

                    foreach (DirectoryDepartment department in list)
                    {
                        XmlElement xmlDepartment = departments.CreateElement("department");
                        xmlDepartment.SetAttribute("name", department.name);
                        xmlDepartment.SetAttribute("company", department.company);
                        departments.DocumentElement.AppendChild(xmlDepartment);
                    }

                    return departments;
                }
            }
        }

        private XmlDocument createXmlDocument(string root)
        {
            XmlDocument document = new XmlDocument();

            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement r = document.CreateElement(root);
            document.InsertBefore(declaration, document.DocumentElement);
            document.AppendChild(r);

            return document;
        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            throw new XmlSchemaValidationException();
        }
    }

}
