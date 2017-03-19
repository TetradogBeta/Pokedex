using Gabriel.Cat.Extension;
using PokemonGBAFrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pokedex
{
    /// <summary>
    /// Interaction logic for ObjetoViewer.xaml
    /// </summary>
    public partial class ObjetoViewer : UserControl
    {
        Objeto obj;
        public ObjetoViewer()
        {
            InitializeComponent();
        }
        public ObjetoViewer(Objeto obj) : this()
        {
            Objeto = obj;
        }

        public Objeto Objeto
        {
            get
            {
                return obj;
            }

            set
            {
                obj = value;
                txtNombreObjeto.Text = obj.Nombre;
                if(obj.Sprite!=null)
                   imgObjeto.SetImage(obj.Sprite);
            }
        }
    }
}
