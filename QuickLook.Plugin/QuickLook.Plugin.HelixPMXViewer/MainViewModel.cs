using System;
using System.Collections.Generic;
using System.IO;
using HelixToolkit.Wpf.SharpDX;
using Transform3D = System.Windows.Media.Media3D.Transform3D;
using System.Linq;
using System.Windows.Controls;
using MMDExtensions;
using MMDExtensions.PMX;
using System.Windows.Media.Media3D;
using SharpDX;
using Color = SharpDX.Color;
using DiffuseMaterial = HelixToolkit.Wpf.SharpDX.DiffuseMaterial;
using MeshGeometry3D = HelixToolkit.Wpf.SharpDX.MeshGeometry3D;
using Material = HelixToolkit.Wpf.SharpDX.Material;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace QuickLook.Plugin.HelixPMXViewer
{
    public class Element3DCollection : List<Element3D>
    {
    }
    public class MainViewModel : UserControl
    {
        private PMXFormat format_;
        // public MeshGeometry3D ModelGeometry { get; set; }
        public Material ModelMaterial { get; set; }
        public Transform3D ModelTransform { get; set; }
        public Element3DCollection ModelGeometry { get; private set; }
        public DefaultEffectsManager SREffectsManager { get; set; }
        
        public Vector3 DirectionalLightDirection { get; private set; }
        public Color4 DirectionalLightColor { get; private set; }
        public Color4 AmbientLightColor { get; private set; }
        
        public MainViewModel(string path)
        {
            SREffectsManager = new DefaultEffectsManager();
            
            AmbientLightColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
            DirectionalLightColor = Color.White;
            DirectionalLightDirection = new Vector3(-2, -5, -2);
            
            
            format_ = LoadPmxMaterials(path);
            MeshCreationInfo creation_info = CreateMeshCreationInfoSingle();

            Console.WriteLine($"creation_info.value.Length: {creation_info.value.Length}");
            for (int index = 0, i_max = creation_info.value.Length; index < i_max; ++index)
            {
                int[] indices = creation_info.value[index].plane_indices.Select(x => (int)creation_info.reassign_dictionary[x]).ToArray();
                Console.WriteLine($"indices.count at index: {index} is {creation_info.value[index]} + {indices.Length}");
                
                Matrix3D rotationMatrix = new Matrix3D();
                // rotationMatrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), 140));
                // rotationMatrix.Rotate(new Quaternion(new Vector3D(0, 0, 1), 140));
                rotationMatrix.Rotate(new Quaternion(new Vector3D(0, 1, 0), 180));

                var mesh = new MeshGeometry3D
                {
                    Positions = new Vector3Collection(format_.vertex_list.vertex.Select(x => rotationMatrix.Transform(x.pos).ToVector3())),
                    Indices = new IntCollection(indices),
                    TextureCoordinates = new Vector2Collection(format_.vertex_list.vertex.Select(x => x.uv.ToVector2())),
                };
                mesh.Normals = CalculateNormals(mesh);
                Console.WriteLine($"normalCount: {mesh.Normals.Count}, {mesh.Normals[0]}, {mesh.Normals[1]}, {mesh.Normals[200]}");
                

                PhongMaterial material;
                DiffuseMaterial diffuseMaterial = new DiffuseMaterial();
                
                var textureIndex = format_.material_list.material[index].usually_texture_index;
                
                if (textureIndex == uint.MaxValue)
                {
                    material = new PhongMaterial { DiffuseColor = new Color4(160 / 255f, 160 / 255f, 160 / 255f, 1) };
                    // material = PhongMaterials.Red;
                    diffuseMaterial.DiffuseColor = Color4.White;
                    
                }
                else
                {
                    var textureModel = TextureModel.Create(Path.Combine(format_.meta_header.folder, format_.texture_list.texture_file[textureIndex]));
                   material = new PhongMaterial
                    {
                        // DiffuseMap = TextureModel.Create(new Uri($"{format_.meta_header.folder}, {format_.texture_list.texture_file[textureIndex]}").ToString()),
                        DiffuseMap = textureModel,
                        ReflectiveColor = new Color4(1.0f,0.5f,0.5f,1.0f),
                        SpecularColor = Color4.White,
                        AmbientColor = Color4.White,
                        SpecularShininess = 10f
                    };
                    diffuseMaterial.DiffuseMap = textureModel;
                }
                

                var model = new MeshGeometryModel3D { Geometry = mesh, Material = material};
                
                if (ModelGeometry == null || ModelGeometry.Count==0)
                {
                    ModelGeometry = new Element3DCollection();
                }
                ModelGeometry.Add(model);
                
            }
            
        }
        
        public Vector3Collection CalculateNormals(MeshGeometry3D mesh)
        {
            Vector3Collection positions = mesh.Positions;
            IntCollection indices = mesh.Indices;
            Vector3Collection normals = new Vector3Collection(new Vector3[positions.Count]);
            Console.WriteLine($"calculate normal, indices.count = {indices.Count}");
            for (int i = 0; i < indices.Count; i += 3)
            {
                int index1 = indices[i];
                int index2 = indices[i + 1];
                int index3 = indices[i + 2];

                Vector3 v1 = positions[index1];
                Vector3 v2 = positions[index2];
                Vector3 v3 = positions[index3];

                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);

                normals[index1] += normal;
                normals[index2] += normal;
                normals[index3] += normal;
            }

            for (int i = 0; i < normals.Count; i++)
            {
                normals[i].Normalize();
            }

            mesh.Normals = normals;
            return normals;
        }

        public static PMXFormat LoadPmxMaterials(string path)
        {
            return PMXLoaderScript.Import(path);
        }
        

        MeshCreationInfo.Pack[] CreateMeshCreationInfoPacks()
        {
            uint plane_start = 0;
            //マテリアル単位のMeshCreationInfo.Packを作成する
            return Enumerable.Range(0, format_.material_list.material.Length)
                .Select(x =>
                {
                    MeshCreationInfo.Pack pack = new MeshCreationInfo.Pack();
                    pack.material_index = (uint) x;
                    uint plane_count = format_.material_list.material[x].face_vert_count;
                    pack.plane_indices = format_.face_vertex_list.face_vert_index.Skip((int) plane_start)
                        .Take((int) plane_count)
                        .ToArray();
                    pack.vertices = pack.plane_indices.Distinct() //重複削除
                        .ToArray();
                    plane_start += plane_count;
                    return pack;
                })
                .ToArray();
        }

        MeshCreationInfo CreateMeshCreationInfoSingle()
        {
            MeshCreationInfo result = new MeshCreationInfo();
            //全マテリアルを設定
            result.value = CreateMeshCreationInfoPacks();
            //全頂点を設定
            result.all_vertices =
                Enumerable.Range(0, format_.vertex_list.vertex.Length).Select(x => (uint) x).ToArray();
            //頂点リアサインインデックス用辞書作成
            result.reassign_dictionary = new Dictionary<uint, uint>(result.all_vertices.Length);
            for (uint i = 0, i_max = (uint) result.all_vertices.Length; i < i_max; ++i)
            {
                result.reassign_dictionary[i] = i;
            }

            return result;
        }
    }

    public class MeshCreationInfo
    {
        public class Pack
        {
            public uint material_index; //マテリアル
            public uint[] plane_indices; //面
            public uint[] vertices; //頂点
        }

        public Pack[] value;
        public uint[] all_vertices; //総頂点
        public Dictionary<uint, uint> reassign_dictionary; //頂点リアサインインデックス用辞書
    }
}