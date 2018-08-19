using ScintillaNET;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace hCpp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Editor> editors = new ObservableCollection<Editor>();

        public MainWindow()
        {
            InitializeComponent();
            editors.Add(new Editor { FileName = "test1", EditorInstance = new CppEditorBuilder().UseHighlight().UseLineNumber().UseCodeFolding().UseBreakpoints().Build() });
            editors.Add(new Editor { FileName = "test2", EditorInstance = new CppEditorBuilder().UseHighlight().UseLineNumber().UseCodeFolding().UseBreakpoints().Build() });
            editors.Add(new Editor { FileName = "test3", EditorInstance = new CppEditorBuilder().UseHighlight().UseLineNumber().UseCodeFolding().UseBreakpoints().Build() });
            Editors.ItemsSource = editors;
        }

        private WindowsFormsHost GetEditorHost(DependencyObject root)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var t = VisualTreeHelper.GetChild(root, i);
                if (t is WindowsFormsHost w) return w;
                else
                {
                    var r = GetEditorHost(t);
                    if (r != null) return r;
                }
            }
            return null;
        }

        private void Editors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl t && t.SelectedItem is Editor editor)
            {
                var host = GetEditorHost(Editors as DependencyObject);
                if (host != null) host.Child = editor.EditorInstance;
                return;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Height > 80) EditorPanel.Height = Math.Max(Height, ActualHeight) - 80;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Editors_SelectionChanged(Editors, null);
        }
    }

    public class Editor : INotifyPropertyChanged
    {
        public int FileType { get; set; }
        public ImageSource FileTypeImage => null;
        public string FileName { get; set; }
        private bool _isSaved;
        public bool IsSaved
        {
            get => _isSaved;
            set
            {
                _isSaved = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SaveMark)));
            }
        }
        public string SaveMark => IsSaved ? null : "*";
        
        public Scintilla EditorInstance { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
