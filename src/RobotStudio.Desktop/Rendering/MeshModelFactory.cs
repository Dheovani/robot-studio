using System.Windows.Media;
using System.Windows.Media.Media3D;
using RobotStudio.Simulation;

namespace RobotStudio.Desktop.Rendering;

public static class MeshModelFactory
{
    public static GeometryModel3D CreateBox(
        VisualVector3 center,
        VisualVector3 size,
        Color color) =>
        CreateModel(CreateBoxMesh(center, size), color);

    public static GeometryModel3D CreateCube(
        Point3D center,
        double size,
        Color color) =>
        CreateBox(
            new VisualVector3(center.X, center.Y, center.Z),
            new VisualVector3(size, size, size),
            color);

    public static GeometryModel3D CreateOrientedBox(
        Point3D start,
        Point3D end,
        double thickness,
        Color color)
    {
        var direction = end - start;
        if (direction.LengthSquared <= 0.000_001)
        {
            return CreateCube(start, thickness, color);
        }

        direction.Normalize();
        var up = new Vector3D(0, 0, 1);
        var side = Vector3D.CrossProduct(direction, up);
        if (side.LengthSquared <= 0.000_001)
        {
            side = new Vector3D(1, 0, 0);
        }

        side.Normalize();
        var vertical = Vector3D.CrossProduct(side, direction);
        vertical.Normalize();

        side *= thickness / 2;
        vertical *= thickness / 2;

        return CreateModel(
            new MeshGeometry3D
            {
                Positions = new Point3DCollection
                {
                    start - side - vertical,
                    start + side - vertical,
                    start + side + vertical,
                    start - side + vertical,
                    end - side - vertical,
                    end + side - vertical,
                    end + side + vertical,
                    end - side + vertical
                },
                TriangleIndices = CreateBoxTriangleIndices()
            },
            color);
    }

    public static Model3DGroup CreatePlanarWorkspace(
        double reach,
        double gridSpacing,
        double floorZ,
        double gridThickness,
        double ringThickness,
        Color gridColor,
        Color ringColor,
        Color originColor)
    {
        var group = new Model3DGroup();

        for (var offset = -reach; offset <= reach; offset += gridSpacing)
        {
            group.Children.Add(CreateOrientedBox(
                new Point3D(-reach, offset, floorZ),
                new Point3D(reach, offset, floorZ),
                gridThickness,
                gridColor));
            group.Children.Add(CreateOrientedBox(
                new Point3D(offset, -reach, floorZ),
                new Point3D(offset, reach, floorZ),
                gridThickness,
                gridColor));
        }

        const int segmentCount = 72;
        for (var index = 0; index < segmentCount; index++)
        {
            var startAngle = 2 * Math.PI * index / segmentCount;
            var endAngle = 2 * Math.PI * (index + 1) / segmentCount;
            group.Children.Add(CreateOrientedBox(
                new Point3D(Math.Cos(startAngle) * reach, Math.Sin(startAngle) * reach, floorZ + 2),
                new Point3D(Math.Cos(endAngle) * reach, Math.Sin(endAngle) * reach, floorZ + 2),
                ringThickness,
                ringColor));
        }

        group.Children.Add(CreateBox(
            new VisualVector3(0, 0, floorZ - 2),
            new VisualVector3(24, 24, 12),
            originColor));

        return group;
    }

    private static GeometryModel3D CreateModel(
        MeshGeometry3D mesh,
        Color color)
    {
        var material = new DiffuseMaterial(new SolidColorBrush(color));
        return new GeometryModel3D(mesh, material)
        {
            BackMaterial = material
        };
    }

    private static MeshGeometry3D CreateBoxMesh(
        VisualVector3 center,
        VisualVector3 size)
    {
        var halfX = Math.Max(size.XMillimeters, 1) / 2;
        var halfY = Math.Max(size.YMillimeters, 1) / 2;
        var halfZ = Math.Max(size.ZMillimeters, 1) / 2;
        var x = center.XMillimeters;
        var y = center.YMillimeters;
        var z = center.ZMillimeters;

        return new MeshGeometry3D
        {
            Positions = new Point3DCollection
            {
                new(x - halfX, y - halfY, z - halfZ),
                new(x + halfX, y - halfY, z - halfZ),
                new(x + halfX, y + halfY, z - halfZ),
                new(x - halfX, y + halfY, z - halfZ),
                new(x - halfX, y - halfY, z + halfZ),
                new(x + halfX, y - halfY, z + halfZ),
                new(x + halfX, y + halfY, z + halfZ),
                new(x - halfX, y + halfY, z + halfZ)
            },
            TriangleIndices = CreateBoxTriangleIndices()
        };
    }

    private static Int32Collection CreateBoxTriangleIndices() =>
    [
        0, 2, 1, 0, 3, 2,
        4, 5, 6, 4, 6, 7,
        0, 1, 5, 0, 5, 4,
        1, 2, 6, 1, 6, 5,
        2, 3, 7, 2, 7, 6,
        3, 0, 4, 3, 4, 7
    ];
}
