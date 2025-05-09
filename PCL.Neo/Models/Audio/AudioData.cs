using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL.Neo.Models.Audio
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
        public required string Name { get; set; }
        public required string Path { get; set; }
        public required FileType Type { get; set; }
    }
}
