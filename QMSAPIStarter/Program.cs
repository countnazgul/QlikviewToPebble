using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QMSAPIStarter.QMSAPIService;
using QMSAPIStarter.ServiceSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Data;
using System.Globalization;

namespace QMSAPIStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new ConsoleOptions();
            CommandLine.Parser.Default.ParseArguments(args, options);

            for (int h = 0; h < args.Length; h++)
            {
                var ar = args[h].ToString();
                if (ar.ToLower(CultureInfo.InvariantCulture).IndexOf("-h", System.StringComparison.Ordinal) >= 0)
                {
                    Environment.Exit(0);
                }
            }

            var serverArg = options.Server;
            var filesArg = options.Params;
            var server = "";
            var files = "";

            if (serverArg == null)
            {
                //Console.WriteLine(GenerateReturnMsg("error",  "Missing server. See: wearable-qv --help for more info"));
                //Environment.Exit(0);
                server = "http://localhost:4799/QMS/Service";
            }
            else
            {
                server = serverArg;
            }

            if (filesArg == null)
            {
                //Console.WriteLine(GenerateReturnMsg("error", "Missing files and fields. See: wearable-qv --help for more info"));
                //Environment.Exit(0);

                files = @"C:\ProgramData\QlikTech\Documents\PebbleData1.qvw,PebbleData1;C:\ProgramData\QlikTech\Documents\PebbleData.qvw,PebbleData;"; //;C:\ProgramData\QlikTech\Documents\PebbleData1.qvw,PebbleData1;
            }
            else
            {
                files = filesArg;
            }


            QMSClient Client;
            Guid qvsId = new Guid("00000000-0000-0000-0000-000000000000");
            Guid qdsId = new Guid("00000000-0000-0000-0000-000000000000");

            //try
            {
                Client = new QMSClient("BasicHttpBinding_IQMS", server);
                string key = Client.GetTimeLimitedServiceKey();
                ServiceKeyClientMessageInspector.ServiceKey = key;
                
                List<ServiceInfo> myServices = Client.GetServices(ServiceTypes.All);

                foreach (ServiceInfo service in myServices)
                {

                    if (service.Type == ServiceTypes.QlikViewServer)
                    {
                        qvsId = service.ID;
                    }

                    if (service.Type == ServiceTypes.QlikViewDistributionService)
                    {
                        qdsId = service.ID;
                    }

                }

                DataTable dtFiles = new DataTable();
                DataColumn dcFile = new DataColumn("file");
                DataColumn dcField = new DataColumn("field");
                dtFiles.Columns.Add(dcFile);
                dtFiles.Columns.Add(dcField);

                var filesandfields = files.Split(';');

                try
                {
                    for (var i = 0; i < filesandfields.Length; i++)
                    {
                        if (filesandfields[i].Length > 0)
                        {
                            var singlefile = filesandfields[i].Split(',');
                            DataRow dr = dtFiles.NewRow();
                            dr["file"] = singlefile[0];
                            dr["field"] = singlefile[1];
                            dtFiles.Rows.Add(dr);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(GenerateReturnMsg("error", "Wrong format of passed file(s) parameteres. The format should be: <fullpath to the file>,<field name>;"));
                    Environment.Exit(0);
                }


                var jsObj = new JObject();
                var jsonArray = new JArray();
                jsObj.Add("data", jsonArray);

                for (var i = 0; i < dtFiles.Rows.Count; i++)
                {
                    string field = dtFiles.Rows[i]["field"].ToString();
                    string filePath = dtFiles.Rows[i]["file"].ToString();

                    QDSDocumentSessionConfiguration qds = new QDSDocumentSessionConfiguration();
                    qds.FilePath = filePath;
                    qds.QDSID = qdsId;

                    FileInfo fi = new FileInfo(filePath);

                    //try
                    {

                        var t = Client.CreateSession(qds);

                        if (t.OpenDocumentResult == DocumentState.OpenedSuccessfully)
                        {
                            //try
                            {
                                var data = Client.GetFieldContentList(t, field, FieldContentType.All, 0, 100);

                                List<string> categories = new List<string>();
                                foreach (var vals in data[0].Values)
                                {
                                    var d = vals.Text;
                                    var d1 = d.Split('_');
                                    categories.Add(d1[0]);
                                }

                                categories = categories.Distinct().ToList();

                                for (var c = 0; c < categories.Count; c++)
                                {
                                    var jsArea = new JObject();
                                    var jsAreaData = new JArray();
                                    jsArea.Add("areadata", jsAreaData);
                                    jsArea.Add("name", categories[c]);

                                    foreach (var vals in data[0].Values)
                                    {
                                        var d = vals.Text;
                                        var d1 = d.Split('_');
                                        if (d1[0] == categories[c])
                                        {
                                            var jsonObj = new JObject();
                                            jsonObj.Add("file", fi.FullName.ToString());
                                            jsonObj.Add("field", field);
                                            jsonObj.Add("status", t.OpenDocumentResult.ToString());
                                            jsonObj.Add("main", d1[0]);
                                            jsonObj.Add("category", d1[1]);
                                            jsonObj.Add("value", d1[2]);
                                            jsAreaData.Add(jsonObj);
                                        }
                                    }

                                    jsonArray.Add(jsArea);
                                }
                            }
                            //catch (System.Exception)
                            {
                            //    Console.WriteLine(GenerateReturnMsg("error", "Cannot get field content for field '" + field + "' in file " + fi.FullName));
                            //    Environment.Exit(0);
                            }
                        }
                        else
                        {
                            var jsonObj = new JObject();
                            jsonObj.Add("file", fi.FullName.ToString());
                            jsonObj.Add("field", field);
                            jsonObj.Add("status", t.OpenDocumentResult.ToString());
                            jsonObj.Add("main", "");
                            jsonObj.Add("category", "");
                            jsonObj.Add("value", "");
                            jsonArray.Add(jsonObj);

                        }

                        Client.CloseSession(t);
                    }
                    //catch (System.Exception)
                    {
                        //Console.WriteLine(GenerateReturnMsg("error", "Cannot create sessioin for for file " + fi.FullName));
                        //Environment.Exit(0);
                    }
                }

                Console.WriteLine(jsObj.ToString());

                Console.ReadLine();
            }
            //catch (System.Exception ex)
            {
            //    Console.WriteLine(GenerateReturnMsg("error", "Cannot reach the Qlikview Server at " + server));
            //    Environment.Exit(0);
            }
        
        }

        public static string GenerateReturnMsg(string msgtype, string msgtext)
        {
            var jsObj = new JObject();

            var jsObjInner = new JObject();
            jsObjInner.Add("status", msgtype);
            jsObjInner.Add("text", msgtext);           
            
            jsObj.Add("msg", jsObjInner);
            
            return jsObj.ToString();
        }

    }
}
