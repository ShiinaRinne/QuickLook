using System;
using MMDExtensions;
using MMDExtensions.PMX;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

#if DX11_1
using Device = SharpDX.Direct3D11.Device1;
using DeviceContext = SharpDX.Direct3D11.DeviceContext1;
#else
using Device = SharpDX.Direct3D11.Device;
#endif


namespace QuickLook.Plugin.HelixPMXViewer
{
    public partial class HelixViewerPanel : UserControl
    {
        private PMXFormat format_;
        public HelixViewerPanel(string path)
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            DataContext = new MainViewModel(path);
            
            // SREffectsManager = new DefaultEffectsManager();

            
            // DataContext = this;
            
            // format_ = LoadPmxMaterials(path);
            // MeshCreationInfo creation_info = CreateMeshCreationInfoSingle();
            
            // var models = SceneModels.LoadModel(creation_info, format_);
            // PreviewModel2 = models;
        }
        
        public static PMXFormat LoadPmxMaterials(string path)
        {
            return PMXLoaderScript.Import(path);
        }

        MeshCreationInfo CreateMeshCreationInfoSingle()
        {
            MeshCreationInfo result = new MeshCreationInfo();
            //全マテリアルを設定
            result.value = CreateMeshCreationInfoPacks();
            //全頂点を設定
            result.all_vertices = Enumerable.Range(0, format_.vertex_list.vertex.Length).Select(x => (uint)x).ToArray();
            //頂点リアサインインデックス用辞書作成
            result.reassign_dictionary = new Dictionary<uint, uint>(result.all_vertices.Length);
            for (uint i = 0, i_max = (uint)result.all_vertices.Length; i < i_max; ++i)
            {
                result.reassign_dictionary[i] = i;
            }
            return result;
        }
        MeshCreationInfo.Pack[] CreateMeshCreationInfoPacks()
        {
            uint plane_start = 0;
            //マテリアル単位のMeshCreationInfo.Packを作成する
            return Enumerable.Range(0, format_.material_list.material.Length)
                            .Select(x =>
                            {
                                MeshCreationInfo.Pack pack = new MeshCreationInfo.Pack();
                                pack.material_index = (uint)x;
                                uint plane_count = format_.material_list.material[x].face_vert_count;
                                pack.plane_indices = format_.face_vertex_list.face_vert_index.Skip((int)plane_start)
                                                                                                        .Take((int)plane_count)
                                                                                                        .ToArray();
                                pack.vertices = pack.plane_indices.Distinct() //重複削除
                                                                        .ToArray();
                                plane_start += plane_count;
                                return pack;
                            })
                            .ToArray();
        }
    }
}

public class MeshCreationInfo
{
    public class Pack
    {
        public uint material_index; //マテリアル
        public uint[] plane_indices;    //面
        public uint[] vertices;     //頂点
    }
    public Pack[] value;
    public uint[] all_vertices;         //総頂点
    public Dictionary<uint, uint> reassign_dictionary;  //頂点リアサインインデックス用辞書
}