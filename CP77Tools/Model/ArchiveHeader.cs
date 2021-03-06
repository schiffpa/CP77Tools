﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.IoC;
using CP77Tools.Services;
using WolvenKit.CR2W.Types;

namespace CP77Tools.Model
{
    public class ArHeader
    {
        public static int SIZE = 40;

        public byte[] Magic { get; set; }
        public uint Version { get; set; }
        public ulong Tableoffset { get; set; }
        public ulong Tablesize { get; set; }
        public ulong Unk3 { get; set; }
        public ulong Filesize { get; set; }

        public ArHeader(BinaryReader br)
        {
            Read(br);
        }



        private void Read(BinaryReader br)
        {
            Magic = br.ReadBytes(4);
            if (!Magic.SequenceEqual(new byte[] { 82, 68, 65, 82 }))
                throw new NotImplementedException();

            Version = br.ReadUInt32();
            Tableoffset = br.ReadUInt64();
            Tablesize = br.ReadUInt64();
            Unk3 = br.ReadUInt64();
            Filesize = br.ReadUInt64();
        }
    }
    public class ArTable
    {
        public uint Num { get; private set; }
        public uint Size { get; private set; }
        public ulong Checksum { get; private set; }
        public uint Table1count { get; private set; }
        public uint Table2count { get; private set; }
        public uint Table3count { get; private set; }
        public Dictionary<ulong, FileInfoEntry> FileInfo { get; private set; }
        public List<OffsetEntry> Offsets { get; private set; }
        public List<HashEntry> HashTable { get; private set; }

        public ArTable(BinaryReader br)
        {
            Read(br);

            FileInfo = new Dictionary<ulong, FileInfoEntry>();
            Offsets = new List<OffsetEntry>();
            HashTable = new List<HashEntry> ();

            // read tables
            for (int i = 0; i < Table1count; i++)
            {
                var entry = new FileInfoEntry(br);

                if (!FileInfo.ContainsKey(entry.NameHash64))
                {
                    FileInfo.Add(entry.NameHash64, entry);
                }
                else
                {
                    
                }
            }

            for (int i = 0; i < Table2count; i++)
            {
                Offsets.Add(new OffsetEntry(br));
            }

            for (int i = 0; i < Table3count; i++)
            {
                HashTable.Add(new HashEntry(br));
            }
        }

        private void Read(BinaryReader br)
        {
            Num = br.ReadUInt32();
            Size = br.ReadUInt32();
            Checksum = br.ReadUInt64();
            Table1count = br.ReadUInt32();
            Table2count = br.ReadUInt32();
            Table3count = br.ReadUInt32();
        }
    }
    public class HashEntry
    {
        public ulong Hash { get; set; }

        public HashEntry(BinaryReader br)
        {
            Read(br);
        }

        private void Read(BinaryReader br)
        {
            Hash = br.ReadUInt64();
        }
    }
    public class OffsetEntry
    {
        public ulong Offset { get; set; }
        public uint PhysicalSize { get; set; }
        public uint VirtualSize { get; set; }

        public OffsetEntry(BinaryReader br)
        {
            Read(br);
        }

        private void Read(BinaryReader br)
        {
            Offset = br.ReadUInt64();
            PhysicalSize = br.ReadUInt32();
            VirtualSize = br.ReadUInt32();
        }
    }
    public class FileInfoEntry
    {
        public ulong NameHash64 { get; private set; }
        public DateTime DateTime { get; private set; }
        public uint FileFlags { get; private set; }
        public uint FirstDataSector { get; private set; }
        public uint NextDataSector { get; private set; }
        public uint FirstUnkIndex { get; private set; }
        public uint NextUnkIndex { get; private set; }
        public byte[] SHA1Hash { get; private set; }

        private string _nameStr;
        public string NameStr => string.IsNullOrEmpty(_nameStr) ? NameHash64.ToString() : _nameStr;

        private IMainController _mainController;

        public FileInfoEntry(BinaryReader br)
        {
            _mainController = ServiceLocator.Default.ResolveType<IMainController>();

            Read(br, _mainController);
        }

        private void Read(BinaryReader br, IMainController mainController)
        {
            NameHash64 = br.ReadUInt64();

            if (mainController != null && mainController.Hashdict.ContainsKey(NameHash64))
                _nameStr = mainController.Hashdict[NameHash64];

            DateTime = System.DateTime.FromFileTime(br.ReadInt64());


            FileFlags = br.ReadUInt32();
            FirstDataSector = br.ReadUInt32();
            NextDataSector = br.ReadUInt32();
            FirstUnkIndex = br.ReadUInt32();
            NextUnkIndex = br.ReadUInt32();

            SHA1Hash = br.ReadBytes(20);
        }
    }
}
