// // Copyright © 2023 Paddy Xu
// // 
// // This file is part of QuickLook program.
// // 
// // This program is free software: you can redistribute it and/or modify
// // it under the terms of the GNU General Public License as published by
// // the Free Software Foundation, either version 3 of the License, or
// // (at your option) any later version.
// // 
// // This program is distributed in the hope that it will be useful,
// // but WITHOUT ANY WARRANTY; without even the implied warranty of
// // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// // GNU General Public License for more details.
// // 
// // You should have received a copy of the GNU General Public License
// // along with this program.  If not, see <http://www.gnu.org/licenses/>.
// using System.Collections.Generic;
//
// using HelixToolkit.Wpf.SharpDX;
// using HelixToolkit.Wpf.SharpDX.Controls;
// using HelixToolkit.Wpf.SharpDX.Core;
//
//
// namespace QuickLook.Plugin.HelixPMXViewer
// {
//     public class CustomRenderTechniquesManager : DefaultRenderTechniquesManager
//     {
//         private Dictionary<string, RenderTechnique> renderTechniques = new Dictionary<string, RenderTechnique>();
//         protected override void InitTechniques()
//         {
//             AddRenderTechnique(DefaultRenderTechniqueNames.Blinn, Properties.Resources._custom);
//             AddRenderTechnique(DefaultRenderTechniqueNames.Points,Properties.Resources._custom);
//             AddRenderTechnique(DefaultRenderTechniqueNames.Lines, Properties.Resources._custom);
//             AddRenderTechnique(DefaultRenderTechniqueNames.BillboardText, Properties.Resources._custom);
//             AddRenderTechnique("RenderCustom", Properties.Resources._custom);
//         }
//     }
// }