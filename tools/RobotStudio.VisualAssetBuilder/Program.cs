using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

if (args.Length is < 1 or > 2)
{
    Console.Error.WriteLine(
        "Usage: dotnet run --project tools/RobotStudio.VisualAssetBuilder -- [cartesian|xy-plotter] <output.glb>");
    return 1;
}

var modelId = args.Length == 1 ? "cartesian" : args[0];
var outputPath = Path.GetFullPath(args[^1]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
var asset = modelId switch
{
    "cartesian" => CartesianAsset.Create(),
    "xy-plotter" => XYPlotterAsset.Create(),
    _ => throw new ArgumentException($"Unknown visual asset model '{modelId}'.", nameof(args))
};
asset.Write(outputPath);
Console.WriteLine($"Created {outputPath}");
return 0;

internal static class CartesianAsset
{
    public static GlbBuilder Create()
    {
        var asset = new GlbBuilder("Cartesian Mechanical Showcase");
        var charcoal = asset.Material("Charcoal polymer", 0.045f, 0.055f, 0.075f, 0.55f, 0.34f);
        var frame = asset.Material("Anodized aluminum", 0.34f, 0.38f, 0.44f, 0.82f, 0.26f);
        var steel = asset.Material("Machined steel", 0.62f, 0.67f, 0.73f, 0.9f, 0.2f);
        var blue = asset.Material("RobotStudio blue", 0.035f, 0.28f, 0.72f, 0.48f, 0.3f);
        var bed = asset.Material("Textured work surface", 0.11f, 0.13f, 0.16f, 0.18f, 0.72f);
        var rubber = asset.Material("Drive belt rubber", 0.018f, 0.022f, 0.028f, 0.05f, 0.9f);
        var copper = asset.Material("Copper transmission", 0.72f, 0.28f, 0.06f, 0.74f, 0.28f);
        var tool = asset.Material("Process tool", 0.92f, 0.3f, 0.035f, 0.5f, 0.24f);
        var display = asset.Material("Controller display", 0.02f, 0.55f, 0.82f, 0.2f, 0.2f, emissive: new(0.02f, 0.28f, 0.5f));

        asset.Part("base");
        asset.Box("base", new(0, 0, 0.25f), new(9.5f, 8, 0.5f), charcoal);
        asset.Box("base", new(-4.25f, -3.5f, 0.02f), new(0.8f, 0.8f, 0.25f), rubber);
        asset.Box("base", new(4.25f, -3.5f, 0.02f), new(0.8f, 0.8f, 0.25f), rubber);
        asset.Box("base", new(-4.25f, 3.5f, 0.02f), new(0.8f, 0.8f, 0.25f), rubber);
        asset.Box("base", new(4.25f, 3.5f, 0.02f), new(0.8f, 0.8f, 0.25f), rubber);

        asset.Part("controller", "base");
        asset.Box("controller", new(3.55f, -3.2f, 1.02f), new(1.75f, 1.2f, 1.45f), blue);
        asset.Box("controller", new(3.55f, -3.82f, 1.08f), new(1.15f, 0.04f, 0.55f), display);
        asset.Cylinder("controller", new(4.15f, -3.86f, 0.72f), new(4.15f, -3.98f, 0.72f), 0.16f, tool);

        asset.FramePart("left-frame-column", "base", new(-4.05f, 2.65f, 4.35f), new(0.52f, 0.62f, 7.2f), frame);
        asset.FramePart("right-frame-column", "base", new(4.05f, 2.65f, 4.35f), new(0.52f, 0.62f, 7.2f), frame);
        asset.FramePart("top-frame-beam", "base", new(0, 2.65f, 7.95f), new(8.6f, 0.62f, 0.52f), frame);
        asset.FramePart("left-y-rail", "base", new(-2.45f, -0.45f, 0.82f), new(0.2f, 5.8f, 0.18f), steel);
        asset.FramePart("right-y-rail", "base", new(2.45f, -0.45f, 0.82f), new(0.2f, 5.8f, 0.18f), steel);

        asset.Part("y-motor", "base");
        asset.Box("y-motor", new(0, -3.48f, 0.82f), new(0.85f, 0.82f, 0.85f), charcoal);
        asset.Cylinder("y-motor", new(0, -3.98f, 0.82f), new(0, -3.82f, 0.82f), 0.17f, steel);
        asset.Part("y-belt", "base");
        asset.Box("y-belt", new(0, -0.45f, 0.88f), new(0.13f, 5.65f, 0.09f), rubber);

        asset.Part("y-bed-carriage", "base");
        asset.Box("y-bed-carriage", new(0, -0.8f, 1.02f), new(6.7f, 5.5f, 0.32f), frame);
        asset.Box("y-bed-carriage", new(-2.45f, -0.8f, 0.86f), new(0.65f, 0.9f, 0.24f), blue);
        asset.Box("y-bed-carriage", new(2.45f, -0.8f, 0.86f), new(0.65f, 0.9f, 0.24f), blue);
        asset.Part("build-plate", "y-bed-carriage");
        asset.Box("build-plate", new(0, -0.8f, 1.28f), new(6.3f, 5.1f, 0.14f), bed);
        asset.Box("build-plate", new(0, -3.25f, 1.38f), new(1.4f, 0.18f, 0.18f), steel);

        asset.Guide("left-z-guide", -3.7f, 2.4f, steel);
        asset.Guide("right-z-guide", 3.7f, 2.4f, steel);
        asset.Screw("left-z-screw", -3.45f, 2.8f, copper);
        asset.Screw("right-z-screw", 3.45f, 2.8f, copper);
        asset.Motor("left-z-motor", new(-3.45f, 2.8f, 0.72f), charcoal, steel);
        asset.Motor("right-z-motor", new(3.45f, 2.8f, 0.72f), charcoal, steel);

        asset.Part("z-gantry", "base");
        asset.Box("z-gantry", new(0, 2.5f, 5.4f), new(8.2f, 0.68f, 0.68f), blue);
        asset.Box("z-gantry", new(-3.7f, 2.5f, 5.4f), new(0.78f, 0.98f, 0.98f), frame);
        asset.Box("z-gantry", new(3.7f, 2.5f, 5.4f), new(0.78f, 0.98f, 0.98f), frame);
        asset.FramePart("x-rail", "z-gantry", new(0, 2.12f, 5.4f), new(7.25f, 0.17f, 0.2f), steel);
        asset.Part("x-belt", "z-gantry");
        asset.Box("x-belt", new(0, 1.98f, 5.65f), new(7.05f, 0.1f, 0.1f), rubber);
        asset.Part("x-motor", "z-gantry");
        asset.Box("x-motor", new(-4.12f, 2.5f, 5.4f), new(0.82f, 0.82f, 0.82f), charcoal);
        asset.Cylinder("x-motor", new(-4.58f, 2.5f, 5.4f), new(-4.42f, 2.5f, 5.4f), 0.17f, steel);

        asset.Part("x-tool-carriage", "z-gantry");
        asset.Box("x-tool-carriage", new(-1.6f, 1.9f, 5.35f), new(0.95f, 0.8f, 1.1f), blue);
        asset.Cylinder("x-tool-carriage", new(-1.6f, 1.48f, 5.35f), new(-1.6f, 1.25f, 5.35f), 0.32f, charcoal);
        asset.Part("tool", "x-tool-carriage");
        asset.Box("tool", new(-1.6f, 1.58f, 4.72f), new(0.64f, 0.64f, 0.52f), charcoal);
        asset.Cylinder("tool", new(-1.6f, 1.58f, 4.42f), new(-1.6f, 1.58f, 3.92f), 0.15f, tool);
        asset.Cylinder("tool", new(-1.6f, 1.58f, 3.88f), new(-1.6f, 1.58f, 3.7f), 0.08f, copper);

        return asset;
    }
}

internal static class XYPlotterAsset
{
    public static GlbBuilder Create()
    {
        var asset = new GlbBuilder("XY Plotter Mechanical Showcase");
        var charcoal = asset.Material("Charcoal polymer", 0.045f, 0.055f, 0.075f, 0.5f, 0.36f);
        var frame = asset.Material("Anodized aluminum", 0.34f, 0.38f, 0.44f, 0.82f, 0.26f);
        var steel = asset.Material("Machined steel", 0.62f, 0.67f, 0.73f, 0.9f, 0.2f);
        var blue = asset.Material("RobotStudio blue", 0.035f, 0.28f, 0.72f, 0.48f, 0.3f);
        var paper = asset.Material("Drawing paper", 0.88f, 0.86f, 0.76f, 0.02f, 0.86f);
        var rubber = asset.Material("Drive belt rubber", 0.018f, 0.022f, 0.028f, 0.05f, 0.9f);
        var penBody = asset.Material("Plotter pen", 0.82f, 0.12f, 0.08f, 0.16f, 0.42f);
        var display = asset.Material(
            "Controller display",
            0.02f,
            0.55f,
            0.82f,
            0.2f,
            0.2f,
            emissive: new(0.02f, 0.28f, 0.5f));

        asset.Part("base");
        asset.Box("base", new(0, 0, 0.25f), new(10, 8, 0.5f), charcoal);
        asset.Box("base", new(-4.5f, -3.5f, 0.02f), new(0.65f, 0.65f, 0.22f), rubber);
        asset.Box("base", new(4.5f, -3.5f, 0.02f), new(0.65f, 0.65f, 0.22f), rubber);
        asset.Box("base", new(-4.5f, 3.5f, 0.02f), new(0.65f, 0.65f, 0.22f), rubber);
        asset.Box("base", new(4.5f, 3.5f, 0.02f), new(0.65f, 0.65f, 0.22f), rubber);

        asset.Part("controller", "base");
        asset.Box("controller", new(3.9f, -3.25f, 0.85f), new(1.65f, 1.15f, 1.1f), blue);
        asset.Box("controller", new(3.9f, -3.84f, 0.9f), new(1.05f, 0.04f, 0.42f), display);
        asset.Cylinder("controller", new(4.47f, -3.88f, 0.62f), new(4.47f, -4f, 0.62f), 0.13f, penBody);

        asset.Part("paper-bed", "base");
        asset.Box("paper-bed", new(0, -0.25f, 0.72f), new(7.4f, 5.7f, 0.18f), frame);
        asset.Box("paper-bed", new(0, -0.25f, 0.84f), new(6.8f, 5.1f, 0.05f), paper);
        asset.Box("paper-bed", new(-3.25f, -0.25f, 0.91f), new(0.12f, 4.5f, 0.1f), steel);
        asset.Box("paper-bed", new(3.25f, -0.25f, 0.91f), new(0.12f, 4.5f, 0.1f), steel);

        asset.FramePart("left-y-rail", "base", new(-4.1f, -0.15f, 1.05f), new(0.28f, 6.5f, 0.28f), steel);
        asset.FramePart("right-y-rail", "base", new(4.1f, -0.15f, 1.05f), new(0.28f, 6.5f, 0.28f), steel);
        asset.Part("y-motor", "base");
        asset.Box("y-motor", new(-4.1f, -3.45f, 1.05f), new(0.82f, 0.82f, 0.82f), charcoal);
        asset.Cylinder("y-motor", new(-4.1f, -3.95f, 1.05f), new(-4.1f, -3.78f, 1.05f), 0.16f, steel);
        asset.Part("left-y-belt", "base");
        asset.Box("left-y-belt", new(-3.75f, -0.15f, 1.12f), new(0.1f, 6.3f, 0.1f), rubber);
        asset.Part("right-y-belt", "base");
        asset.Box("right-y-belt", new(3.75f, -0.15f, 1.12f), new(0.1f, 6.3f, 0.1f), rubber);

        asset.Part("y-gantry", "base");
        asset.Box("y-gantry", new(0, -1.5f, 2.15f), new(8.8f, 0.72f, 0.72f), blue);
        asset.Box("y-gantry", new(-4.05f, -1.5f, 1.55f), new(0.75f, 1.05f, 1.4f), frame);
        asset.Box("y-gantry", new(4.05f, -1.5f, 1.55f), new(0.75f, 1.05f, 1.4f), frame);
        asset.FramePart("x-rail", "y-gantry", new(0, -1.86f, 2.15f), new(7.6f, 0.2f, 0.22f), steel);
        asset.Part("x-belt", "y-gantry");
        asset.Box("x-belt", new(0, -1.98f, 2.42f), new(7.5f, 0.1f, 0.1f), rubber);
        asset.Part("x-motor", "y-gantry");
        asset.Box("x-motor", new(-4.35f, -1.5f, 2.15f), new(0.82f, 0.82f, 0.82f), charcoal);
        asset.Cylinder("x-motor", new(-4.82f, -1.5f, 2.15f), new(-4.65f, -1.5f, 2.15f), 0.16f, steel);

        asset.Part("x-carriage", "y-gantry");
        asset.Box("x-carriage", new(-1.8f, -1.9f, 2.05f), new(0.95f, 0.8f, 1.05f), blue);
        asset.Cylinder("x-carriage", new(-1.8f, -2.34f, 2.05f), new(-1.8f, -2.13f, 2.05f), 0.3f, charcoal);
        asset.Part("pen-lift", "x-carriage");
        asset.Box("pen-lift", new(-1.8f, -2.15f, 1.45f), new(0.62f, 0.62f, 0.55f), charcoal);
        asset.Cylinder("pen-lift", new(-1.8f, -2.15f, 1.7f), new(-1.8f, -2.15f, 1.96f), 0.15f, steel);
        asset.Part("pen", "pen-lift");
        asset.Cylinder("pen", new(-1.8f, -2.15f, 1.35f), new(-1.8f, -2.15f, 0.84f), 0.11f, penBody);
        asset.Cylinder("pen", new(-1.8f, -2.15f, 0.84f), new(-1.8f, -2.15f, 0.72f), 0.055f, charcoal);

        return asset;
    }
}

internal sealed class GlbBuilder
{
    private readonly string sceneName;
    private readonly List<NodeData> nodes = [];
    private readonly List<JsonObject> materials = [];
    private readonly List<JsonObject> meshes = [];
    private readonly List<JsonObject> bufferViews = [];
    private readonly List<JsonObject> accessors = [];
    private readonly Dictionary<string, int> parts = new(StringComparer.Ordinal);
    private readonly Dictionary<(Shape Shape, int Material), int> meshCache = [];
    private readonly Dictionary<Shape, GeometryAccessors> geometryCache = [];
    private readonly MemoryStream binary = new();

    public GlbBuilder(string sceneName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sceneName);
        this.sceneName = sceneName;
    }

    public int Material(
        string name,
        float red,
        float green,
        float blue,
        float metallic,
        float roughness,
        Vector3? emissive = null)
    {
        var material = new JsonObject
        {
            ["name"] = name,
            ["pbrMetallicRoughness"] = new JsonObject
            {
                ["baseColorFactor"] = Floats(red, green, blue, 1),
                ["metallicFactor"] = metallic,
                ["roughnessFactor"] = roughness
            }
        };
        if (emissive is Vector3 color)
        {
            material["emissiveFactor"] = Floats(color.X, color.Y, color.Z);
        }

        materials.Add(material);
        return materials.Count - 1;
    }

    public void Part(string id, string? parentId = null)
    {
        if (parts.ContainsKey(id))
        {
            return;
        }

        var nodeIndex = AddNode(new NodeData($"RS_{id}"));
        parts.Add(id, nodeIndex);
        if (parentId is not null)
        {
            nodes[parts[parentId]].Children.Add(nodeIndex);
        }
    }

    public void FramePart(string id, string parentId, Vector3 center, Vector3 size, int material)
    {
        Part(id, parentId);
        Box(id, center, size, material);
    }

    public void Guide(string id, float x, float y, int material)
    {
        Part(id, "base");
        Cylinder(id, new(x, y, 1.05f), new(x, y, 7.55f), 0.11f, material);
    }

    public void Screw(string id, float x, float y, int material)
    {
        Part(id, "base");
        Cylinder(id, new(x, y, 1.08f), new(x, y, 7.55f), 0.085f, material);
    }

    public void Motor(string id, Vector3 center, int bodyMaterial, int shaftMaterial)
    {
        Part(id, "base");
        Box(id, center, new(0.82f, 0.82f, 0.78f), bodyMaterial);
        Cylinder(id, center + new Vector3(0, 0, 0.35f), center + new Vector3(0, 0, 0.66f), 0.16f, shaftMaterial);
    }

    public void Box(string partId, Vector3 center, Vector3 size, int material) =>
        AddPrimitive(partId, Shape.Box, center, size, Quaternion.Identity, material);

    public void Cylinder(string partId, Vector3 start, Vector3 end, float radius, int material)
    {
        var direction = end - start;
        AddPrimitive(
            partId,
            Shape.Cylinder,
            (start + end) / 2,
            new(radius, radius, direction.Length()),
            RotationFromZ(direction),
            material);
    }

    public void Write(string path)
    {
        PadBinary();
        var root = new JsonObject
        {
            ["asset"] = new JsonObject
            {
                ["version"] = "2.0",
                ["generator"] = "RobotStudio.VisualAssetBuilder"
            },
            ["scene"] = 0,
            ["scenes"] = new JsonArray(new JsonObject
            {
                ["name"] = sceneName,
                ["nodes"] = Ints(parts["base"])
            }),
            ["nodes"] = new JsonArray(nodes.Select(node => node.ToJson()).ToArray()),
            ["meshes"] = new JsonArray(meshes.ToArray()),
            ["materials"] = new JsonArray(materials.ToArray()),
            ["bufferViews"] = new JsonArray(bufferViews.ToArray()),
            ["accessors"] = new JsonArray(accessors.ToArray()),
            ["buffers"] = new JsonArray(new JsonObject { ["byteLength"] = binary.Length })
        };
        var json = Encoding.UTF8.GetBytes(root.ToJsonString(new JsonSerializerOptions { WriteIndented = false }));
        var paddedJsonLength = (json.Length + 3) & ~3;
        var totalLength = 12 + 8 + paddedJsonLength + 8 + (int)binary.Length;

        using var output = File.Create(path);
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: false);
        writer.Write(0x46546C67u);
        writer.Write(2u);
        writer.Write((uint)totalLength);
        writer.Write((uint)paddedJsonLength);
        writer.Write(0x4E4F534Au);
        writer.Write(json);
        for (var index = json.Length; index < paddedJsonLength; index++)
        {
            writer.Write((byte)' ');
        }

        writer.Write((uint)binary.Length);
        writer.Write(0x004E4942u);
        binary.Position = 0;
        binary.CopyTo(output);
    }

    private void AddPrimitive(
        string partId,
        Shape shape,
        Vector3 translation,
        Vector3 scale,
        Quaternion rotation,
        int material)
    {
        var meshIndex = GetMesh(shape, material);
        var semanticName = nodes[parts[partId]].Name;
        var childIndex = AddNode(new NodeData($"{semanticName}_mesh_{nodes[parts[partId]].Children.Count + 1}")
        {
            Mesh = meshIndex,
            Translation = translation,
            Scale = scale,
            Rotation = rotation
        });
        nodes[parts[partId]].Children.Add(childIndex);
    }

    private int GetMesh(Shape shape, int material)
    {
        if (meshCache.TryGetValue((shape, material), out var existing))
        {
            return existing;
        }

        var geometry = GetGeometry(shape);
        var mesh = new JsonObject
        {
            ["name"] = $"{materials[material]["name"]}_{shape}",
            ["primitives"] = new JsonArray(new JsonObject
            {
                ["attributes"] = new JsonObject
                {
                    ["POSITION"] = geometry.PositionAccessor,
                    ["NORMAL"] = geometry.NormalAccessor
                },
                ["indices"] = geometry.IndexAccessor,
                ["material"] = material
            })
        };
        meshes.Add(mesh);
        var index = meshes.Count - 1;
        meshCache.Add((shape, material), index);
        return index;
    }

    private GeometryAccessors GetGeometry(Shape shape)
    {
        if (geometryCache.TryGetValue(shape, out var existing))
        {
            return existing;
        }

        var geometry = shape == Shape.Box ? Geometry.Box() : Geometry.Cylinder(24);
        var positionAccessor = AddVectorAccessor(geometry.Positions, "VEC3", 34962);
        var normalAccessor = AddVectorAccessor(geometry.Normals, "VEC3", 34962);
        var indexAccessor = AddIndexAccessor(geometry.Indices);
        var accessors = new GeometryAccessors(positionAccessor, normalAccessor, indexAccessor);
        geometryCache.Add(shape, accessors);
        return accessors;
    }

    private int AddVectorAccessor(Vector3[] values, string type, int target)
    {
        PadBinary();
        var offset = (int)binary.Position;
        using (var writer = new BinaryWriter(binary, Encoding.UTF8, leaveOpen: true))
        {
            foreach (var value in values)
            {
                writer.Write(value.X);
                writer.Write(value.Y);
                writer.Write(value.Z);
            }
        }

        var view = AddBufferView(offset, values.Length * 12, target);
        var minimum = new Vector3(
            values.Min(value => value.X),
            values.Min(value => value.Y),
            values.Min(value => value.Z));
        var maximum = new Vector3(
            values.Max(value => value.X),
            values.Max(value => value.Y),
            values.Max(value => value.Z));
        accessors.Add(new JsonObject
        {
            ["bufferView"] = view,
            ["componentType"] = 5126,
            ["count"] = values.Length,
            ["type"] = type,
            ["min"] = Floats(minimum.X, minimum.Y, minimum.Z),
            ["max"] = Floats(maximum.X, maximum.Y, maximum.Z)
        });
        return accessors.Count - 1;
    }

    private int AddIndexAccessor(ushort[] values)
    {
        PadBinary();
        var offset = (int)binary.Position;
        using (var writer = new BinaryWriter(binary, Encoding.UTF8, leaveOpen: true))
        {
            foreach (var value in values)
            {
                writer.Write(value);
            }
        }

        var view = AddBufferView(offset, values.Length * 2, 34963);
        accessors.Add(new JsonObject
        {
            ["bufferView"] = view,
            ["componentType"] = 5123,
            ["count"] = values.Length,
            ["type"] = "SCALAR",
            ["min"] = Ints(values.Min()),
            ["max"] = Ints(values.Max())
        });
        return accessors.Count - 1;
    }

    private int AddBufferView(int offset, int length, int target)
    {
        bufferViews.Add(new JsonObject
        {
            ["buffer"] = 0,
            ["byteOffset"] = offset,
            ["byteLength"] = length,
            ["target"] = target
        });
        return bufferViews.Count - 1;
    }

    private int AddNode(NodeData node)
    {
        nodes.Add(node);
        return nodes.Count - 1;
    }

    private void PadBinary()
    {
        while (binary.Position % 4 != 0)
        {
            binary.WriteByte(0);
        }
    }

    private static Quaternion RotationFromZ(Vector3 direction)
    {
        var target = Vector3.Normalize(direction);
        var dot = Math.Clamp(Vector3.Dot(Vector3.UnitZ, target), -1, 1);
        if (dot > 0.9999f)
        {
            return Quaternion.Identity;
        }

        if (dot < -0.9999f)
        {
            return Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI);
        }

        return Quaternion.CreateFromAxisAngle(
            Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, target)),
            MathF.Acos(dot));
    }

    private static JsonArray Floats(params float[] values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());

    private static JsonArray Ints(params int[] values) =>
        new(values.Select(value => JsonValue.Create(value)).ToArray());

    private enum Shape
    {
        Box,
        Cylinder
    }

    private sealed class NodeData(string name)
    {
        public string Name { get; } = name;
        public List<int> Children { get; } = [];
        public int? Mesh { get; init; }
        public Vector3 Translation { get; init; }
        public Vector3 Scale { get; init; } = Vector3.One;
        public Quaternion Rotation { get; init; } = Quaternion.Identity;

        public JsonObject ToJson()
        {
            var node = new JsonObject { ["name"] = Name };
            if (Children.Count > 0)
            {
                node["children"] = Ints(Children.ToArray());
            }

            if (Mesh is int mesh)
            {
                node["mesh"] = mesh;
                node["translation"] = Floats(Translation.X, Translation.Y, Translation.Z);
                node["scale"] = Floats(Scale.X, Scale.Y, Scale.Z);
                node["rotation"] = Floats(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W);
            }

            return node;
        }
    }

    private sealed record GeometryAccessors(
        int PositionAccessor,
        int NormalAccessor,
        int IndexAccessor);
}

