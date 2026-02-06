using System;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace NRobot.Server.Imp.Domain
{
    /// <summary>
    /// Encapsulates a keyword as a method and documentation
    /// </summary>
    public class Keyword
    {
        public MethodInfo KeywordMethod { get; set; }
        public String KeywordDocumentation { get; set; }
        public String FriendlyName { get; set; }
        public string[] ArgumentNames { get; set; }
        public string[] ArgumentDocumentation { get; set; }
        public Object ClassInstance { get; set; }

        /// <summary>
        /// Get the number of arguments to the keyword
        /// </summary>
        public int ArgumentCount
        {
            get { return ArgumentNames.Length; }
        }

        /// <summary>
        /// Constructor from method
        /// </summary>
        public Keyword(Object classinstance, MethodInfo method, XDocument documentation)
        {
            if (classinstance == null)
                throw new Exception("No class instance specified");
            if (method == null)
                throw new Exception("No keyword method specified");
            //record properties
            KeywordMethod = method;
            KeywordDocumentation = String.Empty;
            FriendlyName = ToSnakeCase(KeywordMethod.Name);
            ClassInstance = classinstance;
            //get argument names
            ParameterInfo[] pis = KeywordMethod.GetParameters();
            ArgumentNames = new String[pis.Length];
            ArgumentDocumentation = new String[pis.Length];
            int i = 0;
            foreach (ParameterInfo pi in pis)
            {
                ArgumentNames[i++] = pi.Name;
            }
            //get xml documentation
            if (documentation != null)
            {
                KeywordDocumentation = method.GetXmlDocumentation(documentation);
                for (int j = 0; j < pis.Length; j++)
                {
                    ArgumentDocumentation[j] = pis[j].GetXmlDocumentation(documentation);
                }
            }
        }

        private static string ToSnakeCase(string name)
        {
            if (String.IsNullOrEmpty(name))
                return String.Empty;

            var builder = new StringBuilder(name.Length + 4);
            for (var i = 0; i < name.Length; i++)
            {
                var ch = name[i];
                if (ch == '_')
                {
                    builder.Append('_');
                    continue;
                }

                if (Char.IsDigit(ch))
                {
                    if (i > 0)
                    {
                        var prev = name[i - 1];
                        if (prev != '_' && !Char.IsDigit(prev))
                        {
                            builder.Append('_');
                        }
                    }
                    builder.Append(ch);
                    continue;
                }

                if (Char.IsUpper(ch))
                {
                    if (i > 0)
                    {
                        var prev = name[i - 1];
                        var next = i + 1 < name.Length ? name[i + 1] : '\0';
                        if (
                            prev != '_'
                            && (Char.IsLower(prev) || Char.IsDigit(prev) || Char.IsLower(next))
                        )
                        {
                            builder.Append('_');
                        }
                    }
                    builder.Append(Char.ToLowerInvariant(ch));
                    continue;
                }

                builder.Append(Char.ToLowerInvariant(ch));
            }

            return builder.ToString();
        }
    }
}
