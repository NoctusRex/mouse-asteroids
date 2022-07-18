using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MouseAsteroids.Models
{
    public abstract class Entity
    {
        public Point Position { get; set; }
        public virtual Vector Direction { get; set; }
        public double Speed { get; set; }
        public bool Destroyed { get; set; }
        public abstract UIElement CanvasElement { get; set; }

        public abstract void Update(Dictionary<string, object>? parameters = null);
        public abstract void Draw(Dictionary<string, object>? parameters = null);

    }
}
