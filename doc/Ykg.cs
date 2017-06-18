// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System;
using System.Collections.Generic;

namespace Kaitai
{
    public partial class Ykg : KaitaiStruct
    {
        public static Ykg FromFile(string fileName)
        {
            return new Ykg(new KaitaiStream(fileName));
        }

        public enum FrameType
        {
            End = 0,
            Single = 1,
            Loop = 2,
            Animation = 4,
        }

        public Ykg(KaitaiStream io, KaitaiStruct parent = null, Ykg root = null) : base(io)
        {
            m_parent = parent;
            m_root = root ?? this;
            _parse();
        }

        private void _parse()
        {
            f_colorData = false;
            f_alphaData = false;
            f_frameCount = false;
            f_frameData = false;
            _magic = m_io.EnsureFixedContents(new byte[] { 89, 75, 71, 48, 48, 48, 0, 0 });
            _headerLength = m_io.ReadU4le();
            __unnamed2 = m_io.ReadBytes(28);
            _colorOffset = m_io.ReadU4le();
            _colorLength = m_io.ReadU4le();
            _alphaOffset = m_io.ReadU4le();
            _alphaLength = m_io.ReadU4le();
            _frameOffset = m_io.ReadU4le();
            _frameLength = m_io.ReadU4le();
        }
        public partial class FrameData : KaitaiStruct
        {
            public static FrameData FromFile(string fileName)
            {
                return new FrameData(new KaitaiStream(fileName));
            }

            public FrameData(KaitaiStream io, Ykg parent = null, Ykg root = null) : base(io)
            {
                m_parent = parent;
                m_root = root;
                _parse();
            }

            private void _parse()
            {
                _x = m_io.ReadU4le();
                _y = m_io.ReadU4le();
                _width = m_io.ReadU4le();
                _height = m_io.ReadU4le();
                _duration = m_io.ReadU4le();
                _frameType = ((Ykg.FrameType) m_io.ReadU4le());
                _unknown1 = m_io.ReadU4le();
                _unknown2 = m_io.ReadU4le();
            }
            private uint _x;
            private uint _y;
            private uint _width;
            private uint _height;
            private uint _duration;
            private FrameType _frameType;
            private uint _unknown1;
            private uint _unknown2;
            private Ykg m_root;
            private Ykg m_parent;
            public uint X { get { return _x; } }
            public uint Y { get { return _y; } }
            public uint Width { get { return _width; } }
            public uint Height { get { return _height; } }
            public uint Duration { get { return _duration; } }
            public FrameType FrameType { get { return _frameType; } }
            public uint Unknown1 { get { return _unknown1; } }
            public uint Unknown2 { get { return _unknown2; } }
            public Ykg M_Root { get { return m_root; } }
            public Ykg M_Parent { get { return m_parent; } }
        }
        private bool f_colorData;
        private byte[] _colorData;
        public byte[] ColorData
        {
            get
            {
                if (f_colorData)
                    return _colorData;
                if (ColorLength > 0) {
                    long _pos = m_io.Pos;
                    m_io.Seek((ColorOffset + 4));
                    _colorData = m_io.ReadBytes((ColorLength - 4));
                    m_io.Seek(_pos);
                }
                f_colorData = true;
                return _colorData;
            }
        }
        private bool f_alphaData;
        private byte[] _alphaData;
        public byte[] AlphaData
        {
            get
            {
                if (f_alphaData)
                    return _alphaData;
                if (AlphaLength > 0) {
                    long _pos = m_io.Pos;
                    m_io.Seek(AlphaOffset);
                    _alphaData = m_io.ReadBytes(AlphaLength);
                    m_io.Seek(_pos);
                }
                f_alphaData = true;
                return _alphaData;
            }
        }
        private bool f_frameCount;
        private int _frameCount;
        public int FrameCount
        {
            get
            {
                if (f_frameCount)
                    return _frameCount;
                _frameCount = (int) ((FrameLength / 32));
                f_frameCount = true;
                return _frameCount;
            }
        }
        private bool f_frameData;
        private List<FrameData> _frameData;
        public List<FrameData> FrameData
        {
            get
            {
                if (f_frameData)
                    return _frameData;
                if (FrameCount > 0) {
                    long _pos = m_io.Pos;
                    m_io.Seek(FrameOffset);
                    _frameData = new List<FrameData>((int) (FrameCount));
                    for (var i = 0; i < FrameCount; i++) {
                        _frameData.Add(new FrameData(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                }
                f_frameData = true;
                return _frameData;
            }
        }
        private byte[] _magic;
        private uint _headerLength;
        private byte[] __unnamed2;
        private uint _colorOffset;
        private uint _colorLength;
        private uint _alphaOffset;
        private uint _alphaLength;
        private uint _frameOffset;
        private uint _frameLength;
        private Ykg m_root;
        private KaitaiStruct m_parent;
        public byte[] Magic { get { return _magic; } }
        public uint HeaderLength { get { return _headerLength; } }
        public byte[] Unnamed_2 { get { return __unnamed2; } }
        public uint ColorOffset { get { return _colorOffset; } }
        public uint ColorLength { get { return _colorLength; } }
        public uint AlphaOffset { get { return _alphaOffset; } }
        public uint AlphaLength { get { return _alphaLength; } }
        public uint FrameOffset { get { return _frameOffset; } }
        public uint FrameLength { get { return _frameLength; } }
        public Ykg M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
