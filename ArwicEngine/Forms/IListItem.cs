using Microsoft.Xna.Framework.Graphics;

namespace ArwicEngine.Forms
{
    public interface IListItem
    {
        Button Button { get; set; }
        RichText Text { get; set; }
    }
}
