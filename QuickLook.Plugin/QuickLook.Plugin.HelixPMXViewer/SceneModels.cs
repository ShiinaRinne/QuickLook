using HelixToolkit.Wpf.SharpDX;
using MMDExtensions.PMX;
using System.Windows.Media.Media3D;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using SharpDX;
using System.Linq;
using System.IO;
using Quaternion=System.Windows.Media.Media3D.Quaternion;

namespace QuickLook.Plugin.HelixPMXViewer
{
    public class SceneModels
    {
        public MeshGeometry3D Model { get; set; }
        public static SceneNodeGroupModel3D LoadModel(MeshCreationInfo creation_info, PMXFormat format_)
        {
            var models = new SceneNodeGroupModel3D();
            for (int i = 0, i_max = creation_info.value.Length; i < i_max; ++i)
            {
                int[] indices = creation_info.value[i].plane_indices.Select(x => (int)creation_info.reassign_dictionary[x]).ToArray();
                Matrix3D rotationMatrix = new Matrix3D();
                rotationMatrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), 140));
                rotationMatrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), 140));

                var mesh = new MeshGeometry3D
                {
                    Positions = new Vector3Collection(format_.vertex_list.vertex.Select(x => rotationMatrix.Transform(x.pos).ToVector3())),
                    Indices = new IntCollection(indices),
                    TextureCoordinates = new Vector2Collection(format_.vertex_list.vertex.Select(x => x.uv.ToVector2()))
                };

                var textureIndex = format_.material_list.material[i].usually_texture_index;

                PhongMaterial material;

                if (textureIndex == uint.MaxValue)
                {
                    material = new PhongMaterial { DiffuseColor = new Color4(160 / 255f, 160 / 255f, 160 / 255f, 1) };
                }
                else
                {
                    // bool EnableSoftwareRendering = false;
                    // var DriverType = EnableSoftwareRendering ? SharpDX.Direct3D.DriverType.Warp : SharpDX.Direct3D.DriverType.Hardware;
                    // var device = new Device(DriverType, DeviceCreationFlags.BgraSupport);
                    // var texture = TextureLoader.FromFileAsResource(device,Path.Combine(format_.meta_header.folder,
                    //     format_.texture_list.texture_file[textureIndex]));
                    var textureModel = TextureModel.Create(Path.Combine(format_.meta_header.folder, format_.texture_list.texture_file[textureIndex]));
                    material = new PhongMaterial { DiffuseMap = textureModel};

                }

                var model = new MeshGeometryModel3D { Geometry = mesh, Material = material };
                // models.Add(model);
                models.AddNode(model.SceneNode);
            }

            return models;
        }
    }
}