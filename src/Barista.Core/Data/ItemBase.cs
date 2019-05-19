using System;
using System.Collections.Generic;
using System.Linq;
using Barista.Core.Extensions;

namespace Barista.Core.Data
{
    public abstract class ItemBase
    {
        internal Dictionary<string, string> Settings = new Dictionary<string, string>();

        public string OriginalTitle { get; internal set; } = string.Empty;

        public string Title
        {
            get
            {
                var result = OriginalTitle;
                if (Emojize)
                {
                    result = result.ReplaceEmoji();
                }

                if (Trim)
                {
                    result = result.Trim();
                }

                return result;
            }
        }

        public bool Trim
        {
            get
            {
                if (Settings.TryGetValue("trim", out var trim)) return trim == "true";
                return true;
            }

        }

        public int Length
        {
            get
            {
                if (Settings.TryGetValue("length", out var length))
                {
                    if (int.TryParse(length, out var lengthAsInt)) return lengthAsInt;
                }

                return 50;
            }

        }

        public bool Refresh
        {
            get
            {
                if (Settings.TryGetValue("refresh", out var refresh))
                {
                    return refresh == "true";

                }

                return false;
            }
        }

        public string BashScript
        {
            get
            {
                if (Settings.TryGetValue("bash", out var bash))
                {
                    return bash.Replace("\"", "");
                }
                return string.Empty;
            }

        }

        public string[] Params
        {
            get
            {
                return Settings.Where(pv => pv.Key.StartsWith("param", StringComparison.Ordinal) && !string.IsNullOrEmpty(pv.Value))
                        .Select(pv => pv.Value.Replace("\"", ""))
                        .ToArray();
            }

        }

        public bool Terminal
        {
            get
            {
                if (Settings.TryGetValue("terminal", out var terminal))
                {
                    return terminal == "true";

                }
                return false;
            }

        }

        public string Href
        {
            get
            {
                if (Settings.TryGetValue("href", out var href))
                {
                    return href;
                }

                return string.Empty;
            }
        }

        public bool Emojize
        {
            get
            {

                if (Settings.TryGetValue("emojize", out var emojize))
                {
                    return emojize == "true";

                }

                return true;
            }

        }

        public bool Ansi
        {
            get
            {

                if (Settings.TryGetValue("emojize", out var ansi))
                {
                    return ansi == "true";

                }
                return false;
            }
        }

        public string Color
        {
            get
            {
                if (Settings.TryGetValue("color", out var color))
                {
                    return color;

                }
                return string.Empty;
            }
        }

        public string Font
        {
            get
            {
                if (Settings.TryGetValue("font", out var font))
                {
                    return font;

                }
                return string.Empty;
            }
        }
    }
}
