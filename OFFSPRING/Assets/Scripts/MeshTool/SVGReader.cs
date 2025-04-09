using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SVGReader
{
    private TextAsset svgFile;
    private float width;
    private float height;
    public float extrudeAmount = 0f;
    private Dictionary<string, Dictionary<string, string>> styles;

    public SVGReader(TextAsset svgFile)
    {
        this.svgFile = svgFile;
    }

    public void export(string name)
    {
        NameTable nameTable = new NameTable();
        XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(nameTable);
        nameSpaceManager.AddNamespace("svg", "http://www.w3.org/2000/svg");

        XmlParserContext parserContext = new XmlParserContext(null, nameSpaceManager, null, XmlSpace.None);
        XmlTextReader txtReader = new XmlTextReader(svgFile.text, XmlNodeType.Document, parserContext);

        XmlDocument document = new XmlDocument();
        document.Load(txtReader);

        SVG svg = new SVG();
        XmlNode svgNode = document.SelectSingleNode("svg:svg", nameSpaceManager);

        // Parse viewBox or width/height
        if (svgNode.Attributes["viewBox"] != null)
        {
            string[] viewBoxValues = svgNode.Attributes["viewBox"].Value.Split(' ');
            width = float.Parse(viewBoxValues[2]);
            height = float.Parse(viewBoxValues[3]);
        }
        else
        {
            width = float.Parse(svgNode.Attributes["width"].Value);
            height = float.Parse(svgNode.Attributes["height"].Value);
        }

        // Parse styles
        styles = new Dictionary<string, Dictionary<string, string>>();
        XmlNode styleNode = document.SelectSingleNode("svg:svg/svg:defs/svg:style", nameSpaceManager);
        if (styleNode != null)
        {
            styles = ParseStyles(styleNode.InnerText);
        }

        // Process all elements
        SVGGroup rootGroup = new SVGGroup();
        ProcessChildren(svgNode, rootGroup, nameSpaceManager);
        svg.groupList.Add(rootGroup);

        // Create GameObject hierarchy
        GameObject parentObj = new GameObject(svgFile.name);

        int count = 0;
        foreach (SVGGroup group in svg.groupList)
        {
            foreach (SVGPath path in group.pathList)
            {
                Mesh mesh = CreateMesh(path.vertexList, path.color);

                GameObject pathObj = new GameObject(path.id);
                pathObj.transform.SetParent(parentObj.transform);
                pathObj.transform.localPosition = new Vector3(0, 0, -(count) * 0.025f);

                MeshFilter meshFilter = pathObj.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = pathObj.AddComponent<MeshRenderer>();

                meshFilter.mesh = mesh;

                // Create material with proper shader
                Material material = new Material(Shader.Find("Standard"));
                material.color = path.color;
                meshRenderer.material = material;

                count++;
            }
        }
    }

    private Dictionary<string, Dictionary<string, string>> ParseStyles(string styleText)
    {
        Dictionary<string, Dictionary<string, string>> styles = new Dictionary<string, Dictionary<string, string>>();

        // Normalize style text
        styleText = System.Text.RegularExpressions.Regex.Replace(styleText, @"/\*.*?\*/", string.Empty);
        styleText = styleText.Replace("\n", "").Replace("\r", "");

        var matches = System.Text.RegularExpressions.Regex.Matches(styleText, @"\.([^{]+)\{([^}]+)\}");

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            string className = match.Groups[1].Value.Trim();
            Dictionary<string, string> properties = new Dictionary<string, string>();

            foreach (string prop in match.Groups[2].Value.Split(';'))
            {
                if (!string.IsNullOrEmpty(prop))
                {
                    string[] keyValue = prop.Split(':');
                    if (keyValue.Length == 2)
                    {
                        properties[keyValue[0].Trim()] = keyValue[1].Trim();
                    }
                }
            }

            styles[className] = properties;
        }

        return styles;
    }

    private void ProcessChildren(XmlNode parentNode, SVGGroup group, XmlNamespaceManager nsManager)
    {
        foreach (XmlNode childNode in parentNode.ChildNodes)
        {
            ProcessNode(childNode, group, nsManager);
        }
    }

    private void ProcessNode(XmlNode node, SVGGroup group, XmlNamespaceManager nsManager)
    {
        if (node.NodeType != XmlNodeType.Element) return;

        switch (node.Name)
        {
            case "path":
                group.pathList.Add(CreatePathFromNode(node));
                break;
            case "circle":
                group.pathList.Add(CreatePathFromCircle(node));
                break;
            case "rect":
                group.pathList.Add(CreatePathFromRect(node));
                break;
            case "ellipse":
                group.pathList.Add(CreatePathFromEllipse(node));
                break;
            case "polygon":
                group.pathList.Add(CreatePathFromPolygon(node));
                break;
            case "polyline":
                group.pathList.Add(CreatePathFromPolyline(node));
                break;
            case "g":
                // Recursively handle nested groups
                SVGGroup nestedGroup = new SVGGroup();
                foreach (XmlNode child in node.ChildNodes)
                {
                    ProcessNode(child, nestedGroup, nsManager);
                }
                // You can choose to store nested groups if needed
                group.pathList.AddRange(nestedGroup.pathList);
                break;
            default:
                break;
        }
    }

    private void ProcessGroup(XmlNode groupNode, SVG svg, XmlNamespaceManager nsManager)
    {
        SVGGroup group = new SVGGroup();
        foreach (XmlNode childNode in groupNode.ChildNodes)
        {
            ProcessNode(childNode, group, nsManager); 
        }
        svg.groupList.Add(group);
    }

    private void ProcessPath(XmlNode pathNode, SVG svg)
    {
        if (svg.groupList.Count == 0) svg.groupList.Add(new SVGGroup());
        svg.groupList.Last().pathList.Add(CreatePathFromNode(pathNode));
    }

    private void ProcessCircle(XmlNode circleNode, SVG svg)
    {
        if (svg.groupList.Count == 0) svg.groupList.Add(new SVGGroup());
        svg.groupList.Last().pathList.Add(CreatePathFromCircle(circleNode));
    }

    private void ProcessRect(XmlNode rectNode, SVG svg)
    {
        if (svg.groupList.Count == 0) svg.groupList.Add(new SVGGroup());
        svg.groupList.Last().pathList.Add(CreatePathFromRect(rectNode));
    }

    private void ProcessEllipse(XmlNode ellipseNode, SVG svg)
    {
        if (svg.groupList.Count == 0) svg.groupList.Add(new SVGGroup());
        svg.groupList.Last().pathList.Add(CreatePathFromEllipse(ellipseNode));
    }

    private void ProcessPolygon(XmlNode polygonNode, SVG svg)
    {
        if (svg.groupList.Count == 0) svg.groupList.Add(new SVGGroup());
        svg.groupList.Last().pathList.Add(CreatePathFromPolygon(polygonNode));
    }

    private void ProcessPolyline(XmlNode polylineNode, SVG svg)
    {
        if (svg.groupList.Count == 0) svg.groupList.Add(new SVGGroup());
        svg.groupList.Last().pathList.Add(CreatePathFromPolyline(polylineNode));
    }

    private SVGPath CreatePathFromNode(XmlNode node)
    {
        SVGPath path = new SVGPath();
        path.id = node.Attributes["id"]?.Value ?? "path";

        if (node.Attributes["d"] != null)
        {
            path.vertexList = ParsePathVertexList(node.Attributes["d"].Value);
        }

        string style = GetStyleFromNode(node);
        path.color = ParsePathColor(style);

        return path;
    }

    private SVGPath CreatePathFromCircle(XmlNode circleNode)
    {
        float cx = float.Parse(circleNode.Attributes["cx"].Value);
        float cy = float.Parse(circleNode.Attributes["cy"].Value);
        float r = float.Parse(circleNode.Attributes["r"].Value);

        // Create circle path data
        string pathData = $"M {cx - r},{cy} " +
                        $"a {r},{r} 0 1,0 {r * 2},0 " +
                        $"a {r},{r} 0 1,0 -{r * 2},0 Z";

        XmlDocument tempDoc = new XmlDocument();
        XmlElement tempNode = tempDoc.CreateElement("path");
        tempNode.SetAttribute("d", pathData);
        CopyAttributes(circleNode, tempNode);

        return CreatePathFromNode(tempNode);
    }

    private SVGPath CreatePathFromRect(XmlNode rectNode)
    {
        float x = float.Parse(rectNode.Attributes["x"]?.Value ?? "0");
        float y = float.Parse(rectNode.Attributes["y"]?.Value ?? "0");
        float width = float.Parse(rectNode.Attributes["width"].Value);
        float height = float.Parse(rectNode.Attributes["height"].Value);
        float rx = float.Parse(rectNode.Attributes["rx"]?.Value ?? "0");
        float ry = float.Parse(rectNode.Attributes["ry"]?.Value ?? "0");

        string pathData;
        if (rx == 0 && ry == 0)
        {
            pathData = $"M {x},{y} h {width} v {height} h {-width} Z";
        }
        else
        {
            pathData = $"M {x + rx},{y} " +
                      $"h {width - 2 * rx} " +
                      $"a {rx},{ry} 0 0 1 {rx},{ry} " +
                      $"v {height - 2 * ry} " +
                      $"a {rx},{ry} 0 0 1 {-rx},{ry} " +
                      $"h {-width + 2 * rx} " +
                      $"a {rx},{ry} 0 0 1 {-rx},{-ry} " +
                      $"v {-height + 2 * ry} " +
                      $"a {rx},{ry} 0 0 1 {rx},{-ry} Z";
        }

        XmlDocument tempDoc = new XmlDocument();
        XmlElement tempNode = tempDoc.CreateElement("path");
        tempNode.SetAttribute("d", pathData);
        CopyAttributes(rectNode, tempNode);

        return CreatePathFromNode(tempNode);
    }

    private SVGPath CreatePathFromEllipse(XmlNode ellipseNode)
    {
        float cx = float.Parse(ellipseNode.Attributes["cx"].Value);
        float cy = float.Parse(ellipseNode.Attributes["cy"].Value);
        float rx = float.Parse(ellipseNode.Attributes["rx"].Value);
        float ry = float.Parse(ellipseNode.Attributes["ry"].Value);

        // Approximation of ellipse with bezier curves
        string pathData = $"M {cx - rx},{cy} " +
                         $"a {rx},{ry} 0 1,0 {rx * 2},0 " +
                         $"a {rx},{ry} 0 1,0 -{rx * 2},0 Z";

        XmlDocument tempDoc = new XmlDocument();
        XmlElement tempNode = tempDoc.CreateElement("path");
        tempNode.SetAttribute("d", pathData);
        CopyAttributes(ellipseNode, tempNode);

        return CreatePathFromNode(tempNode);
    }

    private SVGPath CreatePathFromPolygon(XmlNode polygonNode)
    {
        string points = polygonNode.Attributes["points"].Value;
        string pathData = "M " + points + " Z";

        XmlDocument tempDoc = new XmlDocument();
        XmlElement tempNode = tempDoc.CreateElement("path");
        tempNode.SetAttribute("d", pathData);
        CopyAttributes(polygonNode, tempNode);

        return CreatePathFromNode(tempNode);
    }

    private SVGPath CreatePathFromPolyline(XmlNode polylineNode)
    {
        string points = polylineNode.Attributes["points"].Value;
        string pathData = "M " + points;

        XmlDocument tempDoc = new XmlDocument();
        XmlElement tempNode = tempDoc.CreateElement("path");
        tempNode.SetAttribute("d", pathData);
        CopyAttributes(polylineNode, tempNode);

        return CreatePathFromNode(tempNode);
    }

    private string GetStyleFromNode(XmlNode node)
    {
        // Check inline style first
        if (node.Attributes["style"] != null)
        {
            return node.Attributes["style"].Value;
        }

        // Check class
        if (node.Attributes["class"] != null)
        {
            string className = node.Attributes["class"].Value;
            if (styles.TryGetValue(className, out var classStyle))
            {
                return string.Join(";", classStyle.Select(kv => $"{kv.Key}:{kv.Value}"));
            }
        }

        // Default style
        return "fill:#000000";
    }

    private void CopyAttributes(XmlNode sourceNode, XmlElement targetNode)
    {
        if (sourceNode.Attributes["id"] != null)
            targetNode.SetAttribute("id", sourceNode.Attributes["id"].Value);
        if (sourceNode.Attributes["style"] != null)
            targetNode.SetAttribute("style", sourceNode.Attributes["style"].Value);
        if (sourceNode.Attributes["class"] != null)
            targetNode.SetAttribute("class", sourceNode.Attributes["class"].Value);
    }

    private List<Vector2> ParsePathVertexList(string path)
    {
        List<Vector2> vertexList = new List<Vector2>();
        Vector2 lastVector = new Vector2(0, height);
        bool isAbsolute = false;

        string[] tokens = path.Split(new[] { ' ', ',', '\t', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            switch (token)
            {
                case "M":
                case "L":
                case "T":
                    isAbsolute = true;
                    break;
                case "m":
                case "l":
                case "t":
                    isAbsolute = false;
                    break;
                case "Z":
                case "z":
                    // Close path
                    break;
                default:
                    if (float.TryParse(token, out float x) && i + 1 < tokens.Length && float.TryParse(tokens[i + 1], out float y))
                    {
                        Vector2 vertex;
                        if (isAbsolute)
                        {
                            vertex = new Vector2(x, height - y);
                        }
                        else
                        {
                            vertex = new Vector2(lastVector.x + x, lastVector.y - y);
                        }
                        vertexList.Add(vertex);
                        lastVector = vertex;
                        i++; // Skip y coordinate
                    }
                    break;
            }
        }

        return vertexList;
    }

    private Color ParsePathColor(string style)
    {
        Color color = Color.black;
        float opacity = 1f;

        string[] properties = style.Split(';');
        foreach (string prop in properties)
        {
            if (string.IsNullOrEmpty(prop)) continue;

            string[] parts = prop.Split(':');
            if (parts.Length != 2) continue;

            string name = parts[0].Trim();
            string value = parts[1].Trim();

            switch (name)
            {
                case "fill":
                    if (value != "none") color = HexToColor(value);
                    break;
                case "fill-opacity":
                    opacity = float.Parse(value);
                    break;
                case "stroke":
                    // Could implement stroke handling here
                    break;
            }
        }

        color.a = opacity;
        return color;
    }

    private Mesh CreateMesh(List<Vector2> vertexList, Color color)
    {
        if (vertexList == null || vertexList.Count < 3)
        {
            Debug.LogWarning("Not enough vertices to create mesh");
            return new Mesh();
        }

        Vector3[] vertices = new Vector3[vertexList.Count];
        Vector2[] uvs = new Vector2[vertexList.Count];
        Color[] colors = new Color[vertexList.Count];

        Triangulator triangulator = new Triangulator(width, height, vertexList);
        int[] triangles = triangulator.Triangulate();

        for (int i = 0; i < vertexList.Count; i++)
        {
            vertices[i] = new Vector3(vertexList[i].x, vertexList[i].y, 0);
            uvs[i] = new Vector2(vertexList[i].x / width, vertexList[i].y / height);
            colors[i] = color;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Color HexToColor(string hex)
    {
        if (hex == "none") return new Color(0, 0, 0, 0);

        hex = hex.TrimStart('#');
        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, 255);
    }
}

// Helper classes
public class SVG
{
    public List<SVGGroup> groupList = new List<SVGGroup>();
}

public class SVGGroup
{
    public List<SVGPath> pathList = new List<SVGPath>();
}

public class SVGPath
{
    public string id;
    public List<Vector2> vertexList = new List<Vector2>();
    public Color color;
}