internal sealed record Geometry(Vector3[] Positions, Vector3[] Normals, ushort[] Indices)
{
    public static Geometry Box()
    {
        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var indices = new List<ushort>();
        AddFace(new(-0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f), Vector3.UnitZ);
        AddFace(new(0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, -0.5f), -Vector3.UnitZ);
        AddFace(new(0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, -0.5f), new(0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, 0.5f), Vector3.UnitX);
        AddFace(new(-0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, -0.5f), -Vector3.UnitX);
        AddFace(new(-0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f), Vector3.UnitY);
        AddFace(new(-0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, 0.5f), new(-0.5f, -0.5f, 0.5f), -Vector3.UnitY);
        return new(positions.ToArray(), normals.ToArray(), indices.ToArray());

        void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            var start = (ushort)positions.Count;
            positions.AddRange([a, b, c, d]);
            normals.AddRange([normal, normal, normal, normal]);
            indices.AddRange([start, (ushort)(start + 1), (ushort)(start + 2), start, (ushort)(start + 2), (ushort)(start + 3)]);
        }
    }

    public static Geometry Cylinder(int segments)
    {
        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var indices = new List<ushort>();
        for (var segment = 0; segment < segments; segment++)
        {
            var angle = MathF.Tau * segment / segments;
            var direction = new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0);
            positions.Add(direction * 0.5f + new Vector3(0, 0, -0.5f));
            positions.Add(direction * 0.5f + new Vector3(0, 0, 0.5f));
            normals.Add(direction);
            normals.Add(direction);
        }

        for (var segment = 0; segment < segments; segment++)
        {
            var next = (segment + 1) % segments;
            var lower = (ushort)(segment * 2);
            var upper = (ushort)(lower + 1);
            var nextLower = (ushort)(next * 2);
            var nextUpper = (ushort)(nextLower + 1);
            indices.AddRange([lower, nextLower, upper, upper, nextLower, nextUpper]);
        }

        AddCap(-0.5f, -Vector3.UnitZ, reverse: true);
        AddCap(0.5f, Vector3.UnitZ, reverse: false);
        return new(positions.ToArray(), normals.ToArray(), indices.ToArray());

        void AddCap(float z, Vector3 normal, bool reverse)
        {
            var center = (ushort)positions.Count;
            positions.Add(new Vector3(0, 0, z));
            normals.Add(normal);
            for (var segment = 0; segment < segments; segment++)
            {
                var angle = MathF.Tau * segment / segments;
                positions.Add(new Vector3(MathF.Cos(angle) * 0.5f, MathF.Sin(angle) * 0.5f, z));
                normals.Add(normal);
            }

            for (var segment = 0; segment < segments; segment++)
            {
                var current = (ushort)(center + 1 + segment);
                var next = (ushort)(center + 1 + ((segment + 1) % segments));
                indices.AddRange(reverse ? [center, next, current] : [center, current, next]);
            }
        }
    }
}
