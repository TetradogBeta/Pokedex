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
using Microsoft.Win32;
using Gabriel.Cat.Extension;
using System.ComponentModel;

namespace Pokedex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FrameWorkPokemonGBA.RomPokemon rom;
        FrameWorkPokemonGBA.Pokemon pokemonActual;
        bool hayCambios;
        public MainWindow()
        {
            hayCambios = false;
            InitializeComponent();
            PideRom();
            Closed += GuardaRom;
        }

        private void GuardaRom(object sender, EventArgs e)
        {
            if(hayCambios)
            if (MessageBox.Show("Desea guardar los cambios en la rom? ", "Importante", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                GuardaDatosPokemon();
                rom.Save();
            }
        }

        private void PideRom()
        {
            OpenFileDialog opnRom = new OpenFileDialog();
            PokemonPokedex pokemon;
            opnRom.Filter = "gba|*.gba";
            if(opnRom.ShowDialog().Value)
            {
                rom = new FrameWorkPokemonGBA.RomPokemon(opnRom.FileName);
                if (!rom.EsCompatiblePokedex)
                    MessageBox.Show("La rom no es compatible con el programa");
                else
                {
                    ugPokedex.Children.Clear();
                    try
                    {
                        for (int i = 0, f = rom.Pokedex.Total; i < f; i++)
                        {
                            pokemon = new PokemonPokedex(rom.Pokedex[i]);
                            pokemon.Selected += PonPokemon;
                            ugPokedex.Children.Add(pokemon);
                        }
                    }
                    catch { }
                }
            } 
        }

        private void PonPokemon(object sender, EventArgs e)
        {
            PokemonPokedex pokemonPokedex = sender as PokemonPokedex;
            GuardaDatosPokemon();
            pokemonActual = pokemonPokedex.Pokemon;
            txtNamePokemon.Text = pokemonActual.Nombre;
            imgPokemonPokedex.SetImage(pokemonActual.ImgFrontal.ToBitmap());
        }

        private void GuardaDatosPokemon()
        {
            if(pokemonActual!=null)
            {
                pokemonActual.Nombre = txtNamePokemon.Text;
                //poner todos los datos!!
            }
        }

        private void txtNamePokemon_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNamePokemon.Text.Length > (int)FrameWorkPokemonGBA.Pokemon.LongitudCampos.Nombre)
            {
                e.Handled = true;
                MessageBox.Show("Se ha pasado del máximo de caracteres para el nombre!!");
            }
            else
            {
                txtNamePokemon.Text = txtNamePokemon.Text.ToUpper();
                txtNamePokemon.CaretIndex = txtNamePokemon.Text.Length;
                hayCambios = true;//poner en todos los sitios ;)

            }
        }

        private void rbt_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
