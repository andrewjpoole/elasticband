using System;
using System.Text;

namespace AJP.ElasticBand
{
    public class ElasticQueryBuilder : IElasticQueryBuilder
    {
        public string Build(string searchString, int limit = 500)
        {
            if (string.IsNullOrEmpty(searchString))
                return string.Empty;

            if (searchString.StartsWith("{"))
                return searchString;

            if (searchString.Contains("<") || searchString.Contains(">"))            
                return BuildRangeQuery(searchString, limit);            

            if (searchString.Contains(":"))            
                return BuildFieldQuery(searchString, limit);

            if (searchString.Contains("*"))
                return BuildWildcardQuery(searchString, limit);

            return BuildSingleStringQuery(searchString, limit);
        }

        private string BuildFieldQuery(string searchString, int limit) 
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("\"version\": true,");
            sb.AppendLine($"\"size\": {limit},");
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
                var key = parts[0].Trim();
                var value = parts[1].Trim();

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

        private string BuildSingleStringQuery(string searchString, int limit)
        {
            var sb = new StringBuilder();

            sb.AppendLine("{");
            sb.AppendLine("  \"version\": true,");
            sb.AppendLine($"  \"size\": {limit},");
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

        private string BuildRangeQuery(string searchString, int limit) 
        {
            var parts = searchString.Split(new[] { '<', '>' });
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            var queryOperator = searchString.Contains("<") ? "lte" : "gte";

            var sb = new StringBuilder();

            sb.AppendLine("{");
            sb.AppendLine("  \"version\": true,");
            sb.AppendLine($"  \"size\": {limit},");
            sb.AppendLine("  \"query\": {");
            sb.AppendLine("    \"bool\": {");
            sb.AppendLine("      \"must\": [");
            sb.AppendLine("        {");
            sb.AppendLine("          \"range\": {");
            sb.AppendLine($"            \"{key}\": {{");
            //sb.AppendLine("              \"format\": \"strict_date_optional_time\","); // dont think this is needed even for dates
            sb.AppendLine($"              \"{queryOperator}\": \"{value}\"");
            sb.AppendLine("            }");
            sb.AppendLine("          }");
            sb.AppendLine("        }");
            sb.AppendLine("      ]");
            sb.AppendLine("    }");
            sb.AppendLine("  }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string BuildWildcardQuery(string searchString, int limit)
        {          
            var sb = new StringBuilder();

            sb.AppendLine("{");
            sb.AppendLine("  \"version\": true,");
            sb.AppendLine($"  \"size\": {limit},");
            sb.AppendLine("  \"query\": {");
            sb.AppendLine("    \"bool\": {");
            sb.AppendLine("      \"must\": [");
            sb.AppendLine("        {");
            sb.AppendLine("          \"query_string\": {");
            sb.AppendLine($"            \"query\": \"{searchString}\",");
            sb.AppendLine("            \"analyze_wildcard\": true");
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
