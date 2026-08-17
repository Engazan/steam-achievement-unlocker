using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SteamAchievementUnlocker
{
    internal class KeyValueNode
    {
        private static readonly KeyValueNode _invalid = new();
        public string Name = "<root>";
        public KeyValueNodeType Type = KeyValueNodeType.None;
        public object Value;
        public bool Valid;

        public List<KeyValueNode> Children = null;

        public KeyValueNode this[string key]
        {
            get
            {
                if (this.Children == null)
                {
                    return _invalid;
                }

                var child = this.Children.SingleOrDefault(
                    c => string.Compare(c.Name, key, StringComparison.InvariantCultureIgnoreCase) == 0);

                if (child == null)
                {
                    return _invalid;
                }

                return child;
            }
        }

        public string AsString(string defaultValue)
        {
            if (this.Valid == false)
            {
                return defaultValue;
            }

            if (this.Value == null)
            {
                return defaultValue;
            }

            return this.Value.ToString();
        }

        public int AsInteger(int defaultValue)
        {
            if (this.Valid == false)
            {
                return defaultValue;
            }

            switch (this.Type)
            {
                case KeyValueNodeType.String:
                case KeyValueNodeType.WideString:
                {
                    return int.TryParse((string)this.Value, out int value) == false
                        ? defaultValue
                        : value;
                }

                case KeyValueNodeType.Int32:
                {
                    return (int)this.Value;
                }

                case KeyValueNodeType.Float32:
                {
                    return (int)((float)this.Value);
                }

                case KeyValueNodeType.UInt64:
                {
                    return (int)((ulong)this.Value & 0xFFFFFFFF);
                }
            }

            return defaultValue;
        }

        public float AsFloat(float defaultValue)
        {
            if (this.Valid == false)
            {
                return defaultValue;
            }

            switch (this.Type)
            {
                case KeyValueNodeType.String:
                case KeyValueNodeType.WideString:
                {
                    return float.TryParse((string)this.Value, out float value) == false
                        ? defaultValue
                        : value;
                }

                case KeyValueNodeType.Int32:
                {
                    return (int)this.Value;
                }

                case KeyValueNodeType.Float32:
                {
                    return (float)this.Value;
                }

                case KeyValueNodeType.UInt64:
                {
                    return (ulong)this.Value & 0xFFFFFFFF;
                }
            }

            return defaultValue;
        }

        public bool AsBoolean(bool defaultValue)
        {
            if (this.Valid == false)
            {
                return defaultValue;
            }

            switch (this.Type)
            {
                case KeyValueNodeType.String:
                case KeyValueNodeType.WideString:
                {
                    return int.TryParse((string)this.Value, out int value) == false
                        ? defaultValue
                        : value != 0;
                }

                case KeyValueNodeType.Int32:
                {
                    return ((int)this.Value) != 0;
                }

                case KeyValueNodeType.Float32:
                {
                    return ((int)((float)this.Value)) != 0;
                }

                case KeyValueNodeType.UInt64:
                {
                    return ((ulong)this.Value) != 0;
                }
            }

            return defaultValue;
        }

        public override string ToString()
        {
            if (this.Valid == false)
            {
                return "<invalid>";
            }

            if (this.Type == KeyValueNodeType.None)
            {
                return this.Name;
            }

            return $"{this.Name} = {this.Value}";
        }

        public static KeyValueNode LoadAsBinary(string path)
        {
            if (File.Exists(path) == false)
            {
                return null;
            }

            try
            {
                using (var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    KeyValueNode kv = new();
                    if (kv.ReadAsBinary(input) == false)
                    {
                        return null;
                    }
                    return kv;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool ReadAsBinary(Stream input)
        {
            this.Children = new();
            try
            {
                while (true)
                {
                    var type = (KeyValueNodeType)input.ReadValueU8();

                    if (type == KeyValueNodeType.End)
                    {
                        break;
                    }

                    KeyValueNode current = new()
                    {
                        Type = type,
                        Name = input.ReadStringUnicode(),
                    };

                    switch (type)
                    {
                        case KeyValueNodeType.None:
                        {
                            current.ReadAsBinary(input);
                            break;
                        }

                        case KeyValueNodeType.String:
                        {
                            current.Valid = true;
                            current.Value = input.ReadStringUnicode();
                            break;
                        }

                        case KeyValueNodeType.WideString:
                        {
                            throw new FormatException("wstring is unsupported");
                        }

                        case KeyValueNodeType.Int32:
                        {
                            current.Valid = true;
                            current.Value = input.ReadValueS32();
                            break;
                        }

                        case KeyValueNodeType.UInt64:
                        {
                            current.Valid = true;
                            current.Value = input.ReadValueU64();
                            break;
                        }

                        case KeyValueNodeType.Float32:
                        {
                            current.Valid = true;
                            current.Value = input.ReadValueF32();
                            break;
                        }

                        case KeyValueNodeType.Color:
                        {
                            current.Valid = true;
                            current.Value = input.ReadValueU32();
                            break;
                        }

                        case KeyValueNodeType.Pointer:
                        {
                            current.Valid = true;
                            current.Value = input.ReadValueU32();
                            break;
                        }

                        default:
                        {
                            throw new FormatException();
                        }
                    }

                    if (input.Position >= input.Length)
                    {
                        throw new FormatException();
                    }

                    this.Children.Add(current);
                }

                this.Valid = true;
                return input.Position == input.Length;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
