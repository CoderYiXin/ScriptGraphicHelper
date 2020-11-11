using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace ScriptGraphicHelper.Models
{
    public class MovieCategory : BindableBase
    {
        private int _Hwnd;
        public int Hwnd
        {
            get { return _Hwnd; }
            set { SetProperty(ref _Hwnd, value); }
        }
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value); }
        }
        private string _ClassName;
        public string ClassName
        {
            get { return _ClassName; }
            set { SetProperty(ref _ClassName, value); }
        }
        private string _Info;
        public string Info
        {
            get { return _Info; }
            set { SetProperty(ref _Info, value); }
        }
        public ObservableCollection<MovieCategory> Movies { get; set; } = new ObservableCollection<MovieCategory>();
        public MovieCategory(int hwnd, string title, string className, params MovieCategory[] movies)
        {
            Hwnd = hwnd;
            Title = title;
            ClassName = className;
            Info = string.Format("[{0}][{1}][{2}]", hwnd, title, className);
            Movies = new ObservableCollection<MovieCategory>(movies);
        }
        public MovieCategory(int hwnd, string title, string className)
        {
            Hwnd = hwnd;
            Title = title;
            ClassName = className;
            Info = string.Format("[{0}][{1}][{2}]", hwnd, title, className);
        }
        public MovieCategory()
        {
            Hwnd = -1;
            Title = "";
            ClassName = "";
            Info = string.Format("[{0}][{1}][{2}]", Hwnd, Title, ClassName);
            Movies = new ObservableCollection<MovieCategory>();
        }
    }

}
