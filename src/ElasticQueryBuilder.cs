using System;
using System.Text;

namespace AJP.ElasticBand
{
    public class ElasticQueryBuilder : IElasticQueryBuilder
    {
        public string Build(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return string.Empty;

            if (searchString.StartsWith("{"))
                return searchString;

            if (searchString.Contains(":"))
            {
                return BuildFieldQuery(searchString);
            }

            return BuildSingleStringQuery(searchString);
        }

        private string BuildFieldQuery(string searchString) 
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("\"version\": true,");
            sb.AppendLine("\"size\": 500,");
            sb.AppendLine("\"query\": {");
            sb.AppendLine("      \"bool\": {   ");
            sb.AppendLine("          \"filter\": [");
            sb.AppendLine("          {");
            sb.AppendLine("          \"bool\": {");
            sb.AppendLine("            \"filter\": [");

            var queryClauses = searchString.Split( new[]{" and "}, StringSplitOptions.None);
            foreach (var clause in queryClauses) 
            {
                var parts = clause.Split(':');
                var key = parts[0];
                var value = parts[1];

                sb.AppendLine("              {");
                sb.AppendLine("                \"bool\": {");
                sb.AppendLine("                  \"should\": [");
                sb.AppendLine("                    {");
                sb.AppendLine("                      \"query_string\": {");
                sb.AppendLine($"                        \"fields\": [\"{key}\"],");
                sb.AppendLine($"                        \"query\": \"{value}\"");
                sb.AppendLine("                      }");
                sb.AppendLine("                    }");
                sb.AppendLine("                  ],");
                sb.AppendLine("                  \"minimum_should_match\": 1");
                sb.AppendLine("                }");
                sb.AppendLine("              },");            
            }

            sb.Remove(sb.Length - 3, 1); // remove trailling comma           

            sb.AppendLine("            ]");
            sb.AppendLine("          }");
            sb.AppendLine("        }");
            sb.AppendLine("      ]");
            sb.AppendLine("    }");
            sb.AppendLine("  }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string BuildSingleStringQuery(string searchString)
        {
            var sb = new StringBuilder();

            sb.AppendLine("{");
            sb.AppendLine("  \"version\": true,");
            sb.AppendLine("  \"size\": 500,");
            sb.AppendLine("  \"query\": {");
            sb.AppendLine("    \"bool\": {");
            sb.AppendLine("        \"filter\": [");
            sb.AppendLine("          {");
            sb.AppendLine("            \"multi_match\": {");
            sb.AppendLine("              \"type\": \"best_fields\",");
            sb.AppendLine($"              \"query\": \"{searchString}\",");
            sb.AppendLine("              \"lenient\": true");
            sb.AppendLine("          }");
            sb.AppendLine("        }");
            sb.AppendLine("      ]");
            sb.AppendLine("    }");
            sb.AppendLine("  }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
