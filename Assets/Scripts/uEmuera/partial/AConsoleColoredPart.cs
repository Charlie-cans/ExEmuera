using uEmuera.Drawing;

namespace MinorShift.Emuera.GameView
{
    /// <summary>
	/// 带颜色的控制台部分
	/// </summary>
	abstract partial class AConsoleColoredPart : AConsoleDisplayPart
    {
        public Color pColor { get { return Color; } }
        public Color pButtonColor { get { return Color; } }
    }
}
