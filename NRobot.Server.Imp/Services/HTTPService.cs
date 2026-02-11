using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NRobot.Server.Imp.Domain;

namespace NRobot.Server.Imp.Services
{
    /// <summary>
    /// HTTP Listener service
    /// </summary>
    public class HttpService
    {
        //log4net
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpService));

        //properties
        private HttpListener _listener;
        private Thread _httpthread;
        private XmlRpcService _rpcService;
        private KeywordManager _keywordManager;
        private int _port;

        /// <summary>
        /// Constructor
        /// </summary>
        public HttpService(XmlRpcService rpcService, KeywordManager keywordManager, int port)
        {
            _rpcService = rpcService;
            _keywordManager = keywordManager;
            _port = port;
            //setup http listener
            _listener = new HttpListener();
            _listener.Prefixes.Add(string.Format("http://*:{0}/", _port));
            _httpthread = null;
        }

        /// <summary>
        /// Background HTTP Listener thread
        /// </summary>
        private void DoWork_Listener()
        {
            Log.Debug(string.Format("HTTP Listener started on port {0}", _port));
            _listener.Start();
            while (true)
            {
                try
                {
                    var reqcontext = _listener.GetContext();
                    var method = reqcontext.Request.HttpMethod;
                    Log.Debug(string.Format("Received Http request with method {0}", method));
                    if (method == "POST")
                    {
                        Task.Factory.StartNew(() => ProcessRequest(reqcontext));
                    }
                    else if (method == "DELETE")
                    {
                        Log.Debug("Closing http listener");
                        reqcontext.Response.StatusCode = 200;
                        reqcontext.Response.Close();
                        break;
                    }
                    else
                    {
                        WriteStatusPage(reqcontext.Response);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
            Log.Debug("HTTP listener thread has exited");
        }

        /// <summary>
        /// Processes a HTTP request
        /// </summary>
        private void ProcessRequest(HttpListenerContext context)
        {
            Log.Debug(string.Format("Processing Http request for Url : {0}", context.Request.Url));
            try
            {
                _rpcService.ProcessRequest(context);
            }
            catch (Exception e)
            {
                Log.Error(string.Format("Error processing HTTP request : {0}", e));
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
        }

        /// <summary>
        /// Starts the http listener and processor async
        /// </summary>
        public void StartAsync()
        {
            if (_httpthread == null)
            {
                if (IsPortInUse())
                    throw new Exception("Unable to start service, port already in use");
                _httpthread = new Thread(DoWork_Listener) { IsBackground = true };
                _httpthread.Start();
            }
        }

        /// <summary>
        /// Stop http listener
        /// </summary>
        public void Stop()
        {
            //stop listener
            if (_httpthread != null)
            {
                //send DELETE method call
                Log.Debug("Sending HTTP request to stop");
                using (var client = new HttpClient())
                using (
                    var request = new HttpRequestMessage(
                        HttpMethod.Delete,
                        string.Format("http://127.0.0.1:{0}/", _port)
                    )
                )
                using (var response = client.Send(request)) { }
                _httpthread.Join(Timeout.Infinite);
            }
            _httpthread = null;
            _listener.Close(); //free's the port
        }

        /// <summary>
        /// Checks if port is available
        /// </summary>
        private bool IsPortInUse()
        {
            bool inUse = false;
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == _port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }

        /// <summary>
        /// Writes status page to a browser
        /// </summary>
        private void WriteStatusPage(HttpListenerResponse response)
        {
            try
            {
                StringBuilder html = new StringBuilder();
                //setup html doc
                html.Append("<!doctype html>");
                html.Append("<html lang=\"en\"><head><meta charset=\"utf-8\">");
                html.Append(
                    "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">"
                );
                html.Append("<title>NRobotRemote</title>");
                html.Append("<style>");
                html.Append(
                    "*{box-sizing:border-box}body{margin:0;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;background:linear-gradient(180deg,#f7f9fc 0%,#eef2f7 100%);color:#1b2430}"
                );
                html.Append(".page{max-width:1100px;margin:32px auto;padding:0 20px}");
                html.Append(
                    ".hero{background:#fff;border-radius:16px;padding:24px 28px;box-shadow:0 10px 30px rgba(0,0,0,.08);display:flex;flex-wrap:wrap;gap:12px;align-items:center;justify-content:space-between}"
                );
                html.Append(".hero h1{margin:0;font-size:28px}");
                html.Append(".hero p{margin:4px 0 0;color:#4d5a6b}");
                html.Append(".search{display:flex;gap:10px;align-items:center}");
                html.Append(
                    ".search input{width:260px;max-width:70vw;padding:10px 12px;border:1px solid #d7dbe3;border-radius:10px;font-size:14px;outline:none;background:#fafbfe}"
                );
                html.Append(
                    ".search input:focus{border-color:#6b86e8;box-shadow:0 0 0 3px rgba(107,134,232,.18)}"
                );
                html.Append(".count{font-size:12px;color:#6b7789}");
                html.Append(".section{margin-top:22px}");
                html.Append(
                    ".card{background:#fff;border-radius:12px;padding:16px 18px;margin-top:14px;box-shadow:0 6px 18px rgba(0,0,0,.06)}"
                );
                html.Append(".card h3{margin:0 0 12px;font-size:18px}");
                html.Append("table{width:100%;border-collapse:collapse;font-size:14px}");
                html.Append(
                    "th,td{padding:10px 8px;border-bottom:1px solid #e6e9ef;vertical-align:top}"
                );
                html.Append("th{text-align:left;font-weight:600;color:#223046;background:#f4f6fa}");
                html.Append("tr:last-child td{border-bottom:none}");
                html.Append(".kw-row{cursor:pointer}");
                html.Append(
                    ".kw-row td:first-child::before{content:'+ ';color:#3658b3;font-weight:700}"
                );
                html.Append(".kw-row.is-open td:first-child::before{content:'- ';color:#3658b3}");
                html.Append(".kw-details{display:none;background:#fafbfe}");
                html.Append(".kw-details td{padding:12px 10px}");
                html.Append(".arg-table{width:100%;border-collapse:collapse;font-size:13px}");
                html.Append(
                    ".arg-table th,.arg-table td{padding:6px 6px;border-bottom:1px solid #e9edf5}"
                );
                html.Append(".arg-table tr:last-child td{border-bottom:none}");
                html.Append(
                    ".pill{display:inline-block;padding:2px 8px;border-radius:999px;background:#eef3ff;color:#3658b3;font-size:12px;margin-left:8px}"
                );
                html.Append(".muted{color:#6b7789}");
                html.Append(
                    "@media (max-width:700px){.hero{flex-direction:column;align-items:flex-start}}"
                );
                html.Append("</style></head><body>");
                html.Append("<div class=\"page\">");
                html.Append(
                    "<div class=\"hero\"><div><h1>NRobotRemote</h1><p class=\"muted\">Available keywords exposed by this server</p></div>"
                );
                html.Append(
                    "<div class=\"search\"><input id=\"filterInput\" type=\"search\" placeholder=\"Filter keywords...\" aria-label=\"Filter keywords\"><div class=\"pill\">HTTP</div></div></div>"
                );
                html.Append(
                    "<div class=\"section\"><div class=\"count\" id=\"resultCount\"></div>"
                );
                var types = _keywordManager.GetLoadedTypeNames();
                foreach (var typename in types)
                {
                    var names = _keywordManager.GetKeywordNamesForType(typename);
                    //per type table
                    html.Append("<div class=\"card\" data-type=\"" + HtmlEncode(typename) + "\">");
                    html.Append("<h3>Keywords from type: " + HtmlEncode(typename) + "</h3>");
                    html.Append("<table><thead><tr>");
                    html.Append(
                        "<th style=\"width:22%\">Keyword</th><th style=\"width:28%\">Arguments</th><th>Description</th></tr>"
                    );
                    html.Append("</thead><tbody>");
                    //add keywords
                    foreach (string name in names)
                    {
                        var keyword = _keywordManager.GetKeyword(typename, name);
                        var args = keyword.ArgumentNames ?? Array.Empty<string>();
                        var argDocs = keyword.ArgumentDocumentation ?? Array.Empty<string>();
                        var argsSummary = args.Length > 0 ? string.Join(", ", args) : "None";
                        html.Append(
                            string.Format(
                                "<tr class=\"kw-row\"><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                                HtmlEncode(name),
                                HtmlEncode(argsSummary),
                                HtmlEncode(keyword.KeywordDocumentation)
                            )
                        );
                        html.Append("<tr class=\"kw-details\"><td colspan=\"3\">");
                        if (args.Length == 0)
                        {
                            html.Append("<div class=\"muted\">No arguments.</div>");
                        }
                        else
                        {
                            html.Append(
                                "<table class=\"arg-table\"><thead><tr><th style=\"width:25%\">Argument</th><th>Description</th></tr></thead><tbody>"
                            );
                            for (int i = 0; i < args.Length; i++)
                            {
                                var doc = i < argDocs.Length ? argDocs[i] : string.Empty;
                                if (string.IsNullOrWhiteSpace(doc))
                                    doc = "No description.";
                                html.Append(
                                    "<tr><td>"
                                        + HtmlEncode(args[i])
                                        + "</td><td>"
                                        + HtmlEncode(doc)
                                        + "</td></tr>"
                                );
                            }
                            html.Append("</tbody></table>");
                        }
                        html.Append("</td></tr>");
                    }
                    html.Append("</tbody></table></div>");
                }

                //finish html
                html.Append("</div>");
                html.Append("<script>");
                html.Append(
                    "(function(){var input=document.getElementById('filterInput');var count=document.getElementById('resultCount');var cards=[].slice.call(document.querySelectorAll('.card'));var rows=[].slice.call(document.querySelectorAll('.kw-row'));function setDetailsVisible(details,isOpen){if(!details){return;}details.style.display=isOpen?'table-row':'none';}function update(){var q=(input.value||'').toLowerCase();var visible=0;cards.forEach(function(card){var any=false;card.querySelectorAll('.kw-row').forEach(function(row){var text=row.textContent.toLowerCase();var show=!q||text.indexOf(q)!==-1;var details=row.nextElementSibling;row.style.display=show?'':'none';if(details&&details.classList.contains('kw-details')){if(!show){row.classList.remove('is-open');setDetailsVisible(details,false);}else{setDetailsVisible(details,row.classList.contains('is-open'));}}if(show){any=true;visible++;}});card.style.display=any?'':'none';});count.textContent=visible + ' keyword' + (visible===1?'':'s') + ' shown';}rows.forEach(function(row){row.addEventListener('click',function(){row.classList.toggle('is-open');var details=row.nextElementSibling;if(details&&details.classList.contains('kw-details')){setDetailsVisible(details,row.classList.contains('is-open'));}});});input.addEventListener('input',update);update();})();"
                );
                html.Append("</script>");
                html.Append("</div></body></html>");
                response.StatusCode = 200;
                byte[] buffer = Encoding.UTF8.GetBytes(html.ToString());
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                response.StatusCode = 500;
            }
            response.Close();
        }

        private static string HtmlEncode(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}
