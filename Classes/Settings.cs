using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSorter.Classes
{
    public class Settings
    {
        private const string Filename = @"settings.xml";
        public SortingOrder SortingOrder { get; set; }

        public string Path { get; set; }
        public bool Subfolders { get; set; }
        public bool Simulate { get; set; }

        public Settings()
        {
            Subfolders = true;
        }

        internal void Load()
        {
            if (File.Exists(Filename))
            {
                var settings = XMLSerializer.Deserialize<Settings>(Filename);
                this.Path = settings.Path;
                this.SortingOrder = settings.SortingOrder;
                this.Subfolders = settings.Subfolders;
                this.Simulate = settings.Simulate;
            }
            else
            {
                Save();
            }
        }

        internal void Save()
        {
            XMLSerializer.Serialize(this, Filename);
        }
    }
}
