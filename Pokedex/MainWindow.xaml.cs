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
using System.Drawing;

namespace Pokedex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FrameWorkPokemonGBA.RomPokemon rom;
        PokemonPokedex pokemonActual;
        System.Drawing.Color colorSelected;
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
            
            GuardaDatosPokemon();
            pokemonActual = sender as PokemonPokedex;
            txtNamePokemon.Text = pokemonActual.Pokemon.Nombre;
            rbt_Checked(null, null);
            imgPokemonPokedex.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap());
            pltNormal.Colors = pokemonActual.Pokemon.ImgFrontal.Paleta;
            pltShiny.Colors = pokemonActual.Pokemon.ImgFrontalShiny.Paleta;
        }

        private void GuardaDatosPokemon()
        {
            if(pokemonActual!=null)
            {
                pokemonActual.Pokemon.Nombre = txtNamePokemon.Text;
                //poner todos los datos!!
                pokemonActual.Pokemon.ImgFrontal.Paleta = pltNormal.Colors;
                pokemonActual.Pokemon.ImgFrontalShiny.Paleta = pltShiny.Colors;
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
            Bitmap bmpImg;
            if (pokemonActual != null)
            {
                if (rbtNormal.IsChecked.Value)
                {
                    bmpImg = pokemonActual.Pokemon.ImgFrontal.ToBitmap();
                    imgPokemonPokedex.SetImage(bmpImg);
                    imgFrontal.SetImage(bmpImg);
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTrasera.ToBitmap());
                }
                else
                {
                    imgFrontal.SetImage(pokemonActual.Pokemon.ImgFrontalShiny.ToBitmap());
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTraseraShiny.ToBitmap());
                }
            }
        }

        private void plt_ColorChanged(object sender, Gabriel.Cat.Wpf.ColorChangedArgs e)
        {
            if(pltNormal==sender)
            {
                if (rbtNormal.IsChecked.Value)
                {
                    pokemonActual.imgPokemon.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap(pltNormal.Colors));
                    imgPokemonPokedex.Source = pokemonActual.imgPokemon.Source;
                    imgFrontal.Source = pokemonActual.imgPokemon.Source;
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTrasera.ToBitmap(pltNormal.Colors));
                }

            }
            else
            {
                if (!rbtNormal.IsChecked.Value)
                {
                    imgFrontal.SetImage(pokemonActual.Pokemon.ImgFrontalShiny.ToBitmap(pltShiny.Colors));
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTraseraShiny.ToBitmap(pltShiny.Colors));
                }

            }
            hayCambios = true;

        }

        private void plt_ColorSelected(object sender, Gabriel.Cat.Wpf.ColorSelectedArgs e)
        {
            colorSelected = e.Color;
        }
    }
}
