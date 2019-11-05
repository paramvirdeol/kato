using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace kato.comparer.lib
{
    public class XmlComparer
    {
        string _primaryFile;
        string _secondaryFile;
        public ILog Logger { get; set; }


        public XmlComparer(string primaryFile, string secondaryFile, ILog logger)
        {
            _primaryFile = primaryFile;
            _secondaryFile = secondaryFile;
            Logger = logger;
        }

        public void CompareFiles()
        {
            var countNodeType = 0;
            var countNodeName = 0;
            var countNodeText = 0;
            var nodeTypes = new List<XmlNodeType>();
            var topology = new Stack<String>();
            string log = string.Empty;

            var primaryReader = XmlReader.Create(_primaryFile);
            var secondaryReader = XmlReader.Create(_secondaryFile);

            while (primaryReader.Read() && secondaryReader.Read())
            {
                var depth = "\t";
                if (!nodeTypes.Contains(primaryReader.NodeType))
                {
                    nodeTypes.Add(primaryReader.NodeType);
                }
                if (primaryReader.NodeType != secondaryReader.NodeType)
                {
                    countNodeType++;
                    Logger.Warn($"{depth}NODE TYPE MISMATCH: {primaryReader.NodeType} {secondaryReader.NodeType}");
                }
                else if (primaryReader.NodeType == XmlNodeType.Element)
                {
                    countNodeName++;
                    topology.Push(primaryReader.Name);
                    if (primaryReader.Name != secondaryReader.Name)
                    {
                        string xpath = getElementXPath(topology);
                        Logger.Warn($"(depth)NODE NAME MISMATCH:[{xpath}] {primaryReader.Name} {secondaryReader.Name}");
                    }
                }
                else if (primaryReader.NodeType == XmlNodeType.EndElement)
                {
                    var item = topology.Peek();
                    if (primaryReader.Name == item)
                        topology.Pop();
                }
                else if (primaryReader.NodeType == XmlNodeType.Text)
                {
                    countNodeText++;
                    if (primaryReader.Value != secondaryReader.Value)
                    {
                        var xpath = getElementXPath(topology);
                        Logger.Warn($"{depth}VALUE MISMATCH: [{xpath}] {primaryReader.Value} {secondaryReader.Value}");
                    }
                }
                else
                {
                    continue;
                }                
            }

            Logger.Info($"Node Type Count: {countNodeType}, Node Count: {countNodeName}");

        }
        private string getElementXPath(Stack<String> stack)
        {
            var builder = new StringBuilder();
            foreach (var item in stack.Reverse())
            {                
                if (builder.Length == 0)
                    builder.Append(item);
                else
                    builder.Append($"\\{item}");
            }
            return builder.ToString();
        }
    }
}
