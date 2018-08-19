using ScintillaNET;
using System;
using System.Drawing;

namespace hCpp
{
    public class CppEditorBuilder : IEditorBuilder
    {
        private readonly Scintilla _scintilla = new Scintilla();
        private const int NUMBER_MARGIN = 1;
        private const int BOOKMARK_MARGIN = 2;
        private const int BOOKMARK_MARKER = 2;
        private const int FOLDING_MARGIN = 3;
        private const bool CODEFOLDING_CIRCULAR = true;

        public CppEditorBuilder()
        {
            _scintilla.StyleResetDefault();
            _scintilla.Styles[Style.Default].Font = "Consolas";
            _scintilla.Styles[Style.Default].Size = 10;
            _scintilla.StyleClearAll();
        }

        public Scintilla Build() => _scintilla;

        public CppEditorBuilder UseHighlight()
        {
            _scintilla.Styles[Style.Cpp.Default].ForeColor = Color.Silver;
            _scintilla.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            _scintilla.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
            _scintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
            _scintilla.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            _scintilla.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            _scintilla.Styles[Style.Cpp.Word2].ForeColor = Color.Blue;
            _scintilla.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
            _scintilla.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
            _scintilla.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
            _scintilla.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
            _scintilla.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
            _scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;
            _scintilla.Lexer = Lexer.Cpp;
            return this;
        }

        public CppEditorBuilder UseKeywords()
        {
            _scintilla.SetKeywords(0, "abstract as base break case catch checked continue default delegate do else event explicit extern false finally fixed for foreach goto if implicit in interface internal is lock namespace new null object operator out override params private protected public readonly ref return sealed sizeof stackalloc switch this throw true try typeof unchecked unsafe using virtual while");
            _scintilla.SetKeywords(1, "bool byte char class const decimal double enum float int long sbyte short static string struct uint ulong ushort void");
            return this;
        }

        public CppEditorBuilder UseCodeFolding()
        {
            _scintilla.SetFoldMarginColor(true, Color.LightGray);
            _scintilla.SetFoldMarginHighlightColor(true, Color.LightGray);
            // Enable code folding
            _scintilla.SetProperty("fold", "1");
            _scintilla.SetProperty("fold.compact", "1");
            // Configure a margin to display folding symbols
            _scintilla.Margins[FOLDING_MARGIN].Type = MarginType.Symbol;
            _scintilla.Margins[FOLDING_MARGIN].Mask = Marker.MaskFolders;
            _scintilla.Margins[FOLDING_MARGIN].Sensitive = true;
            _scintilla.Margins[FOLDING_MARGIN].Width = 20;
            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                _scintilla.Markers[i].SetForeColor(Color.LightGray); // styles for [+] and [-]
                _scintilla.Markers[i].SetBackColor(Color.Black); // styles for [+] and [-]
            }
            // Configure folding markers with respective symbols
            _scintilla.Markers[Marker.Folder].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlus : MarkerSymbol.BoxPlus;
            _scintilla.Markers[Marker.FolderOpen].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinus : MarkerSymbol.BoxMinus;
            _scintilla.Markers[Marker.FolderEnd].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CirclePlusConnected : MarkerSymbol.BoxPlusConnected;
            _scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            _scintilla.Markers[Marker.FolderOpenMid].Symbol = CODEFOLDING_CIRCULAR ? MarkerSymbol.CircleMinusConnected : MarkerSymbol.BoxMinusConnected;
            _scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            _scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;
            // Enable automatic folding
            _scintilla.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);
            return this;
        }

        public CppEditorBuilder UseLineNumber()
        {
            _scintilla.Styles[Style.LineNumber].BackColor = Color.LightGray;
            _scintilla.Styles[Style.LineNumber].ForeColor = Color.Black;
            _scintilla.Styles[Style.IndentGuide].ForeColor = Color.Black;
            _scintilla.Styles[Style.IndentGuide].BackColor = Color.LightGray;

            var nums = _scintilla.Margins[NUMBER_MARGIN];
            nums.Width = (1 + (int)Math.Floor(Math.Log10(_scintilla.Lines.Count))) * (16 + _scintilla.Zoom) + 2 * (1 + Math.Max(0, _scintilla.Zoom));
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

            _scintilla.KeyPress += AdjustLineNumberWidth;
            _scintilla.MarginClick += AddBreakpoint;
            _scintilla.ZoomChanged += AdjustLineNumberWidth;
            return this;
        }

        private void AdjustLineNumberWidth(object sender, EventArgs e)
        {
            _scintilla.Margins[NUMBER_MARGIN].Width = (1 + (int)Math.Floor(Math.Log10(_scintilla.Lines.Count))) * (16 + _scintilla.Zoom) + 2 * (1 + Math.Max(0, _scintilla.Zoom));
        }

        private void AddBreakpoint(object sender, MarginClickEventArgs e)
        {
            if (e.Margin == BOOKMARK_MARGIN)
            {
                const uint mask = (1 << BOOKMARK_MARKER);

                var line = _scintilla.Lines[_scintilla.LineFromPosition(e.Position)];

                if ((line.MarkerGet() & mask) > 0)
                {
                    line.MarkerDelete(BOOKMARK_MARKER);
                }
                else
                {
                    line.MarkerAdd(BOOKMARK_MARKER);
                }
            }
        }

        private void AdjustLineNumberWidth(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            _scintilla.Margins[NUMBER_MARGIN].Width = (1 + (int)Math.Floor(Math.Log10(_scintilla.Lines.Count))) * (16 + _scintilla.Zoom) + 2 * (1 + Math.Max(0, _scintilla.Zoom));
        }

        public CppEditorBuilder UseBreakpoints()
        {
            var margin = _scintilla.Margins[BOOKMARK_MARGIN];

            margin.Width = 20;
            margin.Sensitive = true;
            margin.Type = MarginType.Symbol;
            margin.Mask = (1 << BOOKMARK_MARKER);

            var marker = _scintilla.Markers[BOOKMARK_MARKER];
            marker.Symbol = MarkerSymbol.Circle;
            marker.SetBackColor(Color.Red);
            marker.SetForeColor(Color.Red);

            return this;
        }
    }
}
