// <copyright file="SupportedFileClass.cs" company="Maliev Company Limited">
// Copyright (c) Maliev Company Limited. All rights reserved.
// </copyright>

namespace Maliev.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// This class stored list of supported file formats
    /// </summary>
    public static class SupportedFileClass
    {
        /// <summary>
        /// List of supported 3D print file format
        /// </summary>
        private static Dictionary<string, List<string>> supported3DPrintFormat = new Dictionary<string, List<string>>
        {
            {
                "3D Models",
                new List<string>
                {
                    ".obj",
                    ".3mf"
                }
            },
            {
                "STEP Files",
                new List<string>
                {
                    ".stp",
                    ".step"
                }
            },
            {
                "StereoLithography Files",
                new List<string>
                {
                    ".stl"
                }
            },
        };

        /// <summary>
        /// List of supported CAD file format
        /// </summary>
        private static Dictionary<string, List<string>> supportedCadFormat = new Dictionary<string, List<string>>
        {
            {
                "IGES Files",
                new List<string>
                {
                    ".igs",
                    ".iges"
                }
            },
            {
                "AutoCAD Files",
                new List<string>
                {
                    ".dwg",
                    ".dxf",
                    ".dwf",
                    ".dwfx"
                }
            },
            {
                "Parasolid Files",
                new List<string>
                {
                    ".x_t",
                    ".x_b",
                    ".xmt_txt"
                }
            },
            {
                "ProE/Creo Files",
                new List<string>
                {
                    ".prt",
                    ".asm",
                    ".prt.*",
                    ".asm.*"
                }
            },
            {
                "ACIS Kernel SAT Files",
                new List<string>
                {
                    ".sat",
                    ".sab"
                }
            },
            {
                "STEP Files",
                new List<string>
                {
                    ".stp",
                    ".step"
                }
            },
            {
                "VDA Files",
                new List<string>
                {
                    ".vda"
                }
            },
            {
                "Rhino 3D Files",
                new List<string>
                {
                    ".3dm"
                }
            },
            {
                "SolidWorks Files",
                new List<string>
                {
                    ".sldprt",
                    ".sldasm",
                    ".slddrw"
                }
            },
            {
                "Solid Edge Files",
                new List<string>
                {
                    ".par",
                    ".psm",
                    ".asm"
                }
            },
            {
                "Autodesk Inventor Files",
                new List<string>
                {
                    ".ipt",
                    ".iam",
                    ".idw"
                }
            },
            {
                "KeyCreator Files",
                new List<string>
                {
                    ".ckd"
                }
            },
            {
                "Unigraphics/NX Files",
                new List<string>
                {
                    ".prt"
                }
            },
            {
                "StereoLithography Files",
                new List<string>
                {
                    ".stl"
                }
            },
            {
                "CATIA Files",
                new List<string>
                {
                    ".model",
                    ".exp",
                    ".catpart",
                    ".catproduct"
                }
            },
            {
                "SpaceClaim Files",
                new List<string>
                {
                    ".scdoc"
                }
            },
            {
                "Alibre/Geomagic Design Files",
                new List<string>
                {
                    ".ad_prt",
                    ".ad_smp"
                }
            },
            {
                "HPGL Plotter Files",
                new List<string>
                {
                    ".plt"
                }
            },
            {
                "PostScript Files",
                new List<string>
                {
                    ".eps",
                    ".ai",
                    ".ps"
                }
            },
            {
                "Images Files",
                new List<string>
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                }
            }
        };

        /// <summary>
        /// List of supported documentation file format
        /// </summary>
        private static Dictionary<string, List<string>> supportedDocumentFormat = new Dictionary<string, List<string>>
        {
            {
                "Documentation",
                new List<string>
                {
                    ".pdf",
                    ".tiff",
                    ".dxf",
                    ".dwg"
                }
            },
            {
                "Images Files",
                new List<string>
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                }
            }
        };

        /// <summary>
        /// Gets or sets supported 3D print file format
        /// </summary>
        /// <value>
        /// The supported3 d print format.
        /// </value>
        public static Dictionary<string, List<string>> Supported3DPrintFormat { get => supported3DPrintFormat; set => supported3DPrintFormat = value; }

        /// <summary>
        /// Gets or sets supported CAD format
        /// </summary>
        /// <value>
        /// The supported cad format.
        /// </value>
        public static Dictionary<string, List<string>> SupportedCadFormat { get => supportedCadFormat; set => supportedCadFormat = value; }

        /// <summary>
        /// Gets or sets supported document format
        /// </summary>
        /// <value>
        /// The supported document format.
        /// </value>
        public static Dictionary<string, List<string>> SupportedDocumentFormat { get => supportedDocumentFormat; set => supportedDocumentFormat = value; }
    }
}
