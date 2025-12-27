using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using miniHttpServer.Intreface;

public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    public string RenderFromString(string htmlTemplate, object model)
    {
        string result = htmlTemplate;


        result = ProcessForeach(result, model);
        result = ProcessIfElse(result, model);
        result = ReplaceVariables(result, model);

        return result;
    }

    private string ProcessForeach(string template, object model)
    {
        var regex = new Regex(@"\$foreach\(var (\w+) in ([^)]+)\)([\s\S]*?)\$endfor");

        return regex.Replace(template, match =>
        {
            string varName = match.Groups[1].Value;  
            string listPath = match.Groups[2].Value;  
            string block = match.Groups[3].Value;    

            object colObj = Get(model, listPath);
            if (colObj is not IEnumerable list)
                return "";

            string result = "";

            foreach (var item in list)
            {
                string local = block;

                local = ProcessIfElse(local, item);

            
                local = ReplaceVariables(local, item, varName);

           
                
                local = ReplaceVariables(local, model);

                result += local;
            }

            return result;
        });
    }



    private string ProcessIfElse(string template, object model)
    {
        var regex = new Regex(@"\$if\(([^)]+)\)([\s\S]*?)(?:\$else([\s\S]*?))?\$endif");

        return regex.Replace(template, match =>
        {
            string expr = match.Groups[1].Value.Trim();
            string ok = match.Groups[2].Value;
            string no = match.Groups[3].Value;

            object val = Get(model, expr);
            bool cond = val is bool b && b;

            return cond ? ok : no;
        });
    }

    private string ReplaceVariables(string template, object model, string prefix = null)
    {
        return Regex.Replace(template, @"\$\{([\w\.]+)\}", match =>
        {
            string expr = match.Groups[1].Value; 

       
            if (prefix != null && expr.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase))
            {
                string subPath = expr.Substring(prefix.Length + 1);
                return Get(model, subPath)?.ToString() ?? "";
            }

         
            return Get(model, expr)?.ToString() ?? "";
        });
    }



    private object Get(object obj, string path)
    {
        if (obj == null || string.IsNullOrEmpty(path))
            return null;

        string[] parts = path.Split('.');
        object current = obj;

        foreach (var p in parts)
        {
            if (current == null)
                return null;

            var type = current.GetType();
            var prop = type.GetProperty(p, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null)
                return null;

            current = prop.GetValue(current);
        }

        return current;
    }

    public string RenderFromFile(string filePath, object dataModel)
    {
        return RenderFromString(File.ReadAllText(filePath), dataModel);
    }

    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        string result = RenderFromFile(inputFilePath, dataModel);
        File.WriteAllText(outputFilePath, result);
        return result;
    }
}
