using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Audio
{
    public enum FileType
    {
        MP3,
        WAV,
        OGG,
        FLAC,
        M4A,
        AAC
    }

    public class AudioData
    {
        public  string Name { get; set; }
        public  string Path { get; set; }
        public  FileType Type { get; set; }
    }
}